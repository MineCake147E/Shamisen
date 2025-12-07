using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Shamisen.Data.Binary
{
    /// <summary>
    /// Provides utility methods for working with spans of bits.
    /// </summary>
    public static class BitSpanUtils
    {
        internal static ulong CalculateBitLengthChecked<TWord>(int bufferLength, byte start, byte lastWordInvalidBits) where TWord : unmanaged
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, Unsafe.SizeOf<TWord>() * 8);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lastWordInvalidBits, Unsafe.SizeOf<TWord>() * 8);
            ArgumentOutOfRangeException.ThrowIfNegative(bufferLength);
            var bufferTotalBits = (ulong)bufferLength * (ulong)Unsafe.SizeOf<TWord>() * 8ul;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, bufferTotalBits);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lastWordInvalidBits, bufferTotalBits);
            if (bufferLength <= 1)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((byte)(start + lastWordInvalidBits), (byte)(Unsafe.SizeOf<TWord>() * 8));
            }
            return checked(bufferTotalBits - start - lastWordInvalidBits);
        }

        internal static ulong CalculateBitLengthUnchecked<TWord>(int bufferLength, byte start, byte lastWordInvalidBits) where TWord : unmanaged
        {
            var bufferTotalBits = (ulong)bufferLength * (ulong)Unsafe.SizeOf<TWord>() * 8ul;
            return unchecked(bufferTotalBits - start - lastWordInvalidBits);
        }
    }
}
