using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Modifier;

namespace Shamisen.Data
{
    /// <summary>
    /// Writes some data with specified offset.
    /// </summary>
    public sealed class OffsetSeekableDataSink<TSample> : IDataSink<TSample> where TSample : unmanaged
    {
        private bool disposedValue;
        private IDataSink<TSample>? sink;

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetSeekableDataSink{TSample}"/> class.
        /// </summary>
        /// <param name="sink">The sink.</param>
        /// <param name="offset">The offset.</param>
        public OffsetSeekableDataSink(IDataSink<TSample>? sink, ulong offset)
        {
            this.sink = sink;
            Offset = offset;
        }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public ulong Offset { get; }

        /// <summary>
        /// Gets the asynchronous write support.
        /// </summary>
        /// <value>
        /// The asynchronous write support.
        /// </value>
        public IAsyncWriteSupport<TSample>? AsyncWriteSupport => Sink.AsyncWriteSupport;

        /// <summary>
        /// Gets the current size of written data.
        /// </summary>
        /// <value>
        /// The current size of written data.
        /// </value>
        public ulong? CurrentSize => Sink.CurrentSize - Offset;

        /// <summary>
        /// Gets the remaining space of this <see cref="IDataSink{TSample}" />.
        /// </summary>
        /// <value>
        /// The remaining space.
        /// </value>
        public ulong? RemainingSpace => Sink.RemainingSpace;

        /// <summary>
        /// Gets the maximum size of this <see cref="IDataSink{TSample}" />.<br />
        /// The <see langword="null" /> means that maximum size is either not available, beyond <see cref="ulong.MaxValue" />, or infinity.
        /// </summary>
        /// <value>
        /// The maximum size.
        /// </value>
        public ulong? MaxSize => Sink.MaxSize;

        /// <summary>
        /// Gets the current position of this <see cref="IDataSink{TSample}" />.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong? Position => Sink.Position - Offset;

        /// <summary>
        /// Gets the skip support of this <see cref="IDataSink{TSample}" />.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => Sink.SkipSupport;

        /// <summary>
        /// Gets the seek support of this <see cref="IDataSink{TSample}" />.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => Sink.SeekSupport is { } seek ? new OffsetSeekSupport(seek, Offset) : null;

        /// <summary>
        /// Gets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        IDataSink<TSample> Sink => sink ?? throw new ObjectDisposedException(nameof(OffsetSeekableDataSink<TSample>));

        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The writing data.</param>
        public void Write(ReadOnlySpan<TSample> data) => Sink.Write(data);

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                sink = null;
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
