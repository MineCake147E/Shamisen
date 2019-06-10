using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.SoundOut;
using MonoAudio.Interop;

namespace MonoAudio.SoundOut
{
    public sealed class CSCoreSoundOut : ISoundOut
    {
        /// <summary>
        /// Gets the actual output back-end.
        /// </summary>
        /// <value>
        /// The actual output back-end.
        /// </value>
        public CSCore.SoundOut.ISoundOut Out { get; }

        public PlaybackState PlaybackState => PlaybackStateUtils.ConvertPlaybackState(Out.PlaybackState);

        public void Initialize(IWaveSource source) => Out.Initialize(new MonoAudioWaveSourceWrapper(source));

        public void Pause() => Out.Pause();

        public void Play() => Out.Play();

        public void Resume() => Out.Resume();

        public void Stop() => Out.Stop();

        /// <summary>
        /// Initializes a new instance of the <see cref="CSCoreSoundOut"/> class.
        /// </summary>
        /// <param name="out">The output back-end.</param>
        /// <exception cref="ArgumentNullException">out</exception>
        public CSCoreSoundOut(CSCore.SoundOut.ISoundOut @out)
        {
            Out = @out ?? throw new ArgumentNullException(nameof(@out));
        }

        #region IDisposable Support

        private bool disposedValue = false;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Out.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// アンマネージ リソースの解放またはリセットに関連付けられているアプリケーション定義のタスクを実行します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
