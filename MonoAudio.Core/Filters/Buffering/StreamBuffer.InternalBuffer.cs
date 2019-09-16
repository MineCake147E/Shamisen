using System;
using System.Buffers;
using System.Threading;

namespace MonoAudio.Filters
{
    public sealed partial class StreamBuffer<TSample, TFormat> where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        private sealed class InternalBuffer : IDisposable
        {
            private bool disposedValue;
            private TSample[] array;
            internal Memory<TSample> ActualBuffer { get; private set; }

            internal Memory<TSample> Filled { get; set; }

            internal bool IsFilled => !Filled.IsEmpty;

            public InternalBuffer(int initialSize)
            {
                ActualBuffer = array = ArrayPool<TSample>.Shared.Rent(initialSize);
                Filled = Memory<TSample>.Empty;
                disposedValue = false;
            }

            public void Resize(int newSize)
            {
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
}
