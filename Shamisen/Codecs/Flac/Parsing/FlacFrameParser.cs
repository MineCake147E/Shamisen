using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.SubFrames;
using Shamisen.Data;
using Shamisen.Data.Binary;

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
            //Find next byte-aligned frame sync code.
            while (true)
            {
                //Source.Pin();
                while (MathI.ExtractBitField(q, 2, 14) != 0b1111_1111_1111_10u)
                {
                    q <<= 8;
                    if (!Source.ReadByte(out var tt)) return;
                    q |= tt;
                }
                var ncrc = new FlacCrc8(0);
                var ncrc16 = new FlacCrc16(0);
                ncrc *= q;
                ncrc16 *= q;
                ushort next16 = (ushort)(Source.ReadBitsUInt32(16) ?? throw new FlacException("The decoder ran out of data!"));//Contains blockSize to Reserved before CRC
                var (length, state) = ParseBlockSize((byte)MathI.ExtractBitField(next16, 12, 4));
                var nSampleRate = ParseSampleRate((byte)MathI.ExtractBitField(next16, 8, 4));
                var nChannels = (FlacChannelAssignments)MathI.ExtractBitField(next16, 4, 4);
                var nBitDepth = ParseBitDepth((byte)MathI.ExtractBitField(next16, 1, 3));
                ncrc *= next16;
                ncrc16 *= next16;
                if ((q & 0b1) > 0)   //variable blocking
                {
                    var sn = Source.ReadUtf8UInt64(out var snum, default, out _);
                    if (sn)
                    {
                        sampleNumber = snum;
                    }
                    else
                    {
                        Source.Rewind();
                        _ = Source.ReadByte();
                        continue;
                    }
                }
                else
                {
                    var fn = FlacUtf8NumberUtils.ReadUtf8EncodedShortNumber(Source, ref ncrc, ref ncrc16);
                    if (fn is { } fnum)
                    {
                        frameNumber = fnum;
                    }
                    else
                    {
                        Source.Rewind();
                        _ = Source.ReadByte();
                        continue;
                    }
                }
                switch (state)
                {
                    case BlockSizeState.GetByteFromEnd:
                        byte v = Source.ReadByte();
                        ncrc *= v;
                        ncrc16 *= v;
                        nBitDepth.bitDepth = v + 1u;
                        break;
                    case BlockSizeState.GetUInt16FromEnd:
                        ushort v2 = Source.ReadUInt16BigEndian();
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
                        byte v = Source.ReadByte();
                        ncrc *= v;
                        ncrc16 *= v;
                        nSampleRate.sampleRate = v * 1000u;
                        break;
                    case SampleRateState.GetUInt16HzFromEnd:
                        ushort v2 = Source.ReadUInt16BigEndian();
                        ncrc *= v2;
                        ncrc16 *= v2;
                        nSampleRate.sampleRate = v2;
                        break;
                    case SampleRateState.GetUInt16TenHzFromEnd:
                        ushort v3 = Source.ReadUInt16BigEndian();
                        ncrc *= v3;
                        ncrc16 *= v3;
                        nSampleRate.sampleRate = v3 * 10u;
                        break;
                    default:
                        Source.Rewind();
                        _ = Source.ReadByte();
                        continue;
                }
                var expectedCrc = Source.ReadByte();
                if (expectedCrc != ncrc)
                {
                    Source.Rewind();
                    _ = Source.ReadByte();
                    continue;
                }
                sampleRate = nSampleRate.sampleRate;
                channels = nChannels;
                bitDepth = nBitDepth.bitDepth;
                crc16 = ncrc16;
                //Read sub frame
                int chCount = nChannels.GetChannels();
                subFrames = new IFlacSubFrame[chCount];
                var bitReader = new FlacBitReader(Source, ncrc16);
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
                    Source.Rewind();
                    _ = Source.ReadByte();
                    continue;
                }
                switch (nChannels)
                {
                    case FlacChannelAssignments.Monaural:
                        break;
                    case FlacChannelAssignments.OrdinalStereo:
                        break;
                    case FlacChannelAssignments.ThreePointOne:
                        break;
                    case FlacChannelAssignments.Quad:
                        break;
                    case FlacChannelAssignments.FrontFive:
                        break;
                    case FlacChannelAssignments.FivePointOne:
                        break;
                    case FlacChannelAssignments.DolbySixPointOne:
                        break;
                    case FlacChannelAssignments.SevenPointOne:
                        break;
                    case FlacChannelAssignments.LeftAndDifference:
                        break;
                    case FlacChannelAssignments.RightAndDifference:
                        break;
                    case FlacChannelAssignments.CenterAndDifference:
                        break;
                    default:
                        break;
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
