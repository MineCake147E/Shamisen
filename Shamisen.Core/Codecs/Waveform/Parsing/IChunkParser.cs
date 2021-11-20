using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Data;

namespace Shamisen.Codecs.Waveform.Parsing
{
    /// <summary>
    /// Parses a wave sub-chunk.
    /// </summary>
    public interface IChunkParser : IDisposable
    {
        /// <summary>
        /// Gets the chunk identifier.
        /// </summary>
        /// <value>
        /// The chunk identifier.
        /// </value>
        ChunkId ChunkId { get; }

        /// <summary>
        /// Gets the length of this chunk excluding header 8 bytes.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        ulong ChunkLength { get; }

        /// <summary>
        /// Gets the source chunk reader.
        /// </summary>
        /// <value>
        /// The source chunk reader.
        /// </value>
        IChunkReader Source { get; }
    }

    /// <summary>
    /// Parses a wave sub-chunk with certain data.
    /// </summary>
    /// <typeparam name="T">The type of stored data.</typeparam>
    /// <seealso cref="IDisposable" />
    public interface IChunkParserWithSingleData<out T> : IChunkParser
    {
        /// <summary>
        /// Gets the stored data.<br/>
        /// The data must be parsed at initialization.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        T Data { get; }
    }

    /// <summary>
    /// Parses a wave sub-chunk with certain multiple data.
    /// </summary>
    /// <typeparam name="T">The type of stored data.</typeparam>
    /// <seealso cref="IDisposable" />
    public interface IChunkParserWithMultipleData<T> : IChunkParser, IDataSource<T> where T : unmanaged
    {
    }
}
