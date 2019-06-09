using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace MonoAudio.Interop
{
    public sealed class MonoAudioWaveSourceWrapper : CSCore.IWaveSource
    {
        public IWaveSource Source { get; }

        public bool CanSeek => Source.CanSeek;

        public CSCore.WaveFormat WaveFormat { get; }

        public long Position { get => Source.Position; set => Source.Position = value; }

        public long Length => Source.Length;

        public int Read(byte[] buffer, int offset, int count) => Source.Read(new Span<byte>(buffer, offset, count));

        public MonoAudioWaveSourceWrapper(IWaveSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            WaveFormat = new CSCore.WaveFormat(Source.Format.SampleRate, Source.Format.BitsPerSample, Source.Format.Channels, (AudioEncoding)Source.Format.AudioEncoding, Source.Format.FooterLength);
        }

        #region IDisposable Support

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Source.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MonoAudioWaveSourceWrapper()
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
