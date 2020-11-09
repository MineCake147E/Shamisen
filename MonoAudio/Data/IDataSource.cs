using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Data
{
    /// <summary>
    /// Defines a base infrastructure of a source of binary data.
    /// </summary>
    public interface IDataSource<TSample> : IDisposable where TSample : unmanaged
    {
        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <typeparamref name="TSample"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        ReadResult Read(Span<TSample> destination);

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <typeparamref name="TSample"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        ValueTask<ReadResult> ReadAsync(Memory<TSample> destination);

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        ulong Position { get; }
    }

    /// <summary>
    /// Defines a base infrastructure of a source of binary data, which supports skipping.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <seealso cref="IDataSource{TSample}" />
    public interface ISkippableDataSource<TSample> : IDataSource<TSample> where TSample : unmanaged
    {
        /// <summary>
        /// Skips this data source the specified number of elements to skip.
        /// </summary>
        /// <param name="numberOfElementsToSkip">The number of elements to skip.</param>
        void Skip(ulong numberOfElementsToSkip);
    }

    /// <summary>
    /// Defines a base infrastructure of a source of binary data, which supports seeking.
    /// </summary>
    /// <seealso cref="IDataSource{TSample}" />
    public interface ISeekableDataSource<TSample> : ISkippableDataSource<TSample> where TSample : unmanaged
    {
        /// <summary>
        /// Seeks this data source with the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="origin">The origin.</param>
        void Seek(long offset, SeekOrigin origin);

        /// <summary>
        /// Seeks this data source to the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        void SeekTo(ulong index);
    }
}
