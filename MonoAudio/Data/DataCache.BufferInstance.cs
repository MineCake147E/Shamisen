using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Data
{
    public sealed partial class DataCache<TSample> where TSample : unmanaged
    {
        private sealed class BufferInstance : IDisposable
        {
            private TSample[] actualBuffer;

            private int writePosition;
            public Memory<TSample> WriteHead => actualBuffer.AsMemory(writePosition);

            public Memory<TSample> ReadHead => actualBuffer.AsMemory().Slice(ReadPosition, writePosition - ReadPosition);

            public bool IsFull => WriteHead.IsEmpty;

            public bool HasNoDataToRead => ReadHead.IsEmpty;

            private bool disposedValue;

            public ulong InitialIndex { get; }

            public ulong NextIndex => InitialIndex + (ulong)actualBuffer.Length;

            public int ReadPosition { get; set; }

            public BufferInstance(TSample[] actualBuffer, ulong initialIndex)
            {
                this.actualBuffer = actualBuffer ?? throw new ArgumentNullException(nameof(actualBuffer));
                InitialIndex = initialIndex;
                writePosition = 0;
                ReadPosition = 0;
                disposedValue = false;
            }

            public int CompareRegion(ulong globalIndex)
            {
                bool a = globalIndex >= InitialIndex;
                bool b = globalIndex < NextIndex;
                return Unsafe.As<bool, byte>(ref a) - Unsafe.As<bool, byte>(ref b);
            }

            public int Write(ReadOnlySpan<TSample> data)
            {
                if (IsFull) return 0;
                var whead = WriteHead;
                if (data.Length >= whead.Length)
                {
                    data.Slice(0, whead.Length).CopyTo(whead.Span);
                    writePosition += whead.Length;
                    return whead.Length;
                }
                else
                {
                    data.CopyTo(whead.Span.Slice(0, data.Length));
                    writePosition += data.Length;
                    return data.Length;
                }
            }

            public ReadResult Read(Span<TSample> destination)
            {
                if (HasNoDataToRead) return ReadResult.EndOfStream;
                var rhead = ReadHead;
                if (destination.Length >= rhead.Length)
                {
                    rhead.Span.CopyTo(destination.Slice(0, rhead.Length));
                    ReadPosition += rhead.Length;
                    return rhead.Length;
                }
                else
                {
                    rhead.Span.Slice(0, destination.Length).CopyTo(destination);
                    ReadPosition += destination.Length;
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
