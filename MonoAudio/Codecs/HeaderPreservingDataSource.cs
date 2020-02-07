using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MonoAudio.Data;
// NOT INCLUDED
namespace MonoAudio.Codecs
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="IDataSource" />
    public sealed class HeaderPreservingDataSource : IDataSource
    {
        private bool disposedValue = false;

        private MemoryStream cache;

        /// <summary>
        /// Gets a value indicating whether this <see cref="HeaderPreservingDataSource"/> is caching the <see cref="Read(Span{byte})"/> data.
        /// </summary>
        /// <value>
        ///   <c>true</c> if caching; otherwise, <c>false</c>.
        /// </value>
        public bool Caching { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderPreservingDataSource"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public HeaderPreservingDataSource(IDataSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            cache = new MemoryStream();
        }

        private IDataSource Source { get; set; }

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource" />.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong Position { get; private set; }

        /// <summary>
        /// Returns to the head of data.
        /// </summary>
        public void ReturnToHead() => Position = 0;

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte" />s read from this <see cref="IDataSource" />.
        /// </returns>
        public ReadResult Read(Span<byte> destination)
        {
            var l = Source.Read(destination);
        }

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte" />s read from this <see cref="IDataSource" />.
        /// </returns>
        public async ValueTask<ReadResult> ReadAsync(Memory<byte> destination)
        {
        }

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                    cache.Dispose();
                }
                cache = null;
                Source = null;
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
