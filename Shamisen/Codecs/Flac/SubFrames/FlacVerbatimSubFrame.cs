using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Codecs.Flac.SubFrames
{
    /// <summary>
    /// Represents a verbatim FLAC sub-frame.
    /// </summary>
    public sealed class FlacVerbatimSubFrame : IFlacSubFrame
    {
        /// <summary>
        /// Gets the number of wasted LSBs.
        /// </summary>
        public int WastedBits { get; private set; }

        /// <summary>
        /// Gets the type of the sub-frame.
        /// </summary>
        public byte SubFrameType => 1;

        private int[] values;

        /// <summary>
        /// Initializes a new instance of <see cref="FlacVerbatimSubFrame"/>.
        /// </summary>
        public FlacVerbatimSubFrame(Span<int> values, int wastedBits)
        {
            this.values = new int[values.Length];
            values.CopyTo(this.values.AsSpan());
            WastedBits = wastedBits;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FlacVerbatimSubFrame"/>.
        /// </summary>
        public FlacVerbatimSubFrame(FlacBitReader bitReader, int blockSize, int wastedBits, byte bitsPerSample)
        {
            values = new int[blockSize];
            var vs = values.AsSpan();
            for (int i = 0; i < vs.Length; i++)
            {
                var (hasValue, result) = bitReader.ReadBitsUInt64(bitsPerSample);
                if (!hasValue) throw new FlacException("Invalid FLAC Stream!");
                vs[i] = (int)result << wastedBits;
            }
        }

        /// <summary>
        /// Reads the data to the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<int> buffer)
        {
            if (values.Length > buffer.Length) return ReadResult.EndOfStream;
            values.AsSpan().CopyTo(buffer);
            return values.Length;
        }
    }
}
