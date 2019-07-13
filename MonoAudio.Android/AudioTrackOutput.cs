using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Encoding = Android.Media.Encoding;

namespace MonoAudio.IO.Android
{
    /// <summary>
    /// Provides an <see cref="AudioTrack"/> output.
    /// </summary>
    /// <seealso cref="ISoundOut" />
    public sealed class AudioTrackOutput : ISoundOut
    {
        /// <summary>
        /// Gets the state of the playback.
        /// </summary>
        /// <value>
        /// The state of the playback.
        /// </value>
        public PlaybackState PlaybackState { get; private set; }

        /// <summary>
        /// Initializes the <see cref="T:MonoAudio.IO.ISoundOut" /> for playing a <paramref name="source" />.
        /// </summary>
        /// <param name="source">The source to play.</param>
        public void Initialize(IWaveSource source)
        {
            AudioTrack player = new AudioTrack.Builder()
         .SetAudioAttributes(new AudioAttributes.Builder()
                  .SetUsage(AudioUsageKind.Game)
                  .SetContentType(AudioContentType.Music)
                  .Build())
         .SetAudioFormat(new AudioFormat.Builder()
                 .SetEncoding(Encoding.PcmFloat)
                 .SetSampleRate(192000)
                 .SetChannelMask(ChannelOut.Stereo)
                 .Build())
         .SetBufferSizeInBytes(4096)
         .Build();
        }

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        public void Pause()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts the audio playback.
        /// </summary>
        public void Play()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void Stop()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support

        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AudioTrackOutput"/> class.
        /// </summary>
        ~AudioTrackOutput()
        {
            Dispose(false);
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
