#define DEBUG_MATHI_NON_USER_CODE

using System;
using System.Runtime.CompilerServices;
using System.Numerics;

using DSUtils = DivideSharp.Utils;

using System.Diagnostics;

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
        #region FloorStep
        /// <summary>
        /// Aligns the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static int FloorStep(int value, int step) => value - (value % step);

        /// <summary>
        /// Aligns the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static (int newLength, int remainder) FloorStepRem(int value, int step)
        {
            var m = value % step;
            return (value - m, m);
        }
        #endregion

        #region Rectify
        /// <summary>
        /// Rectifies the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to rectify.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static int Rectify(int value)
        {
            var h = value >> 31;
            return value & ~h;
        }
        #endregion

        #region nint Math

        /// <inheritdoc cref="Math.Min(long, long)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static nint Min(nint val1, nint val2)
        {
            var g = val1 > val2;
            nint y = Unsafe.As<bool, byte>(ref g);
            y = -y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        #endregion

        #region Fast Math
        /// <inheritdoc cref="Math.Min(int, int)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static int Min(int val1, int val2)
        {
            var g = val1 > val2;
            int y = Unsafe.As<bool, byte>(ref g);
            y = -y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        /// <inheritdoc cref="Math.Min(uint, uint)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static uint Min(uint val1, uint val2)
        {
            var g = val1 > val2;
            uint y = Unsafe.As<bool, byte>(ref g);
            y = (uint)-(int)y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }
        #endregion

        #region Fast Bit Operations
        #region AndNot

        /// <summary>
        /// Performs a bitwise <c>and</c> operation on two specified <see cref="int"/> values after negating <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The value to be negated.</param>
        /// <param name="b">The value to be performed bitwise <c>and</c> operation with negated <paramref name="a"/>.</param>
        /// <returns>The and product of <paramref name="b"/> and negated <paramref name="a"/>.</returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int AndNot(int a, int b) => (int)AndNot((uint)a, (uint)b);

        /// <summary>
        /// Performs a bitwise <c>and</c> operation on two specified <see cref="uint"/> values after negating <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The value to be negated.</param>
        /// <param name="b">The value to be performed bitwise <c>and</c> operation with negated <paramref name="a"/>.</param>
        /// <returns>The and product of <paramref name="b"/> and negated <paramref name="a"/>.</returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint AndNot(uint a, uint b)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Bmi1.IsSupported)
                {
                    return Bmi1.AndNot(a, b);
                }
#endif
                return ~a & b;
            }
        }

        /// <summary>
        /// Performs a bitwise <c>and</c> operation on two specified <see cref="long"/> values after negating <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The value to be negated.</param>
        /// <param name="b">The value to be performed bitwise <c>and</c> operation with negated <paramref name="a"/>.</param>
        /// <returns>The and product of <paramref name="b"/> and negated <paramref name="a"/>.</returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static long AndNot(long a, long b) => (long)AndNot((ulong)a, (ulong)b);

        /// <summary>
        /// Performs a bitwise <c>and</c> operation on two specified <see cref="ulong"/> values after negating <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The value to be negated.</param>
        /// <param name="b">The value to be performed bitwise <c>and</c> operation with negated <paramref name="a"/>.</param>
        /// <returns>The and product of <paramref name="b"/> and negated <paramref name="a"/>.</returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong AndNot(ulong a, ulong b)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Bmi1.X64.IsSupported)
                {
                    return Bmi1.X64.AndNot(a, b);
                }
