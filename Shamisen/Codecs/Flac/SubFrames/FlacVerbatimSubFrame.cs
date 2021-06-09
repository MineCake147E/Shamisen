using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.Parsing;
using Shamisen.Data;

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

        private PooledArray<int> values;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of <see cref="FlacVerbatimSubFrame"/>.
        /// </summary>
        public FlacVerbatimSubFrame(Span<int> values, int wastedBits)
        {
            this.values = new(values.Length);
            values.CopyTo(this.values.Array.AsSpan());
            WastedBits = wastedBits;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FlacVerbatimSubFrame"/>.
        /// </summary>
        public FlacVerbatimSubFrame(FlacBitReader bitReader, int blockSize, int wastedBits, byte bitsPerSample)
        {
            values = new(blockSize);
            var vs = values.Span;
            for (int i = 0; i < vs.Length; i++)
            {
                var (hasValue, result) = bitReader.ReadBitsInt32(bitsPerSample);
                if (!hasValue) throw new FlacException("Invalid FLAC Stream!", bitReader);
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
            values.Span.CopyTo(buffer);
            return values.Length;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                values.Dispose();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FlacVerbatimSubFrame"/> class.
        /// </summary>
        ~FlacVerbatimSubFrame()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
