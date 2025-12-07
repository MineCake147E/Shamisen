using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen.Data.Binary
{
    internal sealed class ConcatenatingBitPipeReadBuffer
    {
        private const int BitsPerWord = 8 * BytesPerWord;
        private const int BytesPerWord = sizeof(ulong);
        private const int DefaultCapacity = 131072 / BitsPerWord;
        private ulong[] buffer;
        private ulong consumedBits;
        private ulong lastWrittenByteIndex;
        private uint isReadInProgress = 0;

        public ConcatenatingBitPipeReadBuffer(int bufferCapacity = DefaultCapacity)
        {
            buffer = new ulong[bufferCapacity];
            consumedBits = 0;
            lastWrittenByteIndex = 0;
        }

        private Span<byte> GetWritableBytes(out ulong lastWrittenByteIndex)
        {
            var localLastWrittenByteIndex = this.lastWrittenByteIndex;
            lastWrittenByteIndex = localLastWrittenByteIndex;
            var wordSlice = buffer.AsSpan((int)(localLastWrittenByteIndex / sizeof(ulong)));
            var byteSlice = MemoryMarshal.AsBytes(wordSlice).Slice((int)(localLastWrittenByteIndex % sizeof(ulong)));
            return byteSlice;
        }

        public int Append(ReadOnlySpan<byte> data)
        {
            var byteSlice = GetWritableBytes(out var localLastWrittenByteIndex);
            data = data.SliceWhileIfLongerThan(byteSlice.Length);
            _ = data.TryCopyTo(byteSlice);
            lastWrittenByteIndex = (uint)data.Length + localLastWrittenByteIndex;
            return data.Length;
        }

        public int TryAppendAll(ReadOnlySpan<byte> data)
        {
            if (Interlocked.Exchange(ref isReadInProgress, 1) == 0)
            {
                ShiftWordsToHead();
                Volatile.Write(ref isReadInProgress, 0);
            }
            return Append(data);
        }

        private void ShiftWordsToHead()
        {
            var localConsumedBits = consumedBits;
            var span = buffer.AsSpan();
            var wordSlice = span.Slice((int)(localConsumedBits / BitsPerWord));
            _ = wordSlice.TryCopyTo(span);
            consumedBits = localConsumedBits % BitsPerWord;
        }
    }
}
