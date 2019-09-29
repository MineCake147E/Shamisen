using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Utils
{
    /// <summary>
    /// Provides a simple internal buffer instancing.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <seealso cref="IDisposable" />
    internal sealed class BufferInstance<TSample> : IDisposable
    {
        private bool disposedValue;
        private TSample[] array;
        internal Memory<TSample> ActualBuffer { get; private set; }

        internal Memory<TSample> Filled { get; set; }

        internal bool IsFilled => !Filled.IsEmpty;

        public BufferInstance(int initialSize)
        {
            ActualBuffer = array = ArrayPool<TSample>.Shared.Rent(initialSize);
            Filled = Memory<TSample>.Empty;
            disposedValue = false;
        }

        public void Resize(int newSize)
        {
            Filled.Span.Fill(default);
            ArrayPool<TSample>.Shared.Return(array);
            ActualBuffer = array = ArrayPool<TSample>.Shared.Rent(newSize);
        }

        #region IDisposable Support

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                if (!(array is null)) ArrayPool<TSample>.Shared.Return(array);
                ActualBuffer = Memory<TSample>.Empty;
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
