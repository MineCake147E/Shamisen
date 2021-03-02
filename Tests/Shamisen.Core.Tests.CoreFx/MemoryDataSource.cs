using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Core.Tests.CoreFx
{
    public class MemoryDataSource : IReadableDataSource<byte>, IAsyncReadSupport<byte>
    {
        private bool disposedValue;

        public IReadSupport<byte> ReadSupport => this;

        public IAsyncReadSupport<byte> AsyncReadSupport { get; }

        public ulong? Length { get; }

        public ulong? TotalLength { get; }

        public ulong? Position => unchecked((ulong)memory.Position);

        public ISkipSupport SkipSupport { get; }

        public ISeekSupport SeekSupport { get; }

        private MemoryStream memory;

        public MemoryDataSource(MemoryStream memory)
        {
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));
        }

        public ReadResult Read(Span<byte> buffer)
        {
            var h = memory.Read(buffer);
            return h < 1 ? ReadResult.WaitingForSource : h;
        }

        public async ValueTask<ReadResult> ReadAsync(Memory<byte> destination)
        {
            var h = await memory.ReadAsync(destination);
            return h < 1 ? ReadResult.WaitingForSource : h;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    memory.Dispose();
                    memory = null;
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
