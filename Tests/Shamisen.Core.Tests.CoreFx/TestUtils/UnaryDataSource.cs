using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Core.Tests.CoreFx.TestUtils
{
    public sealed class UnaryDataSource : IReadableDataSource<byte>
    {
        private bool disposedValue;

        private ulong remainingBits;
        private bool hasFinalized = false;

        public UnaryDataSource(ulong remainingBits) => this.remainingBits = remainingBits;

        /// <summary>
        /// Gets the read support.
        /// </summary>
        /// <value>
        /// The read support.
        /// </value>
        public IReadSupport<byte> ReadSupport { get; }

        /// <summary>
        /// Gets the asynchronous read support.
        /// </summary>
        /// <value>
        /// The asynchronous read support.
        /// </value>
        public IAsyncReadSupport<byte> AsyncReadSupport { get; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public ulong? Length { get; }

        /// <summary>
        /// Gets the total length.
        /// </summary>
        /// <value>
        /// The total length.
        /// </value>
        public ulong? TotalLength { get; }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong? Position { get; }

        /// <summary>
        /// Gets the skip support.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport SkipSupport { get; }

        /// <summary>
        /// Gets the seek support.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport SeekSupport { get; }

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ReadResult Read(Span<byte> buffer)
        {
            ulong readingBits = (uint)buffer.Length * 8ul;
            if (buffer.IsEmpty) return 0;
            if (remainingBits > readingBits)
            {
                buffer.FastFill(0);
                remainingBits -= readingBits;
                return buffer.Length;
            }
            else if (remainingBits == readingBits)
            {
                buffer.FastFill(0);
                remainingBits = 0;
                return buffer.Length;
            }
            else if (remainingBits == 0)
            {
                if (!hasFinalized && !buffer.IsEmpty)
                {
                    hasFinalized = true;
                    buffer[0] = (byte)0x80u;
                    buffer.Slice(1).FastFill(0);
                    return buffer.Length;
                }
                if (!buffer.IsEmpty)
                {
                    buffer.FastFill(0);
                }
                return buffer.Length;
            }
            else
            {
                int h = (int)(remainingBits / 8ul);
                var j = buffer.SliceWhileIfLongerThan(h);
                j.FastFill(0);
                var k = buffer.Slice(j.Length);
                int res = (int)(remainingBits % 8);
                if (!k.IsEmpty)
                {
                    hasFinalized = true;
                    k[0] = (byte)(0x80u >> res);
                    if (k.Length > 1) k.Slice(1).FastFill(0);
                    remainingBits = 0;
                    return buffer.Length;
                }
                else
                {
                    remainingBits = 0;
                    return buffer.Length;
                }
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

                disposedValue = true;
            }
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
