using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a readable audio source.
    /// </summary>
    /// <typeparam name="T">The type of audio data.</typeparam>
    /// <seealso cref="IAudioSource" />
    public interface IReadableAudioSource<T> : IAudioSource
    {
        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        int Read(Span<T> buffer);
    }
}
