using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Codecs.Waveform.Rf64;

namespace Shamisen.Codecs.Waveform.Parsing
{
    /// <summary>
    /// Decodes a ".wav" file from <see cref="IChunkReader"/>.<br/>
    /// The instance of <see cref="IWaveParser"/> must be ready to <see cref="IReadSupport{TSample}.Read(Span{TSample})"/> after the instance is initialized with its constructor.
    /// </summary>
    /// <seealso cref="IWaveSource" />
    public interface IWaveParser : IWaveSource
    {
        /// <summary>
        /// Gets the chunk parser factory.
        /// </summary>
        /// <value>
        /// The chunk parser factory.
        /// </value>
        IChunkParserFactory ChunkParserFactory { get; }

        /// <summary>
        /// Gets the source chunk reader.
        /// </summary>
        /// <value>
        /// The chunk reader.
        /// </value>
        IChunkReader ChunkReader { get; }
    }

    /// <summary>
    /// Decodes an RF64 ".wav" file from <see cref="IChunkReader"/>.
    /// </summary>
    /// <seealso cref="IWaveParser" />
    public interface IRf64Parser : IWaveParser, IRf64ChunkSizeContainer
    {
    }
}
