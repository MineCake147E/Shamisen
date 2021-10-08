using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Core.Tests.CoreFx.TestUtils
{
    public sealed class RandomWaveSource : IWaveSource
    {
        private bool disposedValue;

        public RandomWaveSource(IWaveFormat format, RandomDataSource source)
        {
            Format = format ?? throw new ArgumentNullException(nameof(format));
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public IWaveFormat Format { get; }
        public ulong? Length { get; }
        public ulong? TotalLength { get; }
        public ulong? Position { get; }
        public ISkipSupport SkipSupport { get; }
        public ISeekSupport SeekSupport { get; }
        private RandomDataSource Source { get; }

        public ReadResult Read(Span<byte> buffer) => Source.Read(buffer);

        private void Dispose(bool disposing)
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
