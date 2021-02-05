using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Codecs.Waveform.Parsing.Chunks;
using Shamisen.Codecs.Waveform.Riff;

namespace Shamisen.Codecs.Waveform.Parsing
{
    /// <summary>
    /// Defines a base infrastructure of a chunk-parser factory.
    /// </summary>
    public interface IChunkParserFactory
    {
        /// <summary>
        /// Gets the chunk parser for the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="chunkReader">The source chunk reader.</param>
        /// <returns></returns>
        IChunkParser GetChunkParser(ChunkId id, IChunkReader chunkReader);

        /// <summary>
        /// Gets the format chunk parser.
        /// </summary>
        /// <param name="chunkReader">The chunk reader.</param>
        /// <returns></returns>
        IFormatChunkParser GetFormatChunkParser(IChunkReader chunkReader);
    }
}
