using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Codecs.Flac.SubFrames
{
    /// <summary>
    /// Represents a constant FLAC sub-frame.
    /// </summary>
    public sealed class FlacConstantSubFrame : IFlacSubFrame
    {
        private int value;
        private int wastedBits;

        /// <summary>
        /// Initializes a new instance of <see cref="FlacConstantSubFrame"/>.
        /// </summary>
        /// <param name="value"></param>
        public FlacConstantSubFrame(int value, int wastedBits)
        {
            this.value = value;
            this.wastedBits = wastedBits;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FlacConstantSubFrame"/>.
        /// </summary>
        /// <param name="bitReader">A <see cref="FlacBitReader"/> to read the constant from.</param>
        /// <param name="wastedBits">The number of wasted bits.</param>
        public static FlacConstantSubFrame? ReadFrame(FlacBitReader bitReader, int wastedBits)
        {
            (var res, var val) = bitReader.ReadBitsUInt64(32);
            return !res ? null : new FlacConstantSubFrame((int)val, wastedBits);
        }

        /// <summary>
        /// Gets the number of wasted LSBs.
        /// </summary>
        public int WastedBits => wastedBits;

        /// <summary>
        /// Gets the type of the sub-frame.
        /// </summary>
        public byte SubFrameType => 0;

        /// <summary>
        /// Reads the data to the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<int> buffer)
        {
            buffer.FastFill(value << wastedBits);
            return buffer.Length;
        }
    }
}
