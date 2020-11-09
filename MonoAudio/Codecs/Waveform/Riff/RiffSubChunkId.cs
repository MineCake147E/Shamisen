using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Codecs.Waveform.Riff
{
    /// <summary>
    /// Represents a RIFF sub-chunk ID for waveform file format in Little Endian.
    /// </summary>
    public enum RiffSubChunkId : uint
    {
        /// <summary>
        /// The wave sub chunk.
        /// </summary>
        Wave = 0x4556_4157,
    }
}
