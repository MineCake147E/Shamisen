using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Benchmarks
{
    public class DummySourceCSCore : CSCore.IWaveSource
    {
        private bool disposedValue;

        public DummySourceCSCore(CSCore.WaveFormat waveFormat) => WaveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));

        public bool CanSeek { get => false; }

        public CSCore.WaveFormat WaveFormat { get; }

        public long Position { get => 0; set => _ = value; }

        public long Length { get => 0; }

        public int Read(byte[] buffer, int offset, int count) => count;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
