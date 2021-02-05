using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Codecs.Waveform.Parsing.Chunks
{
    /// <summary>
    /// Parses a ".wav" file into waveform data.
    /// Applicable for chunks like <see cref="ChunkId.WaveList"/>, <see cref="ChunkId.Silent"/>, and <see cref="ChunkId.Data"/>
    /// </summary>
    /// <seealso cref="IChunkParser" />
    /// <seealso cref="IWaveSource" />
    public interface IWaveformChunkParser : IChunkParser, IWaveSource
    {
    }
}
