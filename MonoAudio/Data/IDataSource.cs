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
    public interface ISkippableDataSource<TSample> : IDataSource<TSample>, ISkipSupport where TSample : unmanaged
    {
    }

    /// <summary>
    /// Defines a base infrastructure of a source of binary data, which supports seeking.
    /// </summary>
    /// <seealso cref="IDataSource{TSample}" />
    public interface ISeekableDataSource<TSample> : ISkippableDataSource<TSample>, ISeekSupport where TSample : unmanaged
    {
    }
}
