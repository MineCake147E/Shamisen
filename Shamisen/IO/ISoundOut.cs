using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base infrastructure of a sound output.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public interface ISoundOut : IDisposable
    {
        /// <summary>
        /// Starts the audio playback.
        /// </summary>
        void Play();

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        void Resume();

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        void Stop();

        /// <summary>
        /// Initializes the <see cref="ISoundOut"/> for playing a <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to play.</param>
        void Initialize(IWaveSource source);

        /// <summary>
        /// Gets the state of the playback.
        /// </summary>
        /// <value>
        /// The state of the playback.
        /// </value>
        PlaybackState PlaybackState { get; }
    }
}
