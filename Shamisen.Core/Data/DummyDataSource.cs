using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data
{
    /// <summary>
    /// Does nothing when
    /// </summary>
    public sealed class DummyDataSource<TSample> : IReadableDataSource<TSample> where TSample : unmanaged
    {
        private bool disposedValue;

        /// <inheritdoc/>
        public IReadSupport<TSample>? ReadSupport { get; }
        /// <inheritdoc/>
        public IAsyncReadSupport<TSample>? AsyncReadSupport { get; }
        /// <inheritdoc/>
        public ulong? Length { get; }
        /// <inheritdoc/>
        public ulong? TotalLength { get; }
        /// <inheritdoc/>
        public ulong? Position { get; }
        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }
        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }
        /// <inheritdoc/>
        public ReadResult Read(Span<TSample> buffer) => buffer.Length;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
