using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Data;

namespace MonoAudio.Codecs.Waveform.Formats
{
    /// <summary>
    /// Defines a base infrastructure of a WAVE audio decoder.
    /// </summary>
    public interface IFormatDataParser : IWaveSource
    {
        /// <summary>
        /// Initializes the <see cref="IFormatDataParser"/> with the specified fact chunk.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="chunkReader">The chunk reader to read a fact chunk from.</param>
        void Initialize(IChunkReader chunkReader);
    }
}
