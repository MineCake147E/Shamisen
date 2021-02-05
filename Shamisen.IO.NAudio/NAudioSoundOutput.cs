using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Shamisen.IO
{
    /// <summary>
    /// Provides a functionality for playing sound using NAudio.
    /// </summary>
    /// <seealso cref="ISoundOut" />
    public sealed class NAudioSoundOutput : ISoundOut
    {
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="NAudioSoundOutput"/> class.
        /// </summary>
        /// <param name="backend">The backend.</param>
        /// <exception cref="ArgumentNullException">backend</exception>
        public NAudioSoundOutput(IWavePlayer backend)
        {
            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        /// <summary>
        /// Gets the actual backend.
        /// </summary>
        /// <value>
        /// The backend.
        /// </value>
        public IWavePlayer Backend { get; }

        /// <summary>
        /// Gets the state of the playback.
        /// </summary>
        /// <value>
        /// The state of the playback.
        /// </value>
        public PlaybackState PlaybackState => NAudioInteroperationUtils.ConvertPlaybackState(Backend.PlaybackState);

        /// <summary>
        /// Initializes the <see cref="ISoundOut" /> for playing a <paramref name="source" />.
        /// </summary>
        /// <param name="source">The source to play.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Initialize(IWaveSource source) => Backend.Init(new NAudioInteroperatingWaveSource(source));

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
        public void Resume() => Backend.Play();

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
