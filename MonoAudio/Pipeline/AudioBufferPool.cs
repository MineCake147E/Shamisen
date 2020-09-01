using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Pipeline
{
    /// <summary>
    /// Provides some audio buffers to a single pipeline.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public sealed class AudioBufferPool<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format { get; }
    }
}
