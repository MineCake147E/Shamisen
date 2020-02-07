using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Data
{
    /// <summary>
    /// Caches data into managed memory region.
    /// </summary>
    /// <seealso cref="IDisposable" />
    [Obsolete("Undone!")]
    public sealed partial class DataCache : ISeekableDataSource
    {
        private int nextBufferSize = 8;

        /// <summary>
        /// Gets or sets the READING position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        ulong ISeekableDataSource.Position { get => ReadPosition; set => ReadPosition = value; }

        /// <summary>
        /// Gets the bytes written inside internal buffer.
        /// </summary>
        /// <value>
        /// The bytes written.
        /// </value>
        public ulong BytesWritten { get; private set; }

        /// <summary>
        /// Gets or sets the current reading position.
        /// </summary>
        /// <value>
        /// The read position.
        /// </value>
        public ulong ReadPosition { get; set; }

        /// <summary>
        /// Gets the READING position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        ulong IDataSource.Position => (this as ISeekableDataSource).Position;

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte" />s read from this <see cref="IDataSource" />.
        /// </returns>
        public ReadResult Read(Span<byte> destination)
        {
            this.ThrowIfDisposed(disposedValue);
            int wlen = 0;
            var destRem = destination;
            while (destRem.Length > 0)
            {
                throw new NotImplementedException();
            }
            ReadPosition += (uint)wlen;
            return wlen;
        }

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte" />s read from this <see cref="IDataSource" />.
        /// </returns>
#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        public async ValueTask<ReadResult> ReadAsync(Memory<byte> destination) => Read(destination.Span);

#pragma warning restore CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        /// <summary>
        /// Writes the data inside specified buffer to this instance.
        /// </summary>
        /// <param name="buffer">The data buffer.</param>
        public void Write(ReadOnlySpan<byte> buffer)
        {
            this.ThrowIfDisposed(disposedValue);
            while (buffer.Length > 0)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Asynchronously writes the data inside specified buffer to this instance.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer) => Write(buffer.Span);

#pragma warning restore CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます
    }

    public sealed partial class DataCache : IDisposable
    {
        private bool disposedValue = false;

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                //
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DataCache"/> class.
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
