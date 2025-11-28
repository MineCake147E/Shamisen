namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base infrastructure of playback controller.
    /// </summary>
    public interface IPlaybackController
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
        /// Gets the state of the playback.
        /// </summary>
        /// <value>
        /// The state of the playback.
        /// </value>
        PlaybackState PlaybackState { get; }
    }
}