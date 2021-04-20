using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.SubFrames;
using Shamisen.Data;
using Shamisen.Data.Binary;
using Shamisen.Utils;

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Parses FLAC frames.
    /// </summary>
    public sealed partial class FlacFrameParser
    {
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

        private IFlacSubFrame[]? subFrames;
        private ulong? sampleNumber = null;
        private ulong? frameNumber = null;
        private uint sampleRate = 0;
        private FlacChannelAssignments channels = 0;
        private uint bitDepth = 0;
        private FlacCrc16 crc16 = new(0);
        private int[]? samples;

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
        /// Finds the next frame.
        /// </summary>
        /// <returns></returns>
        public void ParseNextFrame()
        {
            ushort q = default;
            Span<byte> rawHeader = stackalloc byte[16];
            byte? lookahead = null;
            //Find next byte-aligned frame sync code.
            while (true)
            {
                //Source.Pin();
                while (MathI.ExtractBitField(q, 2, 14) != 0b1111_1111_1111_10u)
                {
                    if (lookahead is { } la)
                    {
                        q <<= 8;
                        q |= la;
                    }
                    else
                    {
                        q <<= 8;
                        if (!Source.ReadByte(out var tt)) return;
                        q |= tt;
                    }
                }
                Unsafe.As<byte, ushort>(ref rawHeader[0]) = BinaryExtensions.ConvertToBigEndian(q);
                var ncrc = new FlacCrc8(0);
                var ncrc16 = new FlacCrc16(0);
                Source.Crc16 = ncrc16;
                ncrc *= q;
                ushort next16 = (ushort)(Source.ReadBitsUInt32(16) ?? throw new FlacException("The decoder ran out of data!"));//Contains blockSize to Reserved before CRC
                var (length, state) = ParseBlockSize((byte)MathI.ExtractBitField(next16, 12, 4));
                var nSampleRate = ParseSampleRate((byte)MathI.ExtractBitField(next16, 8, 4));
                var nChannels = (FlacChannelAssignments)MathI.ExtractBitField(next16, 4, 4);
                var nBitDepth = ParseBitDepth((byte)MathI.ExtractBitField(next16, 1, 3));
                Unsafe.As<byte, ushort>(ref rawHeader[2]) = BinaryExtensions.ConvertToBigEndian(next16);
                ncrc *= next16;
                int bytesRead = 4;
                if ((q & 0b1) > 0)   //variable blocking
                {
                    var sn = Source.ReadUtf8UInt64(out var snum, rawHeader.Slice(bytesRead), out int a);
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
                    var fn = Source.ReadUtf8UInt32(out var fnum, rawHeader.Slice(bytesRead), out int a);
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
                        if (!Source.ReadByte(out var v))
                        {
                            throw new FlacException("The decoder ran out of data!");
                        }
                        rawHeader[bytesRead++] = v;
                        ncrc *= v;
                        ncrc16 *= v;
                        nBitDepth.bitDepth = v + 1u;
                        break;
                    case BlockSizeState.GetUInt16FromEnd:
                        ushort v2 = (ushort?)Source.ReadBitsUInt32(16) ?? throw new FlacException("The decoder ran out of data!");
                        Unsafe.As<byte, ushort>(ref rawHeader[bytesRead]) = BinaryExtensions.ConvertToBigEndian(v2);
                        bytesRead += 2;
                        ncrc *= v2;
                        ncrc16 *= v2;
                        nBitDepth.bitDepth = v2 + 1u;
                        break;
                    default:
                        break;
                }
                switch (nSampleRate.state)
                {
                    case SampleRateState.RespectStreamInfo:
                        nSampleRate.sampleRate = StreamInfoBlock.SampleRate;
                        break;
                    case SampleRateState.GetByteKHzFromEnd:
                        if (!Source.ReadByte(out var v))
                        {
                            throw new FlacException("The decoder ran out of data!");
                        }
                        rawHeader[bytesRead++] = v;
                        ncrc *= v;
                        ncrc16 *= v;
                        nSampleRate.sampleRate = v * 1000u;
                        break;
                    case SampleRateState.GetUInt16HzFromEnd:
                        ushort v2 = (ushort?)Source.ReadBitsUInt32(16) ?? throw new FlacException("The decoder ran out of data!");
                        Unsafe.As<byte, ushort>(ref rawHeader[bytesRead]) = BinaryExtensions.ConvertToBigEndian(v2);
                        bytesRead += 2;
                        ncrc *= v2;
                        ncrc16 *= v2;
                        nSampleRate.sampleRate = v2;
                        break;
                    case SampleRateState.GetUInt16TenHzFromEnd:
                        ushort v3 = (ushort?)Source.ReadBitsUInt32(16) ?? throw new FlacException("The decoder ran out of data!");
                        Unsafe.As<byte, ushort>(ref rawHeader[bytesRead]) = BinaryExtensions.ConvertToBigEndian(v3);
                        bytesRead += 2;
                        ncrc *= v3;
                        ncrc16 *= v3;
                        nSampleRate.sampleRate = v3 * 10u;
                        break;
                    default:
                        lookahead = rawHeader[bytesRead - 1];
                        continue;
                }
                if (!Source.ReadByte(out var expectedCrc))
                {
                    throw new FlacException("The decoder ran out of data!");
                }
                rawHeader[bytesRead++] = expectedCrc;
                if (expectedCrc != ncrc)
                {
                    lookahead = rawHeader[bytesRead - 1];
                    continue;
                }
                sampleRate = nSampleRate.sampleRate;
                channels = nChannels;
                bitDepth = nBitDepth.bitDepth;
                crc16 = ncrc16;
                //Read sub frame
                int chCount = nChannels.GetChannels();
                subFrames = new IFlacSubFrame[chCount];
                var bitReader = Source;
                for (int ch = 0; ch < subFrames.Length; ch++)
                {
                    var result = ReadSubFrame(bitReader, (int)length, (int)nBitDepth.bitDepth);
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
                if (!Source.ReadZeroPadding())
                {
                    throw new FlacException("The decoder ran out of data!");
                }
                if (!(Source.ReadBitsUInt32(16) is { } expectedCrc16) || expectedCrc16 != Source.Crc16)
                {
                    throw new FlacException("The decoder has detected CRC-16 mismatch!");
                }
                if (subFrames is null) throw new FlacException("The subFrames is null! This is a bug!");
                samples = new int[length * nChannels.GetChannels()];
                switch (nChannels)
                {
                    case FlacChannelAssignments.Monaural when subFrames.Length == 1:
                        var rr1 = subFrames[0].Read(samples);
                        if (rr1.HasNoData)
                        {
                            throw new FlacException("Unknown error! This is a bug!");
                        }
                        break;
                    case FlacChannelAssignments.OrdinalStereo when subFrames.Length == 2:
                        {
                            var left = new int[length];
                            var right = new int[length];
                            if (subFrames[0].Read(left).HasNoData || subFrames[1].Read(right).HasNoData)
                            {
                                throw new FlacException("Unknown error! This is a bug!");
                            }
                            AudioUtils.InterleaveStereo(samples, left, right);
                        }
                        break;
                    case FlacChannelAssignments.FrontThree when subFrames.Length == 3:
                        break;
                    case FlacChannelAssignments.Quad when subFrames.Length == 4:
                        break;
                    case FlacChannelAssignments.FrontFive when subFrames.Length == 5:
                        break;
                    case FlacChannelAssignments.FivePointOne when subFrames.Length == 6:
                        break;
                    case FlacChannelAssignments.DolbySixPointOne when subFrames.Length == 7:
                        break;
                    case FlacChannelAssignments.SevenPointOne when subFrames.Length == 8:
                        break;
                    case FlacChannelAssignments.LeftAndDifference when subFrames.Length == 2:
                        break;
                    case FlacChannelAssignments.RightAndDifference when subFrames.Length == 2:
                        break;
                    case FlacChannelAssignments.CenterAndDifference when subFrames.Length == 2:
                        break;
                    default:
                        throw new FlacException("The channel assignment is not supported!");
                }

                return;
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
                wasted = (int)value;
            }
#pragma warning disable S907 // "goto" statement should not be used
            switch (result >> 1)
            {
                case 0: //CONSTANT
                    return FlacConstantSubFrame.ReadFrame(flacBitReader, wasted);
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

        internal static (uint length, BlockSizeState state) ParseBlockSize(byte value)
            => value switch
            {
                //Reserved
                0 => (0, BlockSizeState.Reserved),
                1 => (192, BlockSizeState.Value),
                <= 0b101 => (576u << (value - 2), BlockSizeState.Value),
                0b0110 => (0, BlockSizeState.GetByteFromEnd),
                0b0111 => (0, BlockSizeState.GetUInt16FromEnd),
                _ => (1u << value, BlockSizeState.Value),
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

        internal static (uint sampleRate, SampleRateState state) ParseSampleRate(byte value)
            => (value & 0xf) switch
            {
                0 => (0, SampleRateState.RespectStreamInfo),
                1 => (0, SampleRateState.RespectStreamInfo),
                <= 0b1011 => (Unsafe.Add(ref MemoryMarshal.GetReference(SampleRateTable), value - 2), SampleRateState.Value),
                0b1100 => (0, SampleRateState.GetByteKHzFromEnd),
                0b1101 => (0, SampleRateState.GetUInt16HzFromEnd),
                0b1110 => (0, SampleRateState.GetUInt16TenHzFromEnd),
                _ => (0, SampleRateState.SyncFooled),
            };

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

        internal static (uint bitDepth, BitDepthState state) ParseBitDepth(byte value)
            => Unsafe.Add(ref MemoryMarshal.GetReference(BitDepthTable), value & 0x7);
    }
}
