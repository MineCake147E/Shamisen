﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MonoAudio.Utils;

namespace MonoAudio.Data
{
    /// <summary>
    /// Buffers the data asynchronously like YouTube does.<br/>
    /// It reads a little more than required, and prevents waiting for IOs.
    /// </summary>
    public sealed class PreloadDataBuffer<TSample> : IDataSource<TSample> where TSample : unmanaged
    {
        private ManualResetEventSlim fillFlag = new ManualResetEventSlim(true);

        private bool disposedValue = false;

        private volatile int bufferSize = 0;

        private ConcurrentQueue<BufferInstance<TSample>> buffersFilled;

        private ConcurrentQueue<BufferInstance<TSample>> buffersEmpty;

        private ConcurrentQueue<(BufferInstance<TSample> buffer, int newSize)> buffersNeededToBeResized;

        private CancellationTokenSource cancellationTokenSource;

        private Task writeTask;

        private IDataSource<TSample> dataSource;

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource{TSample}" />.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong Position { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreloadDataBuffer{TSample}"/> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="initialBlockSize">
        /// The size of initial buffer in Frames(independent on the number of channel and the type of sample).<br/>
        /// The buffer is automatically extended if the internal buffer is smaller than the size of reading buffers.
        /// </param>
        /// <param name="internalBufferNumber">The number of internal buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialBlockSize"/> should be larger than or equals to 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="internalBufferNumber"/> should be larger than or equals to 2.</exception>
        public PreloadDataBuffer(IDataSource<TSample> dataSource, int initialBlockSize, int internalBufferNumber = 4)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            if (initialBlockSize < 0) throw new ArgumentOutOfRangeException(nameof(initialBlockSize));
            if (internalBufferNumber < 2) throw new ArgumentOutOfRangeException(nameof(internalBufferNumber));
            bufferSize = initialBlockSize;
            buffersFilled = new ConcurrentQueue<BufferInstance<TSample>>();
            buffersEmpty = new ConcurrentQueue<BufferInstance<TSample>>();
            buffersNeededToBeResized = new ConcurrentQueue<(BufferInstance<TSample> buffer, int newSize)>();
            for (int i = 0; i < internalBufferNumber; i++)
            {
                var b = new BufferInstance<TSample>(initialBlockSize);
                buffersEmpty.Enqueue(b);
            }
            cancellationTokenSource = new CancellationTokenSource();

            writeTask = Task.Run(() => RunBuffer(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="destination">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public ReadResult Read(Span<TSample> destination)
        {
            bufferSize = Math.Max(destination.Length, bufferSize);
            int written = 0;
            var remBuffer = destination;
#pragma warning disable IDE0068
            while (!remBuffer.IsEmpty && buffersFilled.TryPeek(out var internalBuffer))
#pragma warning restore IDE0068
            {
                if (!internalBuffer.IsFilled)
                {
#if DEBUG
                    throw new InvalidProgramException($"An empty buffer has been queued into {nameof(buffersFilled)}!");
#else
                    buffersFilled.TryDequeue(out _);
                    buffersEmpty.Enqueue(internalBuffer);
                    continue;
#endif
                }
                else
                {
                    var g = internalBuffer.Filled.Span;
                    if (g.Length == 0)  //EOS
                    {
                        return -1;
                    }
                    if (g.Length > remBuffer.Length)
                    {
                        g.Slice(0, remBuffer.Length).CopyTo(remBuffer);
                        internalBuffer.Filled = internalBuffer.Filled.Slice(remBuffer.Length);
                    }
                    else if (g.Length == remBuffer.Length)
                    {
                        g.CopyTo(remBuffer);
                        remBuffer = Span<TSample>.Empty;
                        buffersFilled.TryDequeue(out _);
                        buffersEmpty.Enqueue(internalBuffer);
                        fillFlag.Set();
                    }
                    else
                    {
                        g.CopyTo(remBuffer);
                        remBuffer = remBuffer.Slice(g.Length);
                        buffersFilled.TryDequeue(out _);
                        buffersNeededToBeResized.Enqueue((internalBuffer, destination.Length));
                        fillFlag.Set();
                    }
                }
            }
            return written;
        }

        private void RunBuffer(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                fillFlag.Wait();
                token.ThrowIfCancellationRequested();
                if (buffersNeededToBeResized.TryDequeue(out var resizingBuffer))    //1 resize per loop
                {
                    resizingBuffer.buffer.Resize(resizingBuffer.newSize);
                    buffersEmpty.Enqueue(resizingBuffer.buffer);
                }
                token.ThrowIfCancellationRequested();
                if (!buffersEmpty.TryPeek(out var internalBuffer))
                {
                    if (!buffersNeededToBeResized.IsEmpty || !buffersEmpty.IsEmpty) continue;
                    fillFlag.Reset();
                }
                else
                {
                    ReadResult rr = ReadFromSource(internalBuffer.ActualBuffer.Span);
                    if (rr.HasNoData) continue;
                    int readLength = rr.Length;
                    if (readLength < 0) //End of stream
                    {
                        internalBuffer.Filled = Memory<TSample>.Empty;
                        buffersFilled.Enqueue(internalBuffer);  //Send EOS signal
                        return;
                    }
                    if (readLength == 0)    //Could not read any data
                    {
                        buffersEmpty.Enqueue(internalBuffer);
                        continue;
                    }
                    if (readLength < internalBuffer.ActualBuffer.Length - internalBuffer.Filled.Length)  //Not enough data
                    {
                        internalBuffer.Filled = internalBuffer.ActualBuffer.SliceWhile(internalBuffer.Filled.Length + readLength);
                        continue;
                    }
                    _ = buffersEmpty.TryDequeue(out _);
                    internalBuffer.Filled = internalBuffer.ActualBuffer.Slice(0, readLength);
                    buffersFilled.Enqueue(internalBuffer);
                }
            }
        }

        private ReadResult ReadFromSource(Span<TSample> buffer) => dataSource.Read(buffer);

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <typeparamref name="TSample" />s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
#pragma warning disable CS1998

        public async ValueTask<ReadResult> ReadAsync(Memory<TSample> destination) => Read(destination.Span);

#pragma warning restore CS1998

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                cancellationTokenSource.Cancel();
                fillFlag.Set(); //Resume Buffering Thread
                writeTask.Dispose();
                //writeTask = null;
                if (disposing)
                {
                    // Block intentionally left empty.
                }

                fillFlag.Dispose();
                //fillFlag = null;
                cancellationTokenSource.Dispose();
                //cancellationTokenSource = null;
                while (buffersFilled.TryDequeue(out var buffer))
                {
                    buffer.Dispose();
                }
                while (buffersEmpty.TryDequeue(out var buffer))
                {
                    buffer.Dispose();
                }
                while (buffersNeededToBeResized.TryDequeue(out var buffer))
                {
                    buffer.buffer.Dispose();
                }
                //buffersFilled = null;
                //buffersEmpty = null;
                if (dataSource is IDisposable disposable) disposable.Dispose();
                //dataSource = null;
                //buffersNeededToBeResized = null;
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
