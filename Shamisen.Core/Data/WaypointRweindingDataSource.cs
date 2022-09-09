using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data
{
    /// <summary>
    /// Rewinds the data source to the previously-specified way-point if needed.
    /// </summary>
    public sealed class WaypointRweindingDataSource<TSample> : IReadableDataSource<TSample> where TSample : unmanaged
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaypointRweindingDataSource{TSample}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public WaypointRweindingDataSource(IReadableDataSource<TSample> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            DataCache = new();
        }

        private IReadableDataSource<TSample> Source { get; }

        private DataCache<TSample> DataCache { get; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? Length => isRewinded ? Source.Length + (DataCache.Length - pinnedPosition) : Source.Length;

        /// <summary>
        /// Gets the total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? TotalLength => Source.TotalLength;

        /// <summary>
        /// Gets the position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? Position => isRewinded ? Source.Position - (DataCache.Length - pinnedPosition) : Source.Position;

        /// <summary>
        /// Gets the skip support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => null;

        /// <summary>
        /// Gets the seek support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => null;

        /// <summary>
        /// Gets the read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IReadSupport<TSample>? ReadSupport => this;

        /// <summary>
        /// Gets the asynchronous read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IAsyncReadSupport<TSample>? AsyncReadSupport => null;

        private volatile bool isPinned = false;
        private ulong pinnedPosition = 0;
        private volatile bool isRewinded = false;

        /// <summary>
        /// Pins a way-point to this instance.
        /// </summary>
        /// <returns></returns>
        public void Pin()
        {
            isPinned = true;
            if (!isRewinded || DataCache.Length <= 0)
            {
                DataCache.Clear();
            }
            pinnedPosition = DataCache.ReadPosition;
        }

        /// <summary>
        /// Unpins a way-point to this instance.
        /// </summary>
        /// <returns></returns>
        public void UnPin()
        {
            isPinned = false;
            DataCache.Clear();
        }

        /// <summary>
        /// Rewinds this instance to the way-point.
        /// </summary>
        /// <returns></returns>
        public void Rewind()
        {
            if (isPinned)
            {
                isRewinded = true;
                isPinned = false;
                DataCache.SeekTo(pinnedPosition);
            }
        }

        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<TSample> buffer)
        {
            if (isRewinded && DataCache.Length > 0)
            {
                var q = DataCache.Read(buffer);
                if (q.Length < buffer.Length)
                {
                    var rr = ReadFromSource(buffer.Slice(q.Length));
                    return rr.Length + q.Length;
                }
                else
                {
                    return q;
                }
            }
            else
            {
                isRewinded = false;
                return ReadFromSource(buffer);
            }
        }

        private ReadResult ReadFromSource(Span<TSample> buffer)
        {
            var rr = Source.Read(buffer);
            if (isPinned) DataCache.Write(buffer.SliceWhile(rr.Length));
            return rr;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

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
