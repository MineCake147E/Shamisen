using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Provides a functionality for reading bit stream data.
    /// </summary>
    public sealed partial class FlacBitReader : IDisposable
    {
        private ulong bitBuffer;
        private int consumedBits;
        private Memory<Vector<ulong>> bufferHead;
        private IReadableDataSource<byte>? source;
        private Vector<ulong>[]? buffer;
        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                buffer = null;
                source = null;
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
