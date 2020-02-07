using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonoAudio.Utils;

namespace MonoAudio.Data
{
    public sealed partial class SynchronizedDataReader<TSample>
    {
        private sealed class AsyncStreamSynchronizer : ISynchronizedDataReader<TSample>, IDisposable
        {
            private DataReader<TSample> dataReader;

            private TSample[] internalBuffer;
            private bool disposedValue = false;

            private Memory<TSample> InternalBuffer => new Memory<TSample>(internalBuffer);

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncStreamSynchronizer"/> class.
            /// </summary>
            /// <param name="dataReader">The data reader.</param>
            /// <exception cref="ArgumentNullException">dataReader</exception>
            public AsyncStreamSynchronizer(DataReader<TSample> dataReader) => this.dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));

            public ReadResult Read(Span<TSample> buffer)
            {
                if (internalBuffer == null || buffer.Length > internalBuffer.Length)
                {
                    ExpandBuffer(buffer.Length);
                }

                Memory<TSample> readBuffer = InternalBuffer.SliceWhile(buffer.Length);
                var runTask = ReadFromSourceAsync(readBuffer);
                var lengthRead = runTask.Result;   //wait for result
                readBuffer.SliceWhile(lengthRead).Span.CopyTo(buffer);
                return lengthRead;
            }

            private async Task<int> ReadFromSourceAsync(Memory<TSample> buffer) => await dataReader.ReadAsync(buffer);

            private void ExpandBuffer(int newLength)
            {
                if (internalBuffer != null) ArrayPool<TSample>.Shared.Return(internalBuffer, true);
                internalBuffer = ArrayPool<TSample>.Shared.Rent(newLength);
            }

            #region IDisposable Support

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
                    dataReader.Dispose();
                    dataReader = null;
                    if (internalBuffer != null) ArrayPool<TSample>.Shared.Return(internalBuffer, true);
                    internalBuffer = null;

                    //
                    disposedValue = true;
                }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion IDisposable Support
        }
    }
}
