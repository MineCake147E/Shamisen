using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen.Data.Binary
{
    public readonly ref partial struct ReadOnlyBitSpan
    {
        public bool TryReadBits(byte bitsToRead, out byte value)
        {
            var discardedTopBits = (int)startOffset;
            var lowerValidBits = -discardedTopBits & 7;
            ref var localHead = ref Unsafe.AsRef(in head);
            var localBitLength = bitLength;
            uint result = 0;
            uint mask = (1u << bitsToRead) - 1;
            bool success = bitsToRead <= 8u && bitsToRead < localBitLength;
            if (success && bitsToRead > 0)
            {
                result = (uint)localHead << 24;
                if (bitsToRead > lowerValidBits)
                {
                    result |= (uint)Unsafe.ReadUnaligned<byte>(ref Unsafe.Add(ref localHead, 1)) << 16;
                }
                result <<= discardedTopBits;
                result >>= -bitsToRead;
            }
            result &= mask;
            value = (byte)result;
            return success;
        }
    }
}
