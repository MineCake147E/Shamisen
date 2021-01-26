using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Numerics;
using MonoAudio;

namespace System
{
    /// <summary>
    /// Provides constants and static methods for bitwise, arithmetic, and other common mathematical functions.
    /// </summary>
    public static partial class MathI
    {
        /// <summary>
        /// Aligns the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorStep(int value, int step) => value - (value % step);

        /// <summary>
        /// Aligns the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int newLength, int remainder) FloorStepRem(int value, int step)
        {
            var m = value % step;
            return (value - m, m);
        }

        /// <summary>
        /// Rectifies the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to rectify.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int Rectify(int value)
        {
            var h = value >> 31;
            return value & ~h;
        }

#if !(NET5_0 || NETCOREAPP3_1)
        private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };
#endif

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountConsecutiveZeros(uint value)
        {
#if NET5_0 || NETCOREAPP3_1
#pragma warning disable IDE0022
            return BitOperations.TrailingZeroCount(value);
#pragma warning restore IDE0022
#else
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
                if (value == 0)
                {
                    return 32;
                }
                long v2 = -value;
                var index = (((uint)v2 & value) * 0x077C_B531u) >> 27;

                return Unsafe.AddByteOffset(
                    ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn),
                    (IntPtr)(int)index);
            }
#endif
        }

        /// <summary>
        /// Finds last 1's position from LSB.<br/>
        /// When the value is 0, it returns 0.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int LogBase2(uint value)
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
#if NET5_0 || NETCOREAPP3_1
                return BitOperations.Log2(value);
#else
                var v = value;
                v |= v >> 1;
                v |= v >> 2;
                v |= v >> 4;
                v |= v >> 8;
                v |= v >> 16;
                var index = (v * 0x07C4ACDDU) >> 27;

                return Unsafe.AddByteOffset(
                    ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn),
                    (IntPtr)(int)index);
#endif
            }
        }

        /// <summary>
        /// Returns the largest power-of-two number less than or equals to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static uint LargestPowerOfTwoLessThanOrEqualsTo(uint value) => 1u << LogBase2(value);
    }
}
