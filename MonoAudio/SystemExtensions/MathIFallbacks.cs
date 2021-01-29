using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Numerics;
using System.Buffers.Binary;

namespace MonoAudio
{
    /// <summary>
    /// WARNING: This class is exposed only for testing purposes.<br/>
    /// Provides constants and static methods for bitwise, arithmetic, and other common mathematical functions as MathI does.<br/>
    /// This class contains hand-written software fallbacks for the places that the HW Intrinsics are not available.<br/>
    ///
    /// </summary>
    public static class MathIFallbacks
    {
        private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
        {
                //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
                0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
                31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int TrailingZeroCount(uint value)
        {
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
        }

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int TrailingZeroCount(ulong value)
        {
            unchecked
            {
                return (uint)value == 0 ? TrailingZeroCount((uint)value) : 32 + TrailingZeroCount((uint)(value >> 32));
            }
        }

        /// <summary>
        /// Finds last 1's position from LSB.<br/>
        /// When the value is 0, it returns 0.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int LogBase2(uint value)
        {
            unchecked
            {
                return 31 - LeadingZeroCount(value | 1);
            }
        }

        /// <summary>
        /// Finds last 1's position from LSB.<br/>
        /// When the value is 0, it returns 0.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int LogBase2(ulong value)
        {
            unchecked
            {
                return 63 - LeadingZeroCount(value | 1);
            }
        }

        /// <summary>
        /// Finds last 0's position from MSB.<br/>
        /// When the value is 0, it returns 32.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int LeadingZeroCount(uint value)
        {
            unchecked
            {
                value |= value >> 1;
                value |= value >> 2;
                value |= value >> 4;
                value |= value >> 8;
                value |= value >> 16;
                return 32 - TrailingZeroCount(~value);
            }
        }

        /// <summary>
        /// Finds last 0's position from MSB.<br/>
        /// When the value is 0, it returns 64.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int LeadingZeroCount(ulong value)
        {
            unchecked
            {
                value |= value >> 1;
                value |= value >> 2;
                value |= value >> 4;
                value |= value >> 8;
                value |= value >> 16;
                value |= value >> 32;
                return 64 - TrailingZeroCount(~value);
            }
        }

        /// <summary>
        /// Returns the largest power-of-two number less than or equals to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ExtractHighestSetBit(uint value)
        {
            unchecked
            {
                value |= value >> 1;
                value |= value >> 2;
                value |= value >> 4;
                value |= value >> 8;
                value |= value >> 16;
                return value ^ value >> 1;
            }
        }

        /// <summary>
        /// Returns the largest power-of-two number less than or equals to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ExtractHighestSetBit(ulong value)
        {
            unchecked
            {
                value |= value >> 1;
                value |= value >> 2;
                value |= value >> 4;
                value |= value >> 8;
                value |= value >> 16;
                value |= value >> 32;
                return value ^ value >> 1;
            }
        }

        /// <summary>
        /// Reverses the bits of the specified value in 32bit.
        /// </summary>
        /// <param name="value">The value to reverse bit order.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ReverseBitOrder(uint value)
        {
            //Reference: https://graphics.stanford.edu/~seander/bithacks.html#ReverseParallel and its Clang 12.0 codegen on x86-64
            value = ((value & 0x5555_5555) * 2) + ((value >> 1) & 0x5555_5555);
            value = ((value & 0x3333_3333) * 4) + ((value >> 2) & 0x3333_3333);
            value = ((value & 0x0f0f_0f0f) << 4) | ((value >> 4) & 0x0f0f_0f0f);
            return BinaryPrimitives.ReverseEndianness(value);
        }

        /// <summary>
        /// Reverses the bits of the specified value in 64bit.
        /// </summary>
        /// <param name="value">The value to reverse bit order.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ReverseBitOrder(ulong value)
        {
            value = ((value & 0x5555_5555_5555_5555) * 2) + ((value >> 1) & 0x5555_5555_5555_5555);
            value = ((value & 0x3333_3333_3333_3333) * 4) + ((value >> 2) & 0x3333_3333_3333_3333);
            value = ((value & 0x0f0f_0f0f_0f0f_0f0f) << 4) | ((value >> 4) & 0x0f0f_0f0f_0f0f_0f0f);
            return BinaryPrimitives.ReverseEndianness(value);
        }
    }
}
