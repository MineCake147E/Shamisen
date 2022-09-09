using System;
using ICSCoreSoundOut = CSCore.SoundOut.ISoundOut;

namespace Shamisen.IO
{
    /// <summary>
    /// Provides a functionality for playing sound using CSCore.
    /// </summary>
    /// <seealso cref="ISoundOut" />
    public sealed class CSCoreSoundOutput : ISoundOut
    {
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSCoreSoundOutput"/> class.
        /// </summary>
        /// <param name="backend">The actual backend.</param>
        /// <exception cref="ArgumentNullException">backend</exception>
        public CSCoreSoundOutput(ICSCoreSoundOut backend)
        {
            ArgumentNullException.ThrowIfNull(backend);
            Backend = backend;
        }

        /// <summary>
        /// Gets the actual backend.
        /// </summary>
        /// <value>
        /// The backend.
        /// </value>
        public ICSCoreSoundOut Backend { get; private set; }

        /// <summary>
        /// Gets the state of the playback.
        /// </summary>
        /// <value>
        /// The state of the playback.
        /// </value>
        public PlaybackState PlaybackState => CSCoreInteroperationUtils.ConvertPlaybackState(Backend.PlaybackState);

        /// <summary>
        /// Initializes the <see cref="ISoundOut" /> for playing a <paramref name="source" />.
        /// </summary>
        /// <param name="source">The source to play.</param>
        public void Initialize(IWaveSource source) => Backend.Initialize(new CSCoreInteroperatingWaveSource(source));

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        public void Pause() => Backend.Pause();

        /// <summary>
        /// Starts the audio playback.
        /// </summary>
        public void Play() => Backend.Play();

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume() => Backend.Resume();

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void Stop() => Backend.Stop();

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    Backend.Dispose();

                Backend = null;

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