#endif
                return ~a & b;
            }
        }

        /// <summary>
        /// Performs a bitwise <c>and</c> operation on two specified <see cref="IntPtr"/> values after negating <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The value to be negated.</param>
        /// <param name="b">The value to be performed bitwise <c>and</c> operation with negated <paramref name="a"/>.</param>
        /// <returns>The and product of <paramref name="b"/> and negated <paramref name="a"/>.</returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static nint AndNot(nint a, nint b) => (nint)AndNot((nuint)a, (nuint)b);

        /// <summary>
        /// Performs a bitwise <c>and</c> operation on two specified <see cref="UIntPtr"/> values after negating <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The value to be negated.</param>
        /// <param name="b">The value to be performed bitwise <c>and</c> operation with negated <paramref name="a"/>.</param>
        /// <returns>The and product of <paramref name="b"/> and negated <paramref name="a"/>.</returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static nuint AndNot(nuint a, nuint b)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                unsafe
                {
                    if (Bmi1.X64.IsSupported && sizeof(nuint) == sizeof(ulong))
                    {
                        return (nuint)Bmi1.X64.AndNot(a, b);
                    }
                    if (Bmi1.IsSupported && sizeof(nuint) == sizeof(uint))
                    {
                        return Bmi1.AndNot((uint)a, (uint)b);
                    }
                }
#endif
                return ~a & b;
            }
        }
        #endregion
        #endregion

        #region Abs

        /// <summary>
        /// Returns the absolute value of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static ulong Abs(long value) => DSUtils.Abs(value);

        /// <summary>
        /// Returns the absolute value of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static uint Abs(int value) => DSUtils.Abs(value);

        #endregion Abs

        #region BigMul Polyfill

        /// <summary>
        /// Multiplies the specified <paramref name="x"/> and <paramref name="y"/> and returns the high part of whole 128bit result.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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

        #region ModularMultiplicativeInverse
        /// <summary>
        /// Calculates and returns Modular Multiplicative Inverse of <paramref name="a"/>.
        /// <paramref name="a"/> * x % <paramref name="n"/> = 1.
        /// </summary>
        /// <param name="a">The multiplier.</param>
        /// <param name="n">The modulus.</param>
        /// <returns>The Modular Multiplicative Inverse of <paramref name="a"/>.</returns>
        public static int ModularMultiplicativeInverse(int a, int n)
        {
            if (a == 1) return 1;
            var t = 0;
            var nt = 1;
            var r = n;
            var nr = a;
            while (nr != 0)
            {
                var q = r / nr;
                var ot = t;
                var or = r;
                t = nt;
                r = nr;
                nt = ot - q * nt;
                nr = or - q * nr;
            }
            if (r > 1) return int.MinValue;
            if (t < 0) t += n;
            return t;
        }
        #endregion

        #region ReadResult functions

        /// <summary>
        /// Determines the maximum of the parameters.
        /// </summary>
        /// <param name="a">The value a.</param>
        /// <param name="b">The value b.</param>
        /// <returns></returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static ReadResult Max(ReadResult a, ReadResult b) => a > b ? a : b;

        #endregion ReadResult functions

        #region TrailingZeroCount

        /// <summary>
        /// Counts the consecutive zero bits on the right.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static int PopCount(uint value)
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static ulong ExtractHighestSetBit(ulong value)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return value == 0 ? 0 : 0x8000_0000_0000_0000ul >> BitOperations.LeadingZeroCount(value);
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
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
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static uint ExtractBitField(uint value, byte start, byte length)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Bmi2.IsSupported)
                {
                    value >>= start;
                    return Bmi2.ZeroHighBits(value, length);
                }
                if (Bmi1.IsSupported)
                {
                    return Bmi1.BitFieldExtract(value, start, length);
                }
