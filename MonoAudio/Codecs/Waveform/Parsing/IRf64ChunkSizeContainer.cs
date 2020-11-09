using System.Collections.Generic;

using MonoAudio.Codecs.Waveform.Rf64;

namespace MonoAudio.Codecs.Waveform.Parsing
{
    /// <summary>
    /// Defines a base structure that contains informations about the size of certain chunks.
    /// </summary>
    public interface IRf64ChunkSizeContainer
    {
        /// <summary>
        /// Gets the size of the RF64 chunk.
        /// </summary>
        /// <value>
        /// The size of the RF64 chunk.
        /// </value>
        ulong RiffSize { get; }

        /// <summary>
        /// Gets the size of the data chunk.
        /// </summary>
        /// <value>
        /// The size of the data chunk.
        /// </value>
        ulong DataSize { get; }

        /// <summary>
        /// Gets the sample count in the fact chunk.
        /// </summary>
        /// <value>
        /// The sample count in the fact chunk.
        /// </value>
        ulong SampleCount { get; }

        /// <summary>
        /// Gets the chunk size table.
        /// </summary>
        /// <value>
        /// The chunk size table.
        /// </value>
        IReadOnlyList<ChunkSizeTableEntry> ChunkSizeTable { get; }

        /// <summary>
        /// Gets the size for next chunk with specified <paramref name="id"/>.<br/>
        /// This function must not be called more than once for single chunk.
        /// </summary>
        /// <param name="id">The chunk identifier.</param>
        /// <returns></returns>
        ulong GetSizeForNextChunk(ChunkId id);
    }
}
