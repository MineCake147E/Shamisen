using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a wave format.
    /// </summary>
    public interface IWaveFormat : IAudioFormat<byte>
    {
        /// <summary>
        /// Gets the value indicates how the samples are encoded.
        /// </summary>
        /// <value>
        /// The sample encoding.
        /// </value>
        AudioEncoding Encoding { get; }
    }
}