#endif
                return MathIFallbacks.ExtractBitField(value, start, length);
            }
        }

        /// <summary>
        /// Extracts the bit field inside <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="start">The start from LSB.</param>
        /// <param name="length">The length in bits.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static ulong ExtractBitField(ulong value, byte start, byte length)
        {
#if NETCOREAPP3_1_OR_GREATER

            if (Bmi2.X64.IsSupported)
            {
                value >>= start;
                return Bmi2.X64.ZeroHighBits(value, length);
            }
            if (Bmi1.X64.IsSupported)
            {
                return Bmi1.X64.BitFieldExtract(value, start, length);
            }
#endif
            return MathIFallbacks.ExtractBitField(value, start, length);
        }

        #endregion ExtractBitField

        #region ZeroHighBits

        /// <summary>
        /// Sets the bits of <paramref name="value"/> higher than specified <paramref name="index"/> to 0.
        /// </summary>
        /// <param name="index">The index counting from LSB.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ZeroHighBits(int index, uint value)
        {
#if NET5_0_OR_GREATER
            if (ArmBase.IsSupported)
            {
                //LLVM said that this one might be better in Armv8.
                index = -index;
                var w9 = uint.MaxValue;
                w9 >>= index;
                return w9 & value;
            }
#endif
#if NETCOREAPP3_1_OR_GREATER
            if (Bmi2.IsSupported)
            {
                return Bmi2.ZeroHighBits(value, (uint)index);
            }
            if (Bmi1.IsSupported)
            {
                return Bmi1.BitFieldExtract(value, (ushort)(index << 8));
            }
#endif
            index = -index;
            value <<= index;
            return value >> index;
        }

        /// <summary>
        /// Sets the bits of <paramref name="value"/> higher than specified <paramref name="index"/> to 0.
        /// </summary>
        /// <param name="index">The index counting from LSB.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ZeroHighBits(int index, ulong value)
        {
#if NET5_0_OR_GREATER
            if (ArmBase.IsSupported)
            {
                //LLVM said that this one might be better in Armv8.
                index = -index;
                var x9 = ulong.MaxValue;
                x9 >>= index;
                return x9 & value;
            }
#endif
#if NETCOREAPP3_1_OR_GREATER
            if (Bmi2.X64.IsSupported)
            {
                return Bmi2.X64.ZeroHighBits(value, (uint)index);
            }
            if (Bmi1.X64.IsSupported)
            {
                return Bmi1.X64.BitFieldExtract(value, (ushort)(index << 8));
            }
#endif
            index = -index;
            value <<= index;
            return value >> index;
        }

        /// <summary>
        /// Sets the bits of <paramref name="value"/> higher than specified <paramref name="index"/> counted from MSB, to 0.
        /// </summary>
        /// <param name="index">The index counting from MSB.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ZeroHighBitsFromHigh(int index, uint value)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Bmi2.IsSupported)
            {
                return Bmi2.ZeroHighBits(value, (uint)(32 - index));
            }
#endif
            value <<= index;
            return value >> index;
        }

        /// <summary>
        /// Sets the bits of <paramref name="value"/> higher than specified <paramref name="index"/> counted from MSB, to 0.
        /// </summary>
        /// <param name="index">The index counting from MSB.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ZeroHighBitsFromHigh(int index, ulong value)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Bmi2.X64.IsSupported)
            {
                return Bmi2.X64.ZeroHighBits(value, (uint)(64 - index));
            }
#endif
            value <<= index;
            return value >> index;
        }

        #endregion ZeroHighBits

        #region ZeroIfFalse

        /// <summary>
        /// Returns zero when <paramref name="condition"/> is false, otherwise <paramref name="value"/>.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <param name="value">The value to return if <paramref name="condition"/> were true.</param>
        /// <returns><paramref name="value"/> if <paramref name="condition"/> is true, otherwise 0.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ZeroIfFalse(bool condition, uint value)
        {
            var c = condition;
            var q = (uint)-Unsafe.As<bool, byte>(ref c);
            return q & value;
        }

        /// <summary>
        /// Returns zero when <paramref name="condition"/> is false, otherwise <paramref name="value"/>.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <param name="value">The value to return if <paramref name="condition"/> were true.</param>
        /// <returns><paramref name="value"/> if <paramref name="condition"/> is true, otherwise 0.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ZeroIfFalse(bool condition, ulong value)
        {
            var c = condition;
            var q = (ulong)-(long)Unsafe.As<bool, byte>(ref c);
            return q & value;
        }

        #endregion

        #region IsPowerOfTwo

        /// <summary>
        /// Determines whether the specified <paramref name="i"/> is power of two.
        /// </summary>
        /// <param name="i">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is power of two; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(uint i) => i != 0 && (i & (i - 1)) == 0;

        /// <summary>
        /// Determines whether the specified <paramref name="i"/> is power of two.
        /// </summary>
        /// <param name="i">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is power of two; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int i) => i != 0 && (i & (i - 1)) == 0;

        #endregion IsPowerOfTwo
    }
}
