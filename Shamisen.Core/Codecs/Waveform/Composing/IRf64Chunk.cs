using System.Collections.Generic;

namespace Shamisen.Codecs.Waveform.Composing
{
    /// <summary>
    /// Defines a base infrastructure of an RF64 chunk.
    /// </summary>
    /// <seealso cref="IDataSource{TSample}" />
    public interface IRf64Chunk : IRf64Content
    {
        /// <summary>
        /// Gets the chunk identifier.
        /// </summary>
        /// <value>
        /// The chunk identifier.
        /// </value>
        ChunkId ChunkId { get; }

        /// <summary>
        /// Gets the size of the content excluding header.
        /// </summary>
        /// <value>
        /// The size of the content.
        /// </value>
        ulong ContentSize { get; }

        /// <summary>
        /// Gets the actual size including RIFF chunk header.
        /// </summary>
        /// <value>
        /// The actual size.
        /// </value>
        ulong ActualSize { get; }

        /// <summary>
        /// Gets the size of the riff.
        /// </summary>
        /// <value>
        /// The size of the riff.
        /// </value>
        uint RiffSize { get; }

        /// <summary>
        /// Gets the contents.
        /// </summary>
        /// <value>
        /// The contents.
        /// </value>
        IEnumerable<IRf64Content>? Contents { get; }
    }
}
