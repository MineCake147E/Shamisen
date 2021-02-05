using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Codecs.Waveform.Parsing.Chunks
{
    /// <summary>
    /// Defines a base infrastructure of a parser for "fmt " chunk.
    /// </summary>
    public interface IFormatChunkParser : IChunkParserWithSingleData<IWaveFormat>
    {
        /// <summary>
        /// Gets a value indicating whether the format requires a fact chunk.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the format requires a fact chunk; otherwise, <c>false</c>.
        /// </value>
        bool RequireFactChunk { get; }

        /// <summary>
        /// Gets the waveform chunk parser.
        /// </summary>
        /// <param name="chunkReader">The chunk reader.</param>
        /// <param name="rf64ChunkSizeContainer">The container of informations about the size of certain chunks.</param>
        /// <exception cref="InvalidOperationException">The <see cref="IFormatChunkParser"/> didn't parse a required "fact" chunk.</exception>
        /// <returns></returns>
        IWaveformChunkParser GetWaveformChunkParser(IChunkReader chunkReader, IRf64ChunkSizeContainer rf64ChunkSizeContainer);

        /// <summary>
        /// Parses the fact chunk.
        /// </summary>
        /// <param name="chunkReader">The chunk reader.</param>
        /// <returns></returns>
        void ParseFactChunk(IChunkReader chunkReader);
    }
}
