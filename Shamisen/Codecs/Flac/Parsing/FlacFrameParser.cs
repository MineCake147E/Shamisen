using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Codecs.Flac.SubFrames;
using Shamisen.Data;
using Shamisen.Data.Binary;
using Shamisen.Utils;

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Parses FLAC frames.
    /// </summary>
    public sealed partial class FlacFrameParser : IReadableAudioSource<int, Int32LinearPcmSampleFormat>
    {
        private uint bitDepth = 0;

        private FlacChannelAssignments channels = 0;
        private Int32Divisor channelsDivisor;

        private FlacCrc16 crc16 = new(0);

        private bool disposedValue;

        private ulong? frameNumber = null;

        private ulong? sampleNumber = null;

        private uint sampleRate = 0;

        private PooledArray<int>? samples;

        private IFlacSubFrame[]? subFrames;

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public Int32LinearPcmSampleFormat Format { get; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </value>
        public ulong? Length => TotalLength - Position;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Position { get; private set; } = 0;

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport { get; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public FlacBitReader Source { get; }

        /// <summary>
        /// Gets the stream information block.
        /// </summary>
        /// <value>
        /// The stream information block.
        /// </value>
        public FlacStreamInfoBlock StreamInfoBlock { get; }

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength { get; private set; } = 0;

        private static ReadOnlySpan<(uint bitDepth, BitDepthState state)> BitDepthTable => new (uint bitDepth, BitDepthState state)[8]
               {
            (0, BitDepthState.RespectStreamInfo),
            (8, BitDepthState.Value),
            (12, BitDepthState.Value),
            (0, BitDepthState.Reserved),
            (16, BitDepthState.Value),
            (20, BitDepthState.Value),
            (24, BitDepthState.Value),
            (0, BitDepthState.Reserved),
               };

        private static ReadOnlySpan<uint> SampleRateTable => new uint[]
                {
            88200,
            176400,
            192000,
            8000,
            16000,
            22050,
            24000,
            32000,
            44100,
            48000,
            96000
                };

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacFrameParser"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="streamInfoBlock">The stream information block.</param>
        public FlacFrameParser(FlacBitReader source, FlacStreamInfoBlock streamInfoBlock)
        {
            Source = source;
            StreamInfoBlock = streamInfoBlock;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <returns></returns>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finds the next frame.
        /// </summary>
        /// <returns></returns>
        public static FlacFrameParser? ParseNextFrame(FlacBitReader source, FlacStreamInfoBlock streamInfoBlock)
        {
            ushort q = default;
            Span<byte> rawHeader = stackalloc byte[16];
            byte? lookahead = null;
            //Find next byte-aligned frame sync code.
            while (true)
            {
                while (MathI.ExtractBitField(q, 2, 14) != 0b1111_1111_1111_10u)
                {
                    if (lookahead is { } la)
                    {
                        q <<= 8;
                        q |= la;
                        lookahead = null;
                    }
                    else
                    {
                        q <<= 8;
                        if (!source.ReadByte(out var tt)) return null;
                        q |= tt;
                    }
                }
                Unsafe.As<byte, ushort>(ref rawHeader[0]) = BinaryExtensions.ConvertToBigEndian(q);
                source.Crc16 = new FlacCrc16(0) * rawHeader.SliceWhile(2);
                uint? v1 = source.ReadBitsUInt32(16);
                if (v1 is null) return null;
                ushort next16 = (ushort)v1;//Contains blockSize to Reserved before CRC
                var nFrameSize = ParseBlockSize((byte)MathI.ExtractBitField(next16, 12, 4));
                var (length, state) = nFrameSize;
                var nSampleRate = ParseSampleRate((byte)MathI.ExtractBitField(next16, 8, 4));
                var nChannels = (FlacChannelAssignments)MathI.ExtractBitField(next16, 4, 4);
                var nBitDepth = ParseBitDepth((byte)MathI.ExtractBitField(next16, 1, 3));
                Unsafe.As<byte, ushort>(ref rawHeader[2]) = BinaryExtensions.ConvertToBigEndian(next16);
                int bytesRead = 4;
                uint bitDepth = 0;

                FlacChannelAssignments channels = 0;
                Int32Divisor channelsDivisor;

                FlacCrc16 crc16 = new(0);

                ulong? frameNumber = null;

                ulong? sampleNumber = null;

                uint sampleRate = 0;

                PooledArray<int>? samples;

                if ((q & 0b1) > 0)   //variable blocking
                {
                    var sn = source.ReadUtf8UInt64(out var snum, rawHeader.Slice(bytesRead), out int a);
                    bytesRead += a;
                    if (sn)
                    {
                        sampleNumber = snum;
                    }
                    else
                    {
                        lookahead = rawHeader[bytesRead - 1];
                        continue;
                    }
                }
                else
                {
                    var fn = source.ReadUtf8UInt32(out var fnum, rawHeader.Slice(bytesRead), out int a);
                    bytesRead += a;
                    if (fn)
                    {
                        frameNumber = fnum;
                    }
                    else
                    {
                        lookahead = rawHeader[bytesRead - 1];
                        continue;
                    }
                }
                switch (state)
                {
                    case BlockSizeState.GetByteFromEnd:
                        if (!source.ReadByte(out var v))
                        {
                            return null;
                        }
                        rawHeader[bytesRead++] = v;
                        length = v + 1u;
                        break;
                    case BlockSizeState.GetUInt16FromEnd:
                        ushort v2 = (ushort?)source.ReadBitsUInt32(16) ?? throw new FlacException("The decoder ran out of data!", source);
                        Unsafe.As<byte, ushort>(ref rawHeader[bytesRead]) = BinaryExtensions.ConvertToBigEndian(v2);
                        bytesRead += 2;
                        length = v2 + 1u;
                        break;
                    default:
                        break;
                }
                switch (nSampleRate.state)
                {
                    case SampleRateState.RespectStreamInfo:
                        nSampleRate.sampleRate = streamInfoBlock.SampleRate;
                        break;
                    case SampleRateState.GetByteKHzFromEnd:
                        if (!source.ReadByte(out var v))
                        {
                            throw new FlacException("The decoder ran out of data!", source);
                        }
                        rawHeader[bytesRead++] = v;
                        nSampleRate.sampleRate = v * 1000u;
                        break;
                    case SampleRateState.GetUInt16HzFromEnd:
                        ushort v2 = (ushort?)source.ReadBitsUInt32(16) ?? throw new FlacException("The decoder ran out of data!", source);
                        Unsafe.As<byte, ushort>(ref rawHeader[bytesRead]) = BinaryExtensions.ConvertToBigEndian(v2);
                        bytesRead += 2;
                        nSampleRate.sampleRate = v2;
                        break;
                    case SampleRateState.GetUInt16TenHzFromEnd:
                        ushort v3 = (ushort?)source.ReadBitsUInt32(16) ?? throw new FlacException("The decoder ran out of data!", source);
                        Unsafe.As<byte, ushort>(ref rawHeader[bytesRead]) = BinaryExtensions.ConvertToBigEndian(v3);
                        bytesRead += 2;
                        nSampleRate.sampleRate = v3 * 10u;
                        break;
                    case SampleRateState.Value:
                        break;
                    default:
                        lookahead = rawHeader[bytesRead - 1];
                        continue;
                }
                if (!source.ReadByte(out var expectedCrc))
                {
                    return null;
                }
                rawHeader[bytesRead++] = expectedCrc;
                if (expectedCrc != new FlacCrc8(0) * rawHeader.SliceWhile(bytesRead - 1))
                {
                    lookahead = rawHeader[bytesRead - 1];
                    continue;
                }
                sampleRate = nSampleRate.sampleRate;
                channels = nChannels;
                bitDepth = nBitDepth.bitDepth;
                //Read sub frame
                int chCount = nChannels.GetChannels();
                var subFrames = new IFlacSubFrame[chCount];
                var bitReader = source;
                for (int ch = 0; ch < subFrames.Length; ch++)
                {
                    int bps = (int)nBitDepth.bitDepth;
                    switch ((channels, ch))
                    {
                        case (FlacChannelAssignments.LeftAndDifference, 1):
                        case (FlacChannelAssignments.RightAndDifference, 0):
                        case (FlacChannelAssignments.CenterAndDifference, 1):
                            bps++;
                            break;
                        default:
                            break;
                    }
                    var result = ReadSubFrame(bitReader, (int)length, bps);
                    if (result is null)
                    {
                        break;
                    }
                    subFrames[ch] = result;
                }
                if (subFrames.Any(a => a is null))
                {
                    lookahead = rawHeader[bytesRead - 1];
                    continue;
                }
                if (!source.ReadZeroPadding())
                {
                    return null;
                }
                source.UpdateCrc16();
                var frameCrc = source.Crc16;
                if (!(source.ReadBitsUInt32(16) is { } expectedCrc16))
                {
                    throw new FlacException("The decoder ran out of data!", source);
                }
                if (expectedCrc16 != frameCrc)
                {
                    throw new FlacException($"The decoder has detected CRC-16 mismatch!\nExpected: {expectedCrc16}\nActual:{frameCrc}", source);
                }
                if (subFrames is null) throw new FlacException("The subFrames is null! This is a bug!", source);
                samples = new((int)length * chCount);
                var TotalLength = length;
                channelsDivisor = new(chCount);
                InterleaveChannels(length, nChannels, samples.Span, subFrames);

                var p = new FlacFrameParser(source, streamInfoBlock)
                {
                    subFrames = subFrames,
                    channels = channels,
                    channelsDivisor = channelsDivisor,
                    bitDepth = bitDepth,
                    crc16 = crc16,
                    sampleRate = sampleRate,
                    sampleNumber = sampleNumber,
                    samples = samples,
                    frameNumber = frameNumber,
                    TotalLength = TotalLength
                };
                return p;
            }
        }

        internal static IFlacSubFrame? ReadSubFrame(FlacBitReader flacBitReader, int blockSize, int bitDepthToRead)
        {
            var (hasValue, result) = flacBitReader.ReadBitsUInt64(8);

            if (!hasValue) return null;
            int wasted = (int)result & 1;
            result &= 0xfe;
            if (wasted > 0)
            {
                var q = flacBitReader.ReadUnaryUnsigned(out var value);
                if (!q) return null;
                wasted = (int)value + 1;
                bitDepthToRead -= wasted;
            }
#pragma warning disable S907 // "goto" statement should not be used
            switch (result >> 1)
            {
                case 0: //CONSTANT
                    return FlacConstantSubFrame.ReadFrame(flacBitReader, wasted, (byte)bitDepthToRead);
                case 1: //VERBATIM
                    return new FlacVerbatimSubFrame(flacBitReader, blockSize, wasted, (byte)bitDepthToRead);
                case < 0b1000:
                    goto default;
                case < 0b1101:  //FIXED
                    return new FlacFixedPredictionSubFrame(flacBitReader, blockSize, wasted, (byte)bitDepthToRead, (byte)(result >> 1));
                case < 0b100000:
                    goto default;
                case <= 0b111111:   //LPC
                    return new FlacLinearPredictionSubFrame(flacBitReader, blockSize, wasted, (byte)bitDepthToRead, (byte)(result >> 1));
                default:
                    return null;
            }
#pragma warning restore S907 // "goto" statement should not be used
        }

        private static void InterleaveChannels(uint length, FlacChannelAssignments nChannels, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            switch ((nChannels, subFrames.Length))
            {
                case (FlacChannelAssignments.Monaural, 1):
                    var rr1 = subFrames[0].Read(samples);
                    if (rr1.HasNoData)
                    {
                        throw new FlacException("Unknown error! This is a bug!");
                    }
                    break;
                case (FlacChannelAssignments.OrdinalStereo, 2):
                    {
                        using var left = new PooledArray<int>((int)length);
                        using var right = new PooledArray<int>((int)length);
                        if (subFrames[0].Read(left.Span).HasNoData || subFrames[1].Read(right.Span).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        AudioUtils.InterleaveStereo(samples, left.Span, right.Span);
                    }
                    break;
                case (FlacChannelAssignments.FrontThree, 3):
                    {
                        int iLen = (int)length;
                        using var src = new PooledArray<int>(iLen * 3);
                        var j = src.Span;
                        if (subFrames[0].Read(j.Slice(0, iLen)).HasNoData || subFrames[1].Read(j.Slice(iLen, iLen)).HasNoData || subFrames[2].Read(j.Slice(iLen * 2, iLen)).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        AudioUtils.InterleaveThree(samples, j.Slice(0, iLen), j.Slice(iLen, iLen), j.Slice(iLen * 2, iLen));
                    }
                    break;
                case (FlacChannelAssignments.Quad, 4):
                    {
                        int iLen = (int)length;
                        using var src = new PooledArray<int>(iLen * 4);
                        var j = src.Span;
                        if (subFrames[0].Read(j.Slice(0, iLen)).HasNoData ||
                            subFrames[1].Read(j.Slice(iLen, iLen)).HasNoData ||
                            subFrames[2].Read(j.Slice(iLen * 2, iLen)).HasNoData ||
                            subFrames[3].Read(j.Slice(iLen * 3, iLen)).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        AudioUtils.InterleaveQuad(samples, j.Slice(0, iLen), j.Slice(iLen, iLen), j.Slice(iLen * 2, iLen), j.Slice(iLen * 3, iLen));
                    }
                    break;
                case (FlacChannelAssignments.FrontFive, 5):
                    {
                        int iLen = (int)length;
                        using var src = new PooledArray<int>(iLen * 5);
                        var j = src.Span;
                        if (subFrames[0].Read(j.Slice(0, iLen)).HasNoData ||
                            subFrames[1].Read(j.Slice(iLen, iLen)).HasNoData ||
                            subFrames[2].Read(j.Slice(iLen * 2, iLen)).HasNoData ||
                            subFrames[3].Read(j.Slice(iLen * 3, iLen)).HasNoData ||
                            subFrames[4].Read(j.Slice(iLen * 4, iLen)).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        AudioUtils.Interleave5Channels(samples, j.Slice(0, iLen), j.Slice(iLen, iLen), j.Slice(iLen * 2, iLen)
                            , j.Slice(iLen * 3, iLen), j.Slice(iLen * 4, iLen));
                    }
                    break;
                case (FlacChannelAssignments.FivePointOne, 6):
                    {
                        int iLen = (int)length;
                        using var src = new PooledArray<int>(iLen * 6);
                        var j = src.Span;
                        if (subFrames[0].Read(j.Slice(0, iLen)).HasNoData ||
                            subFrames[1].Read(j.Slice(iLen, iLen)).HasNoData ||
                            subFrames[2].Read(j.Slice(iLen * 2, iLen)).HasNoData ||
                            subFrames[3].Read(j.Slice(iLen * 3, iLen)).HasNoData ||
                            subFrames[4].Read(j.Slice(iLen * 4, iLen)).HasNoData ||
                            subFrames[5].Read(j.Slice(iLen * 5, iLen)).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        AudioUtils.Interleave6Channels(samples, j.Slice(0, iLen), j.Slice(iLen, iLen), j.Slice(iLen * 2, iLen)
                            , j.Slice(iLen * 3, iLen), j.Slice(iLen * 4, iLen), j.Slice(iLen * 5, iLen));
                    }
                    break;
                case (FlacChannelAssignments.DolbySixPointOne, 7):
                    {
                        int iLen = (int)length;
                        using var src = new PooledArray<int>(iLen * 7);
                        var j = src.Span;
                        if (subFrames[0].Read(j.Slice(0, iLen)).HasNoData ||
                            subFrames[1].Read(j.Slice(iLen, iLen)).HasNoData ||
                            subFrames[2].Read(j.Slice(iLen * 2, iLen)).HasNoData ||
                            subFrames[3].Read(j.Slice(iLen * 3, iLen)).HasNoData ||
                            subFrames[4].Read(j.Slice(iLen * 4, iLen)).HasNoData ||
                            subFrames[5].Read(j.Slice(iLen * 5, iLen)).HasNoData ||
                            subFrames[6].Read(j.Slice(iLen * 6, iLen)).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        AudioUtils.Interleave7Channels(samples, j.Slice(0, iLen), j.Slice(iLen, iLen), j.Slice(iLen * 2, iLen)
                            , j.Slice(iLen * 3, iLen), j.Slice(iLen * 4, iLen), j.Slice(iLen * 5, iLen)
                            , j.Slice(iLen * 6, iLen));
                    }
                    break;
                case (FlacChannelAssignments.SevenPointOne, 8):
                    {
                        int iLen = (int)length;
                        using var src = new PooledArray<int>(iLen * 8);
                        var j = src.Span;
                        if (subFrames[0].Read(j.Slice(0, iLen)).HasNoData ||
                            subFrames[1].Read(j.Slice(iLen, iLen)).HasNoData ||
                            subFrames[2].Read(j.Slice(iLen * 2, iLen)).HasNoData ||
                            subFrames[3].Read(j.Slice(iLen * 3, iLen)).HasNoData ||
                            subFrames[4].Read(j.Slice(iLen * 4, iLen)).HasNoData ||
                            subFrames[5].Read(j.Slice(iLen * 5, iLen)).HasNoData ||
                            subFrames[6].Read(j.Slice(iLen * 6, iLen)).HasNoData ||
                            subFrames[7].Read(j.Slice(iLen * 7, iLen)).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        AudioUtils.Interleave8Channels(samples, j.Slice(0, iLen), j.Slice(iLen, iLen), j.Slice(iLen * 2, iLen)
                            , j.Slice(iLen * 3, iLen), j.Slice(iLen * 4, iLen), j.Slice(iLen * 5, iLen)
                            , j.Slice(iLen * 6, iLen), j.Slice(iLen * 7, iLen));
                    }
                    break;
                case (FlacChannelAssignments.LeftAndDifference, 2):
                    {
                        using var left = new PooledArray<int>((int)length);
                        using var right = new PooledArray<int>((int)length);
                        if (subFrames[0].Read(left.Span).HasNoData || subFrames[1].Read(right.Span).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        FlacSideStereoUtils.DecodeAndInterleaveLeftSideStereo(samples, left.Span, right.Span);
                    }
                    break;
                case (FlacChannelAssignments.RightAndDifference, 2):
                    {
                        using var left = new PooledArray<int>((int)length);
                        using var right = new PooledArray<int>((int)length);
                        if (subFrames[0].Read(left.Span).HasNoData || subFrames[1].Read(right.Span).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        FlacSideStereoUtils.DecodeAndInterleaveRightSideStereo(samples, left.Span, right.Span);
                    }
                    break;
                case (FlacChannelAssignments.CenterAndDifference, 2):
                    {
                        using var left = new PooledArray<int>((int)length);
                        using var right = new PooledArray<int>((int)length);
                        if (subFrames[0].Read(left.Span).HasNoData || subFrames[1].Read(right.Span).HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        FlacSideStereoUtils.DecodeAndInterleaveMidSideStereo(samples, left.Span, right.Span);
                    }
                    break;
                default:
                    throw new FlacException("The channel assignment is not supported!");
            }
        }

        /// <summary>
        /// Reads the data to the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadResult Read(Span<int> buffer)
        {
            if (Length == 0 || samples is null) return ReadResult.EndOfStream;
            if (Position is null || Length is null) throw new InvalidProgramException();
            if ((ulong)buffer.Length <= Length)
            {
                samples.Span.Slice((int)Position * channelsDivisor.Divisor, buffer.Length).CopyTo(buffer);
                Position += (ulong)(buffer.Length / channelsDivisor);
                return buffer.Length;
            }
            else
            {
                samples.Span.Slice((int)Position * channelsDivisor.Divisor).CopyTo(buffer);
                int length = (int)Length;
                Position += Length;
                return length * channelsDivisor.Divisor;
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static (uint bitDepth, BitDepthState state) ParseBitDepth(byte value)
            => Unsafe.Add(ref MemoryMarshal.GetReference(BitDepthTable), value & 0x7);

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static (uint length, BlockSizeState state) ParseBlockSize(byte value)
            => value switch
            {
                //Reserved
                0 => (0, BlockSizeState.Reserved),
                1 => (192, BlockSizeState.Value),
                <= 0b101 => (576u << (value - 2), BlockSizeState.Value),
                0b0110 => (0, BlockSizeState.GetByteFromEnd),
                0b0111 => (0, BlockSizeState.GetUInt16FromEnd),
                _ => (256u << (value - 8), BlockSizeState.Value),
            };

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static (uint sampleRate, SampleRateState state) ParseSampleRate(byte value)
            => (value & 0xf) switch
            {
                0 => (0, SampleRateState.RespectStreamInfo),
                <= 0b1011 => (Unsafe.Add(ref MemoryMarshal.GetReference(SampleRateTable), value - 1), SampleRateState.Value),
                0b1100 => (0, SampleRateState.GetByteKHzFromEnd),
                0b1101 => (0, SampleRateState.GetUInt16HzFromEnd),
                0b1110 => (0, SampleRateState.GetUInt16TenHzFromEnd),
                _ => (0, SampleRateState.SyncFooled),
            };

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                samples?.Dispose();
                samples = null;
                disposedValue = true;
            }
        }
    }
}
