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

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Out.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~CSCoreSoundOut()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
