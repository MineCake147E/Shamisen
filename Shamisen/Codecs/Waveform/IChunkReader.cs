using System;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Codecs.Waveform
{
    /// <summary>
    /// Defines a base infrastructure of a chunk reader.
    /// </summary>
    /// <seealso cref="IDataSource{TSample}" />
    public interface IChunkReader : IReadableDataSource<byte>, ISkipSupport
    {
        /// <summary>
        /// Gets the current chunk identifier.
        /// </summary>
        /// <value>
        /// The chunk identifier.
        /// </value>
        ChunkId ChunkId { get; }

        /// <summary>
        /// Gets the total size of this chunk excluding the 8 bytes of header.
        /// </summary>
        /// <value>
        /// The total size.
        /// </value>
        ulong TotalSize { get; }

        /// <summary>
        /// Gets the length of remaining data in bytes.
        /// </summary>
        /// <value>
        /// The remaining bytes.
        /// </value>
        ulong RemainingBytes { get; }

        /// <summary>
        /// Gets the current sub chunk.
        /// </summary>
        /// <value>
        /// The current sub chunk.
        /// </value>
        IChunkReader? CurrentSubChunk { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can be read by <see cref="IReadSupport{TSample}.Read(Span{TSample})"/> and <see cref="IAsyncReadSupport{TSample}.ReadAsync(Memory{TSample})"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can publicly read; otherwise, <c>false</c>.
        /// </value>
        bool CanPubliclyRead { get; }

        /// <summary>
        /// Opens the stream for sub chunk.
        /// </summary>
        /// <returns></returns>
        IChunkReader ReadSubChunk();
    }
}
