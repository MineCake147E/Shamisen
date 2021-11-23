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
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public static FlacFrameParser? ParseNextFrame(FlacBitReader source, FlacStreamInfoBlock streamInfoBlock)
        {
#pragma warning disable S907 // "goto" statement should not be used
            ushort q = default;
            Span<byte> rawHeader = stackalloc byte[16];
            ref var rH = ref MemoryMarshal.GetReference(rawHeader);
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
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rH, 0)) = BinaryExtensions.ConvertToBigEndian(q);
                source.Crc16 = new FlacCrc16(0) * rawHeader.SliceWhile(2);
                if (!source.ReadBitsUInt32(16, out var v1)) return null;
                var next16 = (ushort)v1;//Contains blockSize to Reserved before CRC
                var nFrameSize = ParseBlockSize((byte)MathI.ExtractBitField(next16, 12, 4));
                var (length, state) = nFrameSize;
                var nSampleRate = ParseSampleRate((byte)MathI.ExtractBitField(next16, 8, 4));
                var nChannels = (FlacChannelAssignments)MathI.ExtractBitField(next16, 4, 4);
                var nBitDepth = ParseBitDepth((byte)MathI.ExtractBitField(next16, 1, 3));
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rH, 2)) = BinaryExtensions.ConvertToBigEndian(next16);
                var bytesRead = 4;
                FlacCrc16 pCrc16 = new(0);
                ulong? pFrameNumber = null;
                ulong? pSampleNumber = null;
                if ((q & 0b1) > 0)   //variable blocking
                {
                    var sn = source.ReadUtf8UInt64(out var snum, rawHeader.Slice(bytesRead), out var a);
                    bytesRead += a;
                    if (sn)
                    {
                        pSampleNumber = snum;
                    }
                    else
                    {
                        goto tryAgain;
                    }
                }
                else
                {
                    var fn = source.ReadUtf8UInt32(out var fnum, rawHeader.Slice(bytesRead), out var a);
                    bytesRead += a;
                    if (fn)
                    {
                        pFrameNumber = fnum;
                    }
                    else
                    {
                        goto tryAgain;
                    }
                }
                if (state != BlockSizeState.Value)
                {
                    ParseVariableBlockSize(source, ref rH, ref length, state, ref bytesRead);
                }
                if (nSampleRate.state == SampleRateState.SyncFooled)
                {
                    goto tryAgain;
                }
                if (nSampleRate.state != SampleRateState.Value)
                {
                    ParseVariableSampleRate(source, streamInfoBlock, rawHeader, ref nSampleRate, ref bytesRead);
                }
                if (!source.ReadByte(out var expectedCrc))
                {
                    return null;
                }
                Unsafe.Add(ref rH, bytesRead++) = expectedCrc;
                if (expectedCrc != new FlacCrc8(0) * rawHeader.SliceWhile(bytesRead - 1))
                {
                    goto tryAgain;
                }
                var res = ParseAllSubFrames(source, streamInfoBlock, length, nSampleRate, nChannels, nBitDepth, pCrc16, pFrameNumber, pSampleNumber);
                if (res is not null)
                    return res;
                tryAgain:
                lookahead = Unsafe.Add(ref rH, bytesRead - 1);
            }
