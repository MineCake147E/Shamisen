using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Numerics;

#if NET5_0 || NETCOREAPP3_1

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif
#if NET5_0

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
#if NET5_0 || NETCOREAPP3_1
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
#if NET5_0 || NETCOREAPP3_1
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
#if NET5_0 || NETCOREAPP3_1
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
#if NET5_0 || NETCOREAPP3_1
                return BitOperations.Log2(value);
#else
                return MathIFallbacks.LogBase2(value);
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
        public static int LeadingZeroCount(uint value)
        {
            //https://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
            unchecked
            {
#if NET5_0 || NETCOREAPP3_1
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
#if NET5_0 || NETCOREAPP3_1
                return BitOperations.LeadingZeroCount(value);
#else
                return MathIFallbacks.LeadingZeroCount(value);
#endif
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
#if NET5_0 || NETCOREAPP3_1
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
#if NET5_0 || NETCOREAPP3_1
                return value == 0 ? 0 : (uint)(0x8000_0000_0000_0000ul >> BitOperations.LeadingZeroCount(value));
#else
                return MathIFallbacks.ExtractHighestSetBit(value);
#endif
            }
        }

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
#if NET5_0
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
#if NET5_0
                if (ArmBase.Arm64.IsSupported)
                {
                    return ArmBase.Arm64.ReverseElementBits(value);
                }
#endif
                return MathIFallbacks.ReverseBitOrder(value);
            }
        }
    }
}
