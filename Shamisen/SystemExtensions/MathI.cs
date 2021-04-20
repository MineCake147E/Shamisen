using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Numerics;
using DivideSharp;
using DSUtils = DivideSharp.Utils;

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif
#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif

namespace Shamisen
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
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int FloorStep(int value, int step) => value - (value % step);

        /// <summary>
        /// Aligns the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
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

        #region Abs

        /// <summary>
        /// Returns the absolute value of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static ulong Abs(long value) => DSUtils.Abs(value);

        /// <summary>
        /// Returns the absolute value of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static uint Abs(int value) => DSUtils.Abs(value);

        #endregion Abs

        #region BigMul Polyfill

        /// <summary>
        /// Multiplies the specified <paramref name="x"/> and <paramref name="y"/> and returns the high part of whole 128bit result.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong low, long high) BigMul(long x, long y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                var high = Math.BigMul(x, y, out var low);
                return ((ulong)low, high);
#else
                var (low, high) = BigMul((ulong)x, (ulong)y);
                return (low, (long)high - ((x >> 63) & y) - ((y >> 63) & x));
#endif
            }
        }

        /// <summary>
        /// Multiplies the specified <paramref name="x"/> and <paramref name="y"/> and returns the high part of whole 128bit result.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong low, ulong high) BigMul(ulong x, ulong y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                var high = Math.BigMul(x, y, out var low);
                return (low, high);
#else
                return (x * y, DSUtils.MultiplyHigh(x, y));
#endif
            }
        }

        #endregion BigMul Polyfill

        #region ReadResult functions

        /// <summary>
        /// Determines the maximum of the parameters.
        /// </summary>
        /// <param name="a">The value a.</param>
        /// <param name="b">The value b.</param>
        /// <returns></returns>
        public static ReadResult Max(ReadResult a, ReadResult b) => a > b ? a : b;

        #endregion ReadResult functions

        #region TrailingZeroCount

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int TrailingZeroCount(uint value)
        {
#if NETCOREAPP3_1_OR_GREATER
#pragma warning disable IDE0022
            return BitOperations.TrailingZeroCount(value);
#pragma warning restore IDE0022
#else
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
                return MathIFallbacks.TrailingZeroCount(value);
            }
#endif
        }

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int TrailingZeroCount(ulong value)
        {
#if NETCOREAPP3_1_OR_GREATER
#pragma warning disable IDE0022
            return BitOperations.TrailingZeroCount(value);
#pragma warning restore IDE0022
#else
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
                return MathIFallbacks.TrailingZeroCount(value);
            }
#endif
        }

        #endregion TrailingZeroCount

        #region LogBase2

        /// <summary>
        /// Finds last 1's position from LSB.<br/>
        /// When the value is 0, it returns 0.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int LogBase2(uint value)
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return BitOperations.Log2(value);
#else
                return MathIFallbacks.LogBase2(value);
#endif
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
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return BitOperations.Log2(value);
#else
                return MathIFallbacks.LogBase2(value);
#endif
            }
        }

        #endregion LogBase2

        #region LeadingZeroCount

        /// <summary>
        /// Finds last 0's position from MSB.<br/>
        /// When the value is 0, it returns 32.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int LeadingZeroCount(uint value)
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return BitOperations.LeadingZeroCount(value);
#else
                return MathIFallbacks.LeadingZeroCount(value);
#endif
            }
        }

        /// <summary>
        /// Finds last 0's position from MSB.<br/>
        /// When the value is 0, it returns 32.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int LeadingZeroCount(ulong value)
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return BitOperations.LeadingZeroCount(value);
#else
                return MathIFallbacks.LeadingZeroCount(value);
#endif
            }
        }

        #endregion LeadingZeroCount

        #region PopCount

        /// <summary>
        /// Counts how many the bits are 1.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int Pop(uint value)
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return BitOperations.PopCount(value);
#else
                return MathIFallbacks.PopCount(value);
#endif
            }
        }

        /// <summary>
        /// Counts how many the bits are 1.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int PopCount(ulong value)
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return BitOperations.PopCount(value);
#else
                return MathIFallbacks.PopCount(value);
#endif
            }
        }

        #endregion PopCount

        #region ExtractHighestSetBit

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
#if NETCOREAPP3_1_OR_GREATER
                return (uint)(0x8000_0000ul >> BitOperations.LeadingZeroCount(value));
#else
                return MathIFallbacks.ExtractHighestSetBit(value);
#endif
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
#if NETCOREAPP3_1_OR_GREATER
                return value == 0 ? 0 : (uint)(0x8000_0000_0000_0000ul >> BitOperations.LeadingZeroCount(value));
#else
                return MathIFallbacks.ExtractHighestSetBit(value);
#endif
            }
        }

        #endregion ExtractHighestSetBit

        #region ReverseBitOrder

        /// <summary>
        /// Reverses the bit order of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ReverseBitOrder(uint value)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (ArmBase.IsSupported)
                {
                    return ArmBase.ReverseElementBits(value);
                }
#endif
                return MathIFallbacks.ReverseBitOrder(value);
            }
        }

        /// <summary>
        /// Reverses the bit order of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ReverseBitOrder(ulong value)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (ArmBase.Arm64.IsSupported)
                {
                    return ArmBase.Arm64.ReverseElementBits(value);
                }
#endif
                return MathIFallbacks.ReverseBitOrder(value);
            }
        }

        #endregion ReverseBitOrder

        #region ExtractBitField

        /// <summary>
        /// Extracts the bit field inside <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="start">The start from LSB.</param>
        /// <param name="length">The length in bits.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ExtractBitField(uint value, byte start, byte length)
        {
            if (start == 0) return value & ~(~0u << length);
#if NETCOREAPP3_1_OR_GREATER
            if (Bmi1.IsSupported)
            {
                return Bmi1.BitFieldExtract(value, start, length);
            }
#endif
            return (value >> start) & ~(~0u << length);
        }

        /// <summary>
        /// Extracts the bit field inside <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="start">The start from LSB.</param>
        /// <param name="length">The length in bits.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ExtractBitField(ulong value, byte start, byte length)
        {
            if (start == 0) return value & ~(~0ul << length);
#if NETCOREAPP3_1_OR_GREATER
            if (Bmi1.X64.IsSupported)
            {
                return Bmi1.X64.BitFieldExtract(value, start, length);
            }
#endif
            return (value >> start) & ~(~0ul << length);
        }

        #endregion ExtractBitField
    }
}
