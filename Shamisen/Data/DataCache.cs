using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

namespace Shamisen.Data
{
    /// <summary>
    /// Caches data into managed memory region.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public sealed partial class DataCache<TSample> : IReadableDataSource<TSample>, ISeekSupport, IAsyncReadSupport<TSample> where TSample : unmanaged
    {
        private int allocUnit = 1024;

        private UInt32Divisor allocUnitDivisor;

        /// <summary>
        /// Gets the bytes written inside internal buffer.
        /// </summary>
        /// <value>
        /// The bytes written.
        /// </value>
        public ulong BytesWritten { get; private set; } = 0;

        /// <summary>
        /// Gets or sets the current reading position.
        /// </summary>
        /// <value>
        /// The read position.
        /// </value>
        public ulong ReadPosition { get; set; } = 0;

        /// <summary>
        /// Gets the remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? Length => BytesWritten - ReadPosition;

        /// <summary>
        /// Gets the total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? TotalLength => BytesWritten;

        /// <summary>
        /// Gets the position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        ulong? IDataSource<TSample>.Position => ReadPosition;

        /// <summary>
        /// Gets the skip support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => this;

        /// <summary>
        /// Gets the seek support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => this;

        /// <summary>
        /// Gets the read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IReadSupport<TSample>? ReadSupport => this;

        /// <summary>
        /// Gets the asynchronous read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IAsyncReadSupport<TSample>? AsyncReadSupport => this;

        private List<BufferInstance> buffers;

        private (int buffer, int local) FindBufferIndex(ulong position)
        {
            //if (buffers.Count < 2) return (0, (int)position);
            int imax = buffers.Count;
            int imin = 0;
            //Binary search
            do
            {
                int checking = imin + (imax - imin) / 2;
                if (checking >= buffers.Count) break;
                var buf = buffers[checking];
                int cmp = buf.CompareRegion(position);
                if (cmp == 0)
                {
                    return (checking, (int)(position - buf.InitialIndex));
                }
                else if (cmp > 0)
                {
                    imin = checking + 1;
                }
                else
                {
                    imax = checking - 1;
                }
            } while (imax >= imin);
            return (-1, -1);    //Not found
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCache{TSample}"/> class.
        /// </summary>
        /// <param name="allocationUnit">The allocation unit.</param>
        public DataCache(int allocationUnit = 1024)
        {
            buffers = new List<BufferInstance>();
            if (allocationUnit <= 0) throw new ArgumentOutOfRangeException(nameof(allocationUnit), "The allocationUnit must be greater than 1!");
            allocUnit = allocationUnit;
            allocUnitDivisor = new UInt32Divisor((uint)allocUnit);
            var buf = new BufferInstance(new TSample[allocUnit], 0);
            buffers.Add(buf);
        }

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="buffer">The destination.</param>
        /// <returns>
        /// The number of <typeparamref name="TSample"/>s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
        public ReadResult Read(Span<TSample> buffer)
        {
            this.ThrowIfDisposed(disposedValue);
            int wlen = 0;
            ulong position = ReadPosition;
            var p = FindBufferIndex(position);
            var destRem = buffer;
            while (!destRem.IsEmpty && destRem.Length > 0 && p.buffer >= 0)
            {
                var src = buffers[p.buffer];
                src.ReadPosition = p.local;
                var h = src.Read(destRem);
                destRem = destRem.Slice(h.Length);
                position += (ulong)h.Length;
                wlen += h.Length;
                p = FindBufferIndex(position);
            }
            ReadPosition += (uint)wlen;
            return wlen;
        }

        /// <summary>
        /// Clears this <see cref="DataCache{TSample}"/>.
        /// </summary>
        /// <param name="fillWithDefault">The value which indicates whether this must fill the internal buffer with default value, or not.</param>
        public void Clear(bool fillWithDefault = false)
        {
            BytesWritten = 0;
            ReadPosition = 0;
            if (fillWithDefault)
            {
                foreach (var item in buffers)
                {
                    item.WriteHead.Span.QuickFill(default);
                }
            }
        }

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte" />s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
#pragma warning disable CS1998

        public async ValueTask<ReadResult> ReadAsync(Memory<TSample> destination) => Read(destination.Span);

#pragma warning restore CS1998

        /// <summary>
        /// Writes the data inside specified buffer to this instance.
        /// </summary>
        /// <param name="buffer">The data buffer.</param>
        public void Write(ReadOnlySpan<TSample> buffer)
        {
            this.ThrowIfDisposed(disposedValue);
            ulong position = BytesWritten;
            var p = FindBufferIndex(position);
            var srcBuffer = buffer;
            while (!srcBuffer.IsEmpty && srcBuffer.Length > 0)
            {
                if (p.buffer < 0)   //Buffer exceeded
                {
                    uint size = allocUnitDivisor.FloorRem((uint)srcBuffer.Length, out var g) + (g > 0 ? allocUnitDivisor.Divisor : 0);
                    var buf = new BufferInstance(
                        new TSample[size]
                        , buffers.Last().NextIndex);
                    buffers.Add(buf);
                    p = FindBufferIndex(position);
                    continue;
                }
                var dest = buffers[p.buffer];
                var h = dest.Write(srcBuffer);
                srcBuffer = srcBuffer.Slice(h);
                position += (ulong)h;
                p = FindBufferIndex(position);
            }
            BytesWritten += (ulong)buffer.Length;
        }

        /// <summary>
        /// Copies all of the content of <see cref="DataCache{TSample}"/> to the specified <paramref name="sink"/>.
        /// </summary>
        /// <param name="sink">The sink.</param>
        public void CopyTo(IDataSink<TSample> sink)
        {
            foreach (var item in buffers)
            {
                item.ReadPosition = 0;
                sink.Write(item.ReadHead.Span);
            }
        }

        /// <summary>
        /// Asynchronously writes the data inside specified buffer to this instance.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        public async ValueTask WriteAsync(ReadOnlyMemory<TSample> buffer) => Write(buffer.Span);

        /// <summary>
        /// Seeks the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="origin">The origin.</param>
        public void Seek(long offset, SeekOrigin origin)
        {
            var rpos = ReadPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    rpos = 0;
#pragma warning disable S907 // "goto" statement should not be used
                    goto case SeekOrigin.Current;
                case SeekOrigin.End:
                    rpos = BytesWritten;
                    goto case SeekOrigin.Current;
#pragma warning restore S907 // "goto" statement should not be used
                case SeekOrigin.Current:
                    if (offset >= 0)
                    {
                        rpos += (ulong)offset;
                    }
                    else
                    {
                        unchecked
                        {
                            rpos -= (ulong)-offset;
                        }
                    }
                    break;
            }
            if (rpos < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "The result of seeking is less than 0!");
            }
            ReadPosition = rpos;
        }

#pragma warning restore CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        /// <summary>
        /// Seeks this data source to the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void SeekTo(ulong index) => ReadPosition = index;

        /// <summary>
        /// Skips this data source the specified number of elements to skip.
        /// </summary>
        /// <param name="step">The number of elements to skip.</param>
        public void Skip(ulong step) => ReadPosition += step;

        /// <summary>
        /// Steps this data source the specified step back in frames.
        /// </summary>
        /// <param name="step">The number of frames to step back.</param>
        public void StepBack(ulong step) => throw new NotImplementedException();

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> to the specified index in frames from the end of stream.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public void SeekLast(ulong offset) => throw new NotImplementedException();

        private bool disposedValue = false;

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //buffers = null;
                }
                //
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DataCache{TSample}"/> class.
        /// </summary>
        ~DataCache()
        {
            Dispose(false);
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
