using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Core.Tests.CoreFx
{
    public sealed class RandomDataSource : IReadableDataSource<byte>
    {
        private bool disposedValue;

        public ulong DebugID { get; }

        public bool DoDumpRead { get; }

        public IReadSupport<byte> ReadSupport => this;

        public IAsyncReadSupport<byte> AsyncReadSupport { get; }

        public ulong? Length { get; }

        public ulong? TotalLength { get; }

        public ulong? Position { get; }

        public ISkipSupport SkipSupport { get; }

        public ISeekSupport SeekSupport { get; }

        private ulong state;
        private readonly ulong increment;
        private const ulong Multiplier = 6364136223846793005u;

        public RandomDataSource(ulong seed, ulong streamId, ulong debugID, bool doDumpRead = false)
        {
            state = seed;
            DebugID = debugID;
            DoDumpRead = doDumpRead;
            increment = streamId | 1ul;
            byteBuffer = new byte[sizeof(uint)];
        }

        private byte[] byteBuffer;
        private Memory<byte> remainingBytes;

        private uint Next()
        {
            ulong x = state;
            int count = (int)(x >> 59);       // 59 = 64 - 5

            state = x * Multiplier + increment;
            x ^= x >> 18;                               // 18 = (64 - 27)/2
            return BitOperations.RotateRight((uint)(x >> 27), count);
        }

        private void ReadBytes(Span<byte> span)
        {
        }

        public ReadResult Read(Span<byte> destination)
        {
#if DEBUG
            if (DoDumpRead) Console.WriteLine($"Read Start({DebugID}): {DateTime.Now.TimeOfDay}");
            try
#endif
            {
                unchecked
                {
                    var rem = destination;
                    if (remainingBytes.Length > 0)
                    {
                        if (rem.Length < remainingBytes.Length)
                        {
                            remainingBytes.Span.SliceWhile(rem.Length).CopyTo(rem);
                            remainingBytes = remainingBytes.Slice(rem.Length);
                            return rem.Length;
                        }
                        else if (rem.Length == remainingBytes.Length)
                        {
                            remainingBytes.Span.CopyTo(rem);
                            remainingBytes = default;
                            return rem.Length;
                        }
                        else
                        {
                            remainingBytes.Span.CopyTo(rem);
                            rem = rem.Slice(remainingBytes.Length);
                            remainingBytes = default;
                        }
                    }
                    var j = MemoryUtils.CastSplit<byte, uint>(rem, out rem);
                    for (int i = 0; i < j.Length; i++)
                    {
                        j[i] = BinaryExtensions.ConvertToLittleEndian(Next());
                    }
                    if (remainingBytes.IsEmpty)
                    {
                        var h = MemoryMarshal.Cast<byte, uint>(byteBuffer.AsSpan());
                        for (int i = 0; i < h.Length; i++)
                        {
                            h[i] = BinaryExtensions.ConvertToLittleEndian(Next());
                        }
                        remainingBytes = byteBuffer.AsMemory();
                    }
                    if (!rem.IsEmpty)
                    {
                        remainingBytes.Span.SliceWhile(rem.Length).CopyTo(rem);
                        remainingBytes = remainingBytes.Slice(rem.Length);
                    }
                    return destination.Length;
                }
            }
#if DEBUG
            finally
            {
                if (DoDumpRead) Console.WriteLine($"Read End({DebugID}): {DateTime.Now.TimeOfDay}");
            }

#endif
        }

        public async ValueTask<ReadResult> ReadAsync(Memory<byte> destination) => Read(destination.Span);

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
