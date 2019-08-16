using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a readable audio source.
    /// </summary>
    /// <typeparam name="TSample">The type of audio data.</typeparam>
    /// <typeparam name="TFormat">The format of audio data.</typeparam>
    /// <seealso cref="IAudioSource{TSample,TFormat}" />
    public interface IReadableAudioSource<TSample, TFormat> : IAudioSource<TSample, TFormat> where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        int Read(Span<TSample> buffer);
    }
}
