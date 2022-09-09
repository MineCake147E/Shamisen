using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace Shamisen.Interop
{
    public sealed class ShamisenWaveSourceWrapper : CSCore.IWaveSource
    {
        public IWaveSource Source { get; }

        public bool CanSeek => Source.CanSeek;

        public CSCore.WaveFormat WaveFormat { get; }

        public long Position { get => Source.Position; set => Source.Position = value; }

        public long Length => Source.Length;

        public int Read(byte[] buffer, int offset, int count) => Source.Read(new Span<byte>(buffer, offset, count));

        public ShamisenWaveSourceWrapper(IWaveSource source)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            WaveFormat = new CSCore.WaveFormat(Source.Format.SampleRate, Source.Format.BitDepth, Source.Format.Channels,
                                               (CSCore.AudioEncoding)(short)Source.Format.Encoding, Source.Format.ExtraSize);
        }

        #region IDisposable Support

        private bool disposedValue = false; //

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Source.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
