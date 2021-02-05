using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Core.Tests.CoreFx
{
    public class DummyDataSource : IDataSource<byte>
    {
        private bool disposedValue;

        public ulong Position => unchecked((ulong)memory.Position);

        private MemoryStream memory;

        public DummyDataSource(MemoryStream memory)
        {
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));
        }

        public ReadResult Read(Span<byte> destination)
        {
            var h = memory.Read(destination);
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
