using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Data
{
    public sealed partial class DataCache
    {
        private sealed class BufferInstance : IDisposable
        {
            private byte[] actualBuffer;

            private int writeHead;
            public Memory<byte> WriteHead => actualBuffer.AsMemory(writeHead);

            private int readHead;
            public Memory<byte> ReadHead => actualBuffer.AsMemory(readHead).Slice(0, writeHead);

            public bool IsFull => WriteHead.IsEmpty;

            public bool HasNoDataToRead => ReadHead.IsEmpty;

            private bool disposedValue;

            public BufferInstance(byte[] actualBuffer)
            {
                this.actualBuffer = actualBuffer ?? throw new ArgumentNullException(nameof(actualBuffer));
                writeHead = 0;
                readHead = 0;
                disposedValue = false;
            }

            public int Write(ReadOnlySpan<byte> data)
            {
                if (IsFull) return 0;
                var whead = WriteHead;
                if (data.Length >= whead.Length)
                {
                    data.Slice(0, whead.Length).CopyTo(whead.Span);
                    writeHead += whead.Length;
                    return whead.Length;
                }
                else
                {
                    data.CopyTo(whead.Span.Slice(0, data.Length));
                    writeHead += data.Length;
                    return data.Length;
                }
            }

            public ReadResult Read(Span<byte> destination)
            {
                if (HasNoDataToRead) return ReadResult.EndOfStream;
                var rhead = ReadHead;
                if (destination.Length >= rhead.Length)
                {
                    ReadHead.Span.CopyTo(destination.Slice(rhead.Length));
                    readHead += rhead.Length;
                    return rhead.Length;
                }
                else
                {
                    rhead.Span.Slice(0, destination.Length).CopyTo(destination);
                    readHead += destination.Length;
                    return destination.Length;
                }
            }

            #region IDisposable Support

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        //
                    }
                    actualBuffer = null;
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
