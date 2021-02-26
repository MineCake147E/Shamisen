using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Shamisen.Data.Binary;
using Shamisen.Utils;

namespace Shamisen.Data
{
    /// <summary>
    /// Buffers the data asynchronously like YouTube does.<br/>
    /// It reads a little more than required, and prevents waiting for IOs.
    /// </summary>
    public sealed class PreloadDataBuffer<TSample> : IDataSource<TSample> where TSample : unmanaged
    {
        private ManualResetEventSlim fillFlag = new ManualResetEventSlim(true);
        private ManualResetEventSlim readFlag = new(true);
        private ManualResetEventSlim peekFlag = new(true);
        private ManualResetEventSlim fallbackFlag = new(true);
        private bool isEndOfStream = false;
        private SemaphoreSlim readSemaphore = new(1);

        private bool disposedValue = false;

        private volatile int bufferSize = 0;

        private volatile int totalBuffers = 0;

        private ConcurrentQueue<(BufferInstance<TSample> buffer, bool isEndOfStream)> buffersFilled;

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
        /// Gets or sets the value which indicates whether the <see cref="PreloadDataBuffer{TSample}"/> should wait for another sample block or not.
        /// </summary>
        public bool AllowWaitForRead { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreloadDataBuffer{TSample}"/> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="initialBlockSize">
        /// The size of initial buffer in Frames(independent on the number of channel and the type of sample).<br/>
        /// The buffer is automatically extended if the internal buffer is smaller than the size of reading buffers.
        /// </param>
        /// <param name="internalBufferNumber">The number of internal buffer.</param>
        /// <param name="allowWaitForRead">The value which indicates whether the <see cref="PreloadDataBuffer{TSample}"/> should wait for another sample block or not.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialBlockSize"/> should be larger than or equals to 2048.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="internalBufferNumber"/> should be larger than or equals to 16.</exception>
        public PreloadDataBuffer(IDataSource<TSample> dataSource, int initialBlockSize, int internalBufferNumber = 16, bool allowWaitForRead = false)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            AllowWaitForRead = allowWaitForRead;
            if (initialBlockSize < 2048) throw new ArgumentOutOfRangeException(nameof(initialBlockSize));
            if (internalBufferNumber < 16) throw new ArgumentOutOfRangeException(nameof(internalBufferNumber));
            bufferSize = initialBlockSize;
            buffersFilled = new();
            buffersEmpty = new();
            buffersNeededToBeResized = new ConcurrentQueue<(BufferInstance<TSample> buffer, int newSize)>();
            for (int i = 0; i < internalBufferNumber; i++)
            {
                var b = new BufferInstance<TSample>(initialBlockSize);
                var rr = ReadFromSource(b.ActualBuffer.Span);
                if (rr.IsEndOfStream)
                {
                    //Rarely happens unless the source itself is extremely short
                    b.Filled = default;
                    buffersFilled.Enqueue((b, true));
                    totalBuffers++;
                    break;
                }
                else if (rr.HasNoData)
                {
                    buffersEmpty.Enqueue(b);
                }
                else
                {
                    b.Filled = b.ActualBuffer.SliceWhile(b.Filled.Length + rr.Length);
                    buffersFilled.Enqueue((b, false));
                }
                totalBuffers++;
            }
            cancellationTokenSource = new CancellationTokenSource();
            fillFlag.Set();
            writeTask = Task.Run(() => RunBufferAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
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
            int written = 0;
            bool doDirectRead = false;
            try
            {
                bufferSize = Math.Max(destination.Length, bufferSize);
                var remBuffer = destination;
#pragma warning disable IDE0068
                while (!remBuffer.IsEmpty)
#pragma warning restore IDE0068
                {
                    if (!buffersFilled.TryPeek(out var internalBuffer))
                    {
                        if (isEndOfStream)
                        {
                            return written > 0 ? written : ReadResult.EndOfStream;
                        }
                        if (doDirectRead)
                        {
                            DebugUtils.WriteLine($"Reading stream...");
                            var rr = dataSource.TryReadAll(remBuffer);  //Detecting EOS
                            if (rr.HasNoData)
                            {
                                if (rr.IsEndOfStream)
                                {
                                    isEndOfStream = true;
                                    return written > 0 ? written : ReadResult.EndOfStream;
                                }
                                return written;
                            }
                            return written + rr.Length;
                        }
                        else
                        {
                            if (AllowWaitForRead)
                            {
                                DebugUtils.WriteLine("[WARNING] DIRECT READ MODE ENTERED\nStopping runner thread...");
                                fallbackFlag.Reset();
                                fillFlag.Set();
                                DebugUtils.WriteLine($"Waiting for {nameof(readSemaphore)}...");
                                readSemaphore.Wait();
                                DebugUtils.WriteLine($"Releasing {nameof(readSemaphore)}...");
                                _ = readSemaphore.Release();
                                peekFlag.Reset();
                                if (fillFlag.IsSet)
                                {
                                    fallbackFlag.Set();
                                    peekFlag.Wait();
                                    fillFlag.Reset();
                                }
                                DebugUtils.WriteLine($"Waiting for {nameof(readSemaphore)}...");
                                readSemaphore.Wait(1000, cancellationTokenSource.Token);
                                peekFlag.Reset();
                                readFlag.Reset();
                                doDirectRead = true;
                            }
                            else
                            {
                                return written;
                            }
                        }
                    }
                    else if (internalBuffer.isEndOfStream)
                    {
                        isEndOfStream = true;
                        return written > 0 ? written : ReadResult.EndOfStream;
                    }
                    else if (!internalBuffer.buffer.IsFilled)
                    {
#if DEBUG
                        throw new InvalidProgramException($"An empty buffer has been queued into {nameof(buffersFilled)}!");
#else
                        buffersFilled.TryDequeue(out _);
                        buffersEmpty.Enqueue(internalBuffer.buffer);
#endif
                    }
                    else
                    {
                        var buffer = internalBuffer.buffer;
                        var g = buffer.Filled.Span;
                        if (g.Length > remBuffer.Length)
                        {
                            g.Slice(0, remBuffer.Length).CopyTo(remBuffer);
                            buffer.Filled = buffer.Filled.Slice(remBuffer.Length);
                            written += remBuffer.Length;
                            remBuffer = default;
                        }
                        else if (g.Length == remBuffer.Length)
                        {
                            g.CopyTo(remBuffer);
                            buffersFilled.TryDequeue(out _);
                            buffersEmpty.Enqueue(buffer);
                            fillFlag.Set();
                            written += remBuffer.Length;
                            remBuffer = default;
                        }
                        else
                        {
                            g.CopyTo(remBuffer);
                            remBuffer = remBuffer.Slice(g.Length);
                            buffersFilled.TryDequeue(out _);
                            if (buffer.ActualBuffer.Length < bufferSize)
                                buffersNeededToBeResized.Enqueue((buffer, bufferSize));
                            else buffersEmpty.Enqueue(buffer);
                            fillFlag.Set();
                            written += g.Length;
                        }
                    }
                }
            }
            finally
            {
                if (doDirectRead)
                {
                    DebugUtils.WriteLine($"Leaving direct read mode...");
                    readSemaphore.Release();
                    readFlag.Set();
                    fillFlag.Set();
                    peekFlag.Set();
                    fallbackFlag.Set();
                    DebugUtils.WriteLine($"Left direct read mode...");
                }
            }

            return written;
        }

        private async ValueTask RunBufferAsync(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                while (buffersNeededToBeResized.TryDequeue(out var resizingBuffer))
                {
                    DebugUtils.WriteLine($"Resizing buffer from {resizingBuffer.buffer.ActualBuffer.Length} to {resizingBuffer.newSize}...");
                    resizingBuffer.buffer.Resize(resizingBuffer.newSize);
                    buffersEmpty.Enqueue(resizingBuffer.buffer);
                }
                token.ThrowIfCancellationRequested();
                fillFlag.Wait(token);
                token.ThrowIfCancellationRequested();
                if (!buffersEmpty.TryDequeue(out var internalBuffer))
                {
                    if (!buffersNeededToBeResized.IsEmpty || !buffersEmpty.IsEmpty) continue;
                    /*DebugUtils.WriteLine($"Runner is falling asleep... " +
                        $"{buffersFilled.Count} filled, {buffersNeededToBeResized.Count} to be resized, " +
                        $"{buffersEmpty.Count} empty, {totalBuffers} total");*/
                    fillFlag.Reset();
                }
                else
                {
                    token.ThrowIfCancellationRequested();
                    readFlag.Wait(token);
                    await readSemaphore.WaitAsync(token);
                    var rr = ReadFromSource(internalBuffer.ActualBuffer.Span);

                    int readLength = rr.Length;
                    if (rr.IsEndOfStream) //End of stream
                    {
                        internalBuffer.Filled = Memory<TSample>.Empty;
                        buffersFilled.Enqueue((internalBuffer, true));  //Send EOS signal
                        return;
                    }
                    if (rr.HasNoData)    //Could not read any data
                    {
                        buffersEmpty.Enqueue(internalBuffer);
                        continue;
                    }
                    //if (readLength < internalBuffer.ActualBuffer.Length - internalBuffer.Filled.Length)  //Not enough data
                    //{
                    //    internalBuffer.Filled = internalBuffer.ActualBuffer.SliceWhile(internalBuffer.Filled.Length + readLength);

                    //    continue;
                    //}
                    internalBuffer.Filled = internalBuffer.ActualBuffer.Slice(0, readLength);

                    buffersFilled.Enqueue((internalBuffer, false));
                    _ = readSemaphore.Release();
                }
                fallbackFlag.Wait(token);//block when fallbackFlag is reset
                if (!peekFlag.IsSet)
                {
                    var b = new BufferInstance<TSample>(bufferSize);
                    buffersEmpty.Enqueue(b);
                    totalBuffers++;
                    DebugUtils.WriteLine($"Runner thread is entering direct read mode...");
                    DebugUtils.WriteLine($"Increased number of buffer to {totalBuffers}!");
                    fallbackFlag.Reset();
                    peekFlag.Set();
                    DebugUtils.WriteLine($"Runner is waiting for {nameof(fallbackFlag)}...");
                    fallbackFlag.Wait(token);
                    fallbackFlag.Set();
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
                readFlag.Set();
                fillFlag.Set(); //Resume Buffering Thread
                writeTask.ConfigureAwait(false);
                writeTask.Wait();
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
                    buffer.buffer.Dispose();
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