#pragma warning restore S907 // "goto" statement should not be used
        }

        /// <summary>
        /// Inlining this method causes poor codegen which looks like in debug mode.
        /// </summary>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static FlacFrameParser? ParseAllSubFrames(FlacBitReader source, FlacStreamInfoBlock streamInfoBlock, uint length, (uint sampleRate, SampleRateState state) nSampleRate, FlacChannelAssignments nChannels, (uint bitDepth, BitDepthState state) nBitDepth, FlacCrc16 pCrc16, ulong? pFrameNumber, ulong? pSampleNumber)
        {
            var pSampleRate = nSampleRate.sampleRate;
            var pChannels = nChannels;
            var pBitDepth = nBitDepth.bitDepth;
            //Read sub frame
            var chCount = nChannels.GetChannels();
            var pSubFrames = new IFlacSubFrame[chCount];
            var bitReader = source;
            ReadSubFrames(length, nBitDepth, pChannels, pSubFrames, bitReader);
            if (pSubFrames.Any(a => a is null))
            {
                return null;
            }
            if (!source.ReadZeroPadding())
            {
                throw new FlacException("The decoder ran out of data!", source);
            }
            source.UpdateCrc16();
            var frameCrc = source.Crc16;
            if (!source.ReadBitsUInt32(16, out var expectedCrc16))
            {
                throw new FlacException("The decoder ran out of data!", source);
            }
            if (expectedCrc16 != frameCrc)
            {
                throw new FlacException($"The decoder has detected CRC-16 mismatch!\nExpected: {expectedCrc16}\nActual:{frameCrc}", source);
            }
            if (pSubFrames is null) throw new FlacException("The subFrames is null! This is a bug!", source);
            PooledArray<int>? pSamples = new((int)length * chCount);
            var pTotalLength = length;
            Int32Divisor pChannelsDivisor = new(chCount);
            InterleaveChannels(length, nChannels, pSamples.Span, pSubFrames);

            var p = new FlacFrameParser(source, streamInfoBlock)
            {
                subFrames = pSubFrames,
                channels = pChannels,
                channelsDivisor = pChannelsDivisor,
                bitDepth = pBitDepth,
                crc16 = pCrc16,
                sampleRate = pSampleRate,
                sampleNumber = pSampleNumber,
                samples = pSamples,
                frameNumber = pFrameNumber,
                TotalLength = pTotalLength
            };
            return p;
        }

        private static void ParseVariableBlockSize(FlacBitReader source, ref byte rH, ref uint length, BlockSizeState state, ref int bytesRead)
        {
            switch (state)
            {
                case BlockSizeState.GetByteFromEnd:
                    if (!source.ReadByte(out var v))
                    {
                        throw new FlacException("The decoder ran out of data!", source);
                    }
                    Unsafe.Add(ref rH, bytesRead++) = v;
                    length = v + 1u;
                    break;
                case BlockSizeState.GetUInt16FromEnd:
                    var v2 = source.ReadBitsUInt32(16, out var vvv) ? (ushort)vvv : throw new FlacException("The decoder ran out of data!", source);
                    Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rH, bytesRead)) = BinaryExtensions.ConvertToBigEndian(v2);
                    bytesRead += 2;
                    length = v2 + 1u;
                    break;
                default:
                    break;
            }
        }

        private static void ParseVariableSampleRate(FlacBitReader source, FlacStreamInfoBlock streamInfoBlock, Span<byte> rawHeader, ref (uint sampleRate, SampleRateState state) nSampleRate, ref int bytesRead)
        {
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
                    var v2 = source.ReadBitsUInt32(16, out var vv0) ? (ushort)vv0 : throw new FlacException("The decoder ran out of data!", source);
                    Unsafe.As<byte, ushort>(ref rawHeader[bytesRead]) = BinaryExtensions.ConvertToBigEndian(v2);
                    bytesRead += 2;
                    nSampleRate.sampleRate = v2;
                    break;
                case SampleRateState.GetUInt16TenHzFromEnd:
                    var v3 = source.ReadBitsUInt32(16, out vv0) ? (ushort)vv0 : throw new FlacException("The decoder ran out of data!", source);
                    Unsafe.As<byte, ushort>(ref rawHeader[bytesRead]) = BinaryExtensions.ConvertToBigEndian(v3);
                    bytesRead += 2;
                    nSampleRate.sampleRate = v3 * 10u;
                    break;
                default:
                    break;
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static void ReadSubFrames(uint length, (uint bitDepth, BitDepthState state) nBitDepth, FlacChannelAssignments pChannels, IFlacSubFrame[] pSubFrames, FlacBitReader bitReader)
        {
            for (var ch = 0; ch < pSubFrames.Length; ch++)
            {
                var bps = (int)nBitDepth.bitDepth;
                switch ((pChannels, ch))
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
                pSubFrames[ch] = result;
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static IFlacSubFrame? ReadSubFrame(FlacBitReader flacBitReader, int blockSize, int bitDepthToRead)
        {
            if (!flacBitReader.ReadBitsUInt64(8, out var result)) return null;
            var wasted = (int)result & 1;
            result &= 0xfe;
            if (wasted > 0)
            {
                var q = flacBitReader.ReadUnaryUnsigned(out var value);
                if (!q) return null;
                wasted = (int)value + 1;
                bitDepthToRead -= wasted;
            }

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
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
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
                    InterleaveOrdinalStereo(length, samples, subFrames);
                    break;
                case (FlacChannelAssignments.FrontThree, 3):
                    InterleaveFrontThree(length, samples, subFrames);
                    break;
                case (FlacChannelAssignments.Quad, 4):
                    InterleaveQuad(length, samples, subFrames);
                    break;
                case (FlacChannelAssignments.FrontFive, 5):
                    InterleaveFrontFive(length, samples, subFrames);
                    break;
                case (FlacChannelAssignments.FivePointOne, 6):
                    InterleaveFivePointOne(length, samples, subFrames);
                    break;
                case (FlacChannelAssignments.DolbySixPointOne, 7):
                    InterleaveDolbySixPointOne(length, samples, subFrames);
                    break;
                case (FlacChannelAssignments.SevenPointOne, 8):
                    InterleaveSevenPointOne(length, samples, subFrames);
                    break;
                case (FlacChannelAssignments.LeftAndDifference, 2):
                    InterleaveLeftSideStereo(length, samples, subFrames);
                    break;
                case (FlacChannelAssignments.RightAndDifference, 2):
                    InterleaveRightSideStereo(length, samples, subFrames);
                    break;
                case (FlacChannelAssignments.CenterAndDifference, 2):
                    InterleaveMidSideStereo(length, samples, subFrames);
                    break;
                default:
                    throw new FlacException("The channel assignment is not supported!");
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveMidSideStereo(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            using var left = new PooledArray<int>((int)length);
            using var right = new PooledArray<int>((int)length);
            if (subFrames[0].Read(left.Span).HasNoData || subFrames[1].Read(right.Span).HasNoData)
            {
                throw new FlacException("Unknown error! This is a bug!");
            }
            FlacSideStereoUtils.DecodeAndInterleaveMidSideStereo(samples, left.Span, right.Span);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveRightSideStereo(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            using var left = new PooledArray<int>((int)length);
            using var right = new PooledArray<int>((int)length);
            if (subFrames[0].Read(left.Span).HasNoData || subFrames[1].Read(right.Span).HasNoData)
            {
                throw new FlacException("Unknown error! This is a bug!");
            }
            FlacSideStereoUtils.DecodeAndInterleaveRightSideStereo(samples, left.Span, right.Span);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveLeftSideStereo(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            using var left = new PooledArray<int>((int)length);
            using var right = new PooledArray<int>((int)length);
            if (subFrames[0].Read(left.Span).HasNoData || subFrames[1].Read(right.Span).HasNoData)
            {
                throw new FlacException("Unknown error! This is a bug!");
            }
            FlacSideStereoUtils.DecodeAndInterleaveLeftSideStereo(samples, left.Span, right.Span);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveSevenPointOne(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            var iLen = (int)length;
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

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveDolbySixPointOne(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            var iLen = (int)length;
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

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveFivePointOne(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            var iLen = (int)length;
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

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveFrontFive(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            var iLen = (int)length;
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

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveQuad(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            var iLen = (int)length;
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

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveFrontThree(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            var iLen = (int)length;
            using var src = new PooledArray<int>(iLen * 3);
            var j = src.Span;
            if (subFrames[0].Read(j.Slice(0, iLen)).HasNoData || subFrames[1].Read(j.Slice(iLen, iLen)).HasNoData || subFrames[2].Read(j.Slice(iLen * 2, iLen)).HasNoData)
            {
                throw new FlacException("Unknown error! This is a bug!");
            }
            AudioUtils.InterleaveThree(samples, j.Slice(0, iLen), j.Slice(iLen, iLen), j.Slice(iLen * 2, iLen));
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void InterleaveOrdinalStereo(uint length, Span<int> samples, IFlacSubFrame[] subFrames)
        {
            using var left = new PooledArray<int>((int)length);
            using var right = new PooledArray<int>((int)length);
            if (subFrames[0].Read(left.Span).HasNoData || subFrames[1].Read(right.Span).HasNoData)
            {
                throw new FlacException("Unknown error! This is a bug!");
            }
            AudioUtils.InterleaveStereo(samples, left.Span, right.Span);
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
                var length = (int)Length;
                Position += Length;
                return length * channelsDivisor.Divisor;
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static (uint bitDepth, BitDepthState state) ParseBitDepth(byte value)
        {
            var index = (value & 7) * 8;
            return ((byte)(0x0018_1410_000C_0800 >> index), (BitDepthState)(byte)(0x0201010102010100 >> index));
        }

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

        private static ReadOnlySpan<byte> SampleRateTable => new byte[] { 136, 88, 1, 0, 16, 177, 2, 0, 0, 238, 2, 0, 64, 31, 0, 0, 128, 62, 0, 0, 34, 86, 0, 0, 192, 93, 0, 0, 0, 125, 0, 0, 68, 172, 0, 0, 128, 187, 0, 0, 0, 119, 1, 0 };

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static (uint sampleRate, SampleRateState state) ParseSampleRate(byte value)
            => (value & 0xf) switch
            {
                0 => (0, SampleRateState.RespectStreamInfo),
                <= 0b1011 => (BinaryExtensions.ConvertToLittleEndian(Unsafe.Add(ref Unsafe.As<byte, uint>(ref MemoryMarshal.GetReference(SampleRateTable)), value - 1)), SampleRateState.Value),
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
