using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Data
{
    /// <summary>
    /// Defines a base infrastructure of a source of binary data.
    /// </summary>
    public interface IDataSource : IDisposable
    {
        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <see cref="byte"/>s read from this <see cref="IDataSource"/>.</returns>
        ReadResult Read(Span<byte> destination);

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <see cref="byte"/>s read from this <see cref="IDataSource"/>.</returns>
        ValueTask<ReadResult> ReadAsync(Memory<byte> destination);

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource"/>.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        ulong Position { get; }
    }

    /// <summary>
    /// Defines a base infrastructure of a source of binary data, which supports seeking.
    /// </summary>
    /// <seealso cref="IDataSource" />
    public interface ISeekableDataSource : IDataSource
    {
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        new ulong Position { get; set; }
    }
}
