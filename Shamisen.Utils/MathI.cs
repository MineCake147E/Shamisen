#define DEBUG_MATHI_NON_USER_CODE

using System;
using System.Runtime.CompilerServices;
using System.Numerics;

using System.Diagnostics;

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using DivideSharp;

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
        public static int FloorStep(int value, int step) => value - value % step;

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
        /// Returns <paramref name="value"/> if <paramref name="value"/> is positive, 0 otherwise.
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

        /// <summary>
        /// Returns <paramref name="value"/> if <paramref name="value"/> is positive, 0 otherwise.
        /// </summary>
        /// <param name="value">The value to rectify.</param>
        /// <returns>The rectified value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static long Rectify(long value)
        {
            var h = value >> 63;
            return value & ~h;
        }

        /// <summary>
        /// Returns <paramref name="value"/> if <paramref name="value"/> is positive, 0 otherwise.
        /// </summary>
        /// <param name="value">The value to rectify.</param>
        /// <returns>The rectified value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static nint Rectify(nint value)
        {
            var h = value >> (8 * Unsafe.SizeOf<nint>() - 1);
            return value & ~h;
        }
        #endregion

        #region Integer Min

        /// <summary>
        /// Determines the maximum of the parameters.
        /// </summary>
        /// <param name="a">The value a.</param>
        /// <param name="b">The value b.</param>
        /// <returns></returns>
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static ReadResult Min(ReadResult a, ReadResult b) => a < b ? a : b;

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

        /// <inheritdoc cref="Math.Min(long, long)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE
        [DebuggerStepThrough]
#endif
        public static nuint Min(nuint val1, nuint val2)
        {
            var g = val1 > val2;
            nuint y = Unsafe.As<bool, byte>(ref g);
            y = (nuint)(-(nint)y);
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

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
        /// <inheritdoc cref="Math.Min(long, long)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static long Min(long val1, long val2)
        {
            var g = val1 > val2;
            long y = Unsafe.As<bool, byte>(ref g);
            y = -y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        /// <inheritdoc cref="Math.Min(ulong, ulong)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static ulong Min(ulong val1, ulong val2)
        {
            var g = val1 > val2;
            ulong y = Unsafe.As<bool, byte>(ref g);
            y = (ulong)-(long)y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        /// <inheritdoc cref="Math.Min(short, short)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static short Min(short val1, short val2) => (short)Min((uint)val1, (uint)val2);

        /// <inheritdoc cref="Math.Min(ushort, ushort)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static ushort Min(ushort val1, ushort val2) => (ushort)Min((uint)val1, val2);
        /// <inheritdoc cref="Math.Min(sbyte, sbyte)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static sbyte Min(sbyte val1, sbyte val2) => (sbyte)Min((uint)val1, (uint)val2);

        /// <inheritdoc cref="Math.Min(byte, byte)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static byte Min(byte val1, byte val2) => (byte)Min((uint)val1, val2);

        #endregion

        #region Integer Max

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

        /// <inheritdoc cref="Math.Max(int, int)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static int Max(int val1, int val2)
        {
            var g = val1 < val2;
            int y = Unsafe.As<bool, byte>(ref g);
            y = -y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        /// <inheritdoc cref="Math.Max(uint, uint)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static uint Max(uint val1, uint val2)
        {
            var g = val1 < val2;
            uint y = Unsafe.As<bool, byte>(ref g);
            y = (uint)-(int)y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        /// <inheritdoc cref="Math.Max(long, long)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static long Max(long val1, long val2)
        {
            var g = val1 < val2;
            long y = Unsafe.As<bool, byte>(ref g);
            y = -y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        /// <inheritdoc cref="Math.Max(ulong, ulong)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static ulong Max(ulong val1, ulong val2)
        {
            var g = val1 < val2;
            ulong y = Unsafe.As<bool, byte>(ref g);
            y = (ulong)-(long)y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        /// <inheritdoc cref="Math.Max(long, long)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static nint Max(nint val1, nint val2)
        {
            var g = val1 < val2;
            nint y = Unsafe.As<bool, byte>(ref g);
            y = -y;
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        /// <inheritdoc cref="Math.Max(ulong, ulong)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static nuint Max(nuint val1, nuint val2)
        {
            var g = val1 < val2;
            nuint y = Unsafe.As<bool, byte>(ref g);
            y = (nuint)(-(nint)y);
            var r = y & val2;
            var q = AndNot(y, val1);
            return r | q;
        }

        /// <inheritdoc cref="Math.Max(short, short)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static short Max(short val1, short val2) => (short)Max((uint)val1, (uint)val2);

        /// <inheritdoc cref="Math.Max(ushort, ushort)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static ushort Max(ushort val1, ushort val2) => (ushort)Max((uint)val1, val2);
        /// <inheritdoc cref="Math.Max(sbyte, sbyte)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static sbyte Max(sbyte val1, sbyte val2) => (sbyte)Max((uint)val1, (uint)val2);

        /// <inheritdoc cref="Math.Max(byte, byte)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        public static byte Max(byte val1, byte val2) => (byte)Max((uint)val1, val2);

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
        public static int AndNot(int a, int b) => ~a & b;

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
        public static uint AndNot(uint a, uint b) => ~a & b;

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
        public static long AndNot(long a, long b) => ~a & b;

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
        public static ulong AndNot(ulong a, ulong b) => ~a & b;

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
        public static nint AndNot(nint a, nint b) => ~a & b;

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
        public static nuint AndNot(nuint a, nuint b) => ~a & b;
        #endregion

        #region GetShortestBitLength

        /// <inheritdoc cref="IBinaryInteger{TSelf}.GetShortestBitLength"/>
        public static int GetShortestBitLength(int value)
        {
            var y = value >> 31;
            value ^= y;
            value = -BitOperations.LeadingZeroCount((uint)value);
            return y + value + 32;
        }

        /// <inheritdoc cref="IBinaryInteger{TSelf}.GetShortestBitLength"/>
        public static int GetShortestBitLength(long value)
        {
            var y = value >> 63;
            value ^= y;
            var lzcnt = -BitOperations.LeadingZeroCount((ulong)value);
            return (int)(y + lzcnt + 64);
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
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong Abs(long value)
        {
            var q = value >> 63;
            return (ulong)((value + q) ^ q);
        }

        /// <summary>
        /// Returns the absolute value of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint Abs(int value)
        {
            var q = value >> 31;
            return (uint)((value + q) ^ q);
        }

        /// <summary>
        /// Returns the absolute value of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ushort Abs(short value)
        {
            var q = value >> 15;
            return (ushort)((value + q) ^ q);
        }

        /// <summary>
        /// Returns the absolute value of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
#if DEBUG_MATHI_NON_USER_CODE

        [DebuggerStepThrough]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static byte Abs(sbyte value)
        {
            var q = value >> 7;
            return (byte)((value + q) ^ q);
        }
        #endregion Abs

        #region CopySign

        #endregion

        #region Floating-point Logical
        /// <summary>
        /// Calculates the logical bitwise XOR of <paramref name="left"/> and <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The logical bitwise XOR of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Xor(float left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Xor(Vector64.CreateScalarUnsafe(left), Vector64.CreateScalarUnsafe(right)).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Xor(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
                if (Sse.IsSupported)
                {
                    return Sse.Xor(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right)).GetElement(0);
                }
#endif
                return BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(left) ^ BitConverter.SingleToUInt32Bits(right));
            }
        }

        /// <summary>
        /// Calculates the logical bitwise XOR of <paramref name="left"/> and <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The logical bitwise XOR of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double Xor(double left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Xor(Vector64.CreateScalar(left), Vector64.CreateScalar(right)).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Xor(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
                if (Sse.IsSupported)
                {
                    return Sse.Xor(Vector128.CreateScalarUnsafe(left).AsSingle(), Vector128.CreateScalarUnsafe(right).AsSingle()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) ^ BitConverter.DoubleToInt64Bits(right));
            }
        }

        /// <summary>
        /// Calculates the logical bitwise AND of <paramref name="left"/> and <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The logical bitwise AND of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float And(float left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.And(Vector64.CreateScalarUnsafe(left), Vector64.CreateScalarUnsafe(right)).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.And(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
                if (Sse.IsSupported)
                {
                    return Sse.And(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right)).GetElement(0);
                }
#endif
                return BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(left) & BitConverter.SingleToUInt32Bits(right));
            }
        }

        /// <summary>
        /// Calculates the logical bitwise AND of <paramref name="left"/> and <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The logical bitwise AND of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double And(double left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.And(Vector64.CreateScalar(left), Vector64.CreateScalar(right)).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.And(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
                if (Sse.IsSupported)
                {
                    return Sse.And(Vector128.CreateScalarUnsafe(left).AsSingle(), Vector128.CreateScalarUnsafe(right).AsSingle()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) & BitConverter.DoubleToInt64Bits(right));
            }
        }

        /// <summary>
        /// Calculates the logical bitwise OR of <paramref name="left"/> and <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The logical bitwise OR of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Or(float left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Or(Vector64.CreateScalarUnsafe(left), Vector64.CreateScalarUnsafe(right)).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Or(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
                if (Sse.IsSupported)
                {
                    return Sse.Or(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right)).GetElement(0);
                }
#endif
                return BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(left) | BitConverter.SingleToUInt32Bits(right));
            }
        }

        /// <summary>
        /// Calculates the logical bitwise OR of <paramref name="left"/> and <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The logical bitwise OR of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double Or(double left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Or(Vector64.CreateScalar(left), Vector64.CreateScalar(right)).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Or(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
                if (Sse.IsSupported)
                {
                    return Sse.Or(Vector128.CreateScalarUnsafe(left).AsSingle(), Vector128.CreateScalarUnsafe(right).AsSingle()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) | BitConverter.DoubleToInt64Bits(right));
            }
        }

        /// <summary>
        /// Calculates the logical bitwise AND of negative <paramref name="left"/> and <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first value to be negated.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The logical bitwise AND of negative <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float AndNot(float left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = AdvSimd.Not(Vector64.CreateScalarUnsafe(left));
                    return AdvSimd.And(s0, Vector64.CreateScalarUnsafe(right)).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.AndNot(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
                if (Sse.IsSupported)
                {
                    return Sse.AndNot(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right)).GetElement(0);
                }
#endif
                return BitConverter.UInt32BitsToSingle(~BitConverter.SingleToUInt32Bits(left) & BitConverter.SingleToUInt32Bits(right));
            }
        }

        /// <summary>
        /// Calculates the logical bitwise AND of negative <paramref name="left"/> and <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first value to be negated.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The logical bitwise AND of negative <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double AndNot(double left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = AdvSimd.Not(Vector64.CreateScalar(left));
                    return AdvSimd.And(s0, Vector64.CreateScalar(right)).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.AndNot(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
                if (Sse.IsSupported)
                {
                    return Sse.AndNot(Vector128.CreateScalarUnsafe(left).AsSingle(), Vector128.CreateScalarUnsafe(right).AsSingle()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(~BitConverter.DoubleToInt64Bits(left) & BitConverter.DoubleToInt64Bits(right));
            }
        }
        #endregion

        #region Vector-Friendly Integer Arithmetics
        #region Add
        /// <summary>
        /// Adds the internal representation of <paramref name="left"/> and <paramref name="right"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second integer value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float AddInteger(float left, int right)
        {
            unchecked
            {
                if (Vector64.IsHardwareAccelerated)
                {
                    return Vector64.Add(Vector64.CreateScalarUnsafe(left).AsInt32(), Vector64.CreateScalarUnsafe(right)).AsSingle().GetElement(0);
                }
                if (Vector128.IsHardwareAccelerated)
                {
                    return Vector128.Add(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right)).AsSingle().GetElement(0);
                }
                if (Vector256.IsHardwareAccelerated)
                {
                    return Vector256.Add(Vector256.CreateScalarUnsafe(left).AsInt32(), Vector256.CreateScalarUnsafe(right)).AsSingle().GetElement(0);
                }
                return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(left) + right);
            }
        }

        /// <summary>
        /// Adds the internal representation of <paramref name="right"/> and <paramref name="left"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first integer value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float AddInteger(int left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Add(Vector64.CreateScalarUnsafe(left), Vector64.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Add(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
#endif
                return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(right) + left);
            }
        }

        /// <summary>
        /// Adds the internal representation of <paramref name="left"/> and <paramref name="right"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second integer value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float AddInteger(float left, uint right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Add(Vector64.CreateScalarUnsafe(left).AsUInt32(), Vector64.CreateScalarUnsafe(right)).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Add(Vector128.CreateScalarUnsafe(left).AsUInt32(), Vector128.CreateScalarUnsafe(right)).AsSingle().GetElement(0);
                }
#endif
                return BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(left) + right);
            }
        }

        /// <summary>
        /// Adds the internal representation of <paramref name="right"/> and <paramref name="left"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first integer value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float AddInteger(uint left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Add(Vector64.CreateScalarUnsafe(left), Vector64.CreateScalarUnsafe(right).AsUInt32()).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Add(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right).AsUInt32()).AsSingle().GetElement(0);
                }
#endif
                return BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(right) + left);
            }
        }

        /// <summary>
        /// Adds the internal representation of <paramref name="left"/> and the internal representation of <paramref name="right"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float AddInteger(float left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Add(Vector64.CreateScalarUnsafe(left).AsInt32(), Vector64.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Add(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
#endif
                return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(left) + BitConverter.SingleToInt32Bits(right));
            }
        }

        /// <summary>
        /// Adds the internal representation of <paramref name="left"/> and <paramref name="right"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second integer value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double AddInteger(double left, long right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Add(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right)).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Add(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) + right);
            }
        }

        /// <summary>
        /// Adds the internal representation of <paramref name="right"/> and <paramref name="left"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first integer value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double AddInteger(long left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Add(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Add(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(right) + left);
            }
        }

        /// <summary>
        /// Adds the internal representation of <paramref name="left"/> and <paramref name="right"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second integer value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double AddInteger(double left, ulong right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Add(Vector128.CreateScalarUnsafe(left).AsUInt64(), Vector128.CreateScalarUnsafe(right)).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Add(Vector128.CreateScalarUnsafe(left).AsUInt64(), Vector128.CreateScalarUnsafe(right)).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.UInt64BitsToDouble(BitConverter.DoubleToUInt64Bits(left) + right);
            }
        }

        /// <summary>
        /// Adds the internal representation of <paramref name="right"/> and <paramref name="left"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first integer value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double AddInteger(ulong left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Add(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right).AsUInt64()).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Add(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right).AsUInt64()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.UInt64BitsToDouble(BitConverter.DoubleToUInt64Bits(right) + left);
            }
        }

        /// <summary>
        /// Adds the internal representation of <paramref name="left"/> and the internal representation of <paramref name="right"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer addition of <paramref name="left"/> and <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double AddInteger(double left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Add(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Add(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) + BitConverter.DoubleToInt64Bits(right));
            }
        }
        #endregion

        #region Subtract

        /// <summary>
        /// Subtracts the internal representation of <paramref name="left"/> by <paramref name="right"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second integer value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float SubtractInteger(float left, int right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector64.CreateScalarUnsafe(left).AsInt32(), Vector64.CreateScalarUnsafe(right)).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
#endif
                return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(left) - right);
            }
        }

        /// <summary>
        /// Subtracts the internal representation of <paramref name="right"/> by <paramref name="left"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first integer value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float SubtractInteger(int left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector64.CreateScalarUnsafe(left), Vector64.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
#endif
                return BitConverter.Int32BitsToSingle(left - BitConverter.SingleToInt32Bits(right));
            }
        }

        /// <summary>
        /// Subtracts the internal representation of <paramref name="left"/> by <paramref name="right"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second integer value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float SubtractInteger(float left, uint right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector64.CreateScalarUnsafe(left).AsUInt32(), Vector64.CreateScalarUnsafe(right)).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left).AsUInt32(), Vector128.CreateScalarUnsafe(right)).AsSingle().GetElement(0);
                }
#endif
                return BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(left) - right);
            }
        }

        /// <summary>
        /// Subtracts the internal representation of <paramref name="right"/> by <paramref name="left"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first integer value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float SubtractInteger(uint left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector64.CreateScalarUnsafe(left), Vector64.CreateScalarUnsafe(right).AsUInt32()).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right).AsUInt32()).AsSingle().GetElement(0);
                }
#endif
                return BitConverter.UInt32BitsToSingle(left - BitConverter.SingleToUInt32Bits(right));
            }
        }

        /// <summary>
        /// Subtracts the internal representation of <paramref name="left"/> by the internal representation of <paramref name="right"/> as 32-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by the internal representation of <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float SubtractInteger(float left, float right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector64.CreateScalarUnsafe(left).AsInt32(), Vector64.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left).AsInt32(), Vector128.CreateScalarUnsafe(right).AsInt32()).AsSingle().GetElement(0);
                }
#endif
                return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(left) - BitConverter.SingleToInt32Bits(right));
            }
        }

        /// <summary>
        /// Subtracts the internal representation of <paramref name="left"/> by <paramref name="right"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second integer value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double SubtractInteger(double left, long right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right)).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) - right);
            }
        }

        /// <summary>
        /// Subtracts the internal representation of <paramref name="right"/> by <paramref name="left"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first integer value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double SubtractInteger(long left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(left - BitConverter.DoubleToInt64Bits(right));
            }
        }

        /// <summary>
        /// Subtracts the internal representation of <paramref name="left"/> by <paramref name="right"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second integer value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double SubtractInteger(double left, ulong right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector128.CreateScalarUnsafe(left).AsUInt64(), Vector128.CreateScalarUnsafe(right)).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left).AsUInt64(), Vector128.CreateScalarUnsafe(right)).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.UInt64BitsToDouble(BitConverter.DoubleToUInt64Bits(left) - right);
            }
        }

        /// <summary>
        /// Subtracts the internal representation of <paramref name="right"/> by <paramref name="left"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first integer value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double SubtractInteger(ulong left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right).AsUInt64()).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left), Vector128.CreateScalarUnsafe(right).AsUInt64()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.UInt64BitsToDouble(left - BitConverter.DoubleToUInt64Bits(right));
            }
        }

        /// <summary>
        /// Subtracts the internal representation of <paramref name="left"/> by the internal representation of <paramref name="right"/> as 64-bit integer.
        /// </summary>
        /// <param name="left">The first floating-point value.</param>
        /// <param name="right">The second floating-point value.</param>
        /// <returns>The integer subtraction of <paramref name="left"/> by the internal representation of <paramref name="right"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double SubtractInteger(double left, double right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.Subtract(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return Sse2.Subtract(Vector128.CreateScalarUnsafe(left).AsInt64(), Vector128.CreateScalarUnsafe(right).AsInt64()).AsDouble().GetElement(0);
                }
#endif
                return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(left) - BitConverter.DoubleToInt64Bits(right));
            }
        }
        #endregion

        #region ShiftRightArithmetic

        /// <summary>
        /// Shifts the internal representation of <paramref name="value"/> right by <paramref name="count"/> bits with copying the sign bits.
        /// </summary>
        /// <param name="value">The value to be shifted.</param>
        /// <param name="count">The constant amount of bits to perform arithmetic right shift of <paramref name="value"/>.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float ShiftRightArithmetic(float value, byte count)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.ShiftRightArithmetic(Vector64.CreateScalarUnsafe(value).AsInt32(), count).AsSingle().GetElement(0);
            }
#endif
#if NETCOREAPP3_1_OR_GREATER
            if (Sse2.IsSupported)
            {
                return Sse2.ShiftRightArithmetic(Vector128.CreateScalarUnsafe(value).AsInt32(), count).AsSingle().GetElement(0);
            }
#endif
            return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(value) >> count);
        }

        /// <summary>
        /// Shifts the internal representation of <paramref name="value"/> right by <paramref name="count"/> bits with copying the sign bits.
        /// </summary>
        /// <param name="value">The value to be shifted.</param>
        /// <param name="count">The constant amount of bits to perform arithmetic right shift of <paramref name="value"/>.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double ShiftRightArithmetic(double value, byte count)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.ShiftRightArithmetic(Vector128.CreateScalarUnsafe(value).AsInt32(), count).AsSingle().GetElement(0);
            }
#endif
#if NETCOREAPP3_1_OR_GREATER
            if (Sse2.IsSupported)
            {
                return Sse2.ShiftRightArithmetic(Vector128.CreateScalarUnsafe(value).AsInt32(), count).AsSingle().GetElement(0);
            }
#endif
            return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(value) >> count);
        }
        #endregion
        #endregion

        #region BigMul Polyfill

        /// <summary>
        /// Multiplies the specified <paramref name="x"/> and <paramref name="y"/> and returns the whole 128bit result.
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
        /// Multiplies the specified <paramref name="x"/> and <paramref name="y"/> and returns the whole 128bit result.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
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
#if NETCOREAPP3_1_OR_GREATER
                if (Bmi2.X64.IsSupported)
                {
                    unsafe
                    {
                        ulong low = 0;
                        var high = Bmi2.X64.MultiplyNoFlags(x, y, &low);
                        return (low, high);
                    }
                }
#endif
                return MathIFallbacks.BigMul(x, y);
#endif
            }
        }

        #endregion BigMul Polyfill

        #region ModularMultiplicativeInverse
        /// <summary>
        /// Calculates and returns Modular Multiplicative Inverse of <paramref name="a"/>.<br/>
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

        #region Reciprocal

        /// <summary>
        /// Calculates the (2^64) / <paramref name="value"/> and its remainder.
        /// </summary>
        /// <param name="value">The value to divide 2^64 by.</param>
        /// <param name="remainder">The remainder (2^64) % <paramref name="value"/>.</param>
        /// <returns>The quotient (2^64) / <paramref name="value"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ReciprocalUInt64(ulong value, out ulong remainder)
        {
            var (q, r) = Math.DivRem(~0ul, value);
            var k = ++r < value;
            var kk = Unsafe.As<bool, byte>(ref k) - 1ul;
            q -= kk;
            r = ~kk & r;
            remainder = r;
            return q;
        }

        /// <summary>
        /// Calculates the (2^64) / <paramref name="value"/> and its remainder.
        /// </summary>
        /// <param name="value">The value to divide 2^64 by.</param>
        /// <param name="remainder">The remainder (2^64) % <paramref name="value"/>.</param>
        /// <returns>The quotient (2^64) / <paramref name="value"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ReciprocalUInt64(UInt64Divisor value, out ulong remainder)
        {
            var d = value.Divisor;
            var r = value.DivRem(~0ul, out var q);
            var k = ++r < d;
            var kk = Unsafe.As<bool, byte>(ref k) - 1ul;
            q -= kk;
            r = ~kk & r;
            remainder = r;
            return q;
        }

        /// <summary>
        /// Calculates the (2^32) / <paramref name="value"/> and its remainder.
        /// </summary>
        /// <param name="value">The value to divide 2^32 by.</param>
        /// <param name="remainder">The remainder (2^32) % <paramref name="value"/>.</param>
        /// <returns>The quotient (2^32) / <paramref name="value"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ReciprocalUInt32(uint value, out uint remainder)
        {
            var (q, r) = Math.DivRem(uint.MaxValue, value);
            var k = ++r < value;
            var kk = Unsafe.As<bool, byte>(ref k) - 1u;
            q -= kk;
            r = ~kk & r;
            remainder = r;
            return q;
        }

        /// <summary>
        /// Calculates the (<see cref="nuint.MaxValue"/> + 1) / <paramref name="value"/> and its remainder.
        /// </summary>
        /// <param name="value">The value to divide 2^32 by.</param>
        /// <param name="remainder">The remainder (<see cref="nuint.MaxValue"/> + 1) % <paramref name="value"/>.</param>
        /// <returns>The quotient (<see cref="nuint.MaxValue"/> + 1) / <paramref name="value"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static nuint ReciprocalUIntPtr(nuint value, out nuint remainder)
        {
            var (q, r) = Math.DivRem(nuint.MaxValue, value);
            var k = ++r < value;
            var kk = Unsafe.As<bool, byte>(ref k) - (nuint)1;
            q -= kk;
            r = ~kk & r;
            remainder = r;
            return q;
        }

        #endregion

        #region BigDiv

        /// <summary>
        /// Divides a 128-bit number consists of two 64-bit number <paramref name="hi"/> and <paramref name="lo"/> by a constant <paramref name="divisor"/>.
        /// </summary>
        /// <param name="hi">The higher part of 128-bit numerator.</param>
        /// <param name="lo">The lower part of 128-bit numerator.</param>
        /// <param name="divisor">The divisor. Assumed to be a constant number.</param>
        /// <returns>The lower 64-bit quotient.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong BigDivConstant(ulong hi, ulong lo, ulong divisor)
        {
            var hiq = ReciprocalUInt64(divisor, out var hir);
            lo += hir * hi;
            hi = hiq * hi;
            hi += lo / divisor;
            return hi;
        }
        #endregion

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
        /// Returns zero when <paramref name="condition"/> is false, otherwise 1.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <returns>1 if <paramref name="condition"/> is true, otherwise 0.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int ToInt32(bool condition) => Unsafe.As<bool, byte>(ref condition);

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
        public static int ZeroIfFalse(bool condition, int value) => (int)ZeroIfFalse(condition, (uint)value);

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

        /// <summary>
        /// Returns zero when <paramref name="condition"/> is false, otherwise <paramref name="value"/>.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <param name="value">The value to return if <paramref name="condition"/> were true.</param>
        /// <returns><paramref name="value"/> if <paramref name="condition"/> is true, otherwise 0.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static long ZeroIfFalse(bool condition, long value) => (long)ZeroIfFalse(condition, (ulong)value);

        /// <summary>
        /// Returns zero when <paramref name="condition"/> is false, otherwise <paramref name="value"/>.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <param name="value">The value to return if <paramref name="condition"/> were true.</param>
        /// <returns><paramref name="value"/> if <paramref name="condition"/> is true, otherwise 0.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static nint ZeroIfFalse(bool condition, nint value)
        {
            var c = condition;
            var q = -(nint)Unsafe.As<bool, byte>(ref c);
            return q & value;
        }

        /// <summary>
        /// Returns zero when <paramref name="condition"/> is false, otherwise <paramref name="value"/>.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <param name="value">The value to return if <paramref name="condition"/> were true.</param>
        /// <returns><paramref name="value"/> if <paramref name="condition"/> is true, otherwise 0.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static nuint ZeroIfFalse(bool condition, nuint value)
        {
            var c = condition;
            var q = (nuint)(-(nint)Unsafe.As<bool, byte>(ref c));
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

        #region SingleToInt32BitsTwosComplement

        /// <summary>
        /// Returns the internal representation of a specified floating-point number from signed absolute value representation converted to two's complement representation.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns></returns>
        public static int SingleToInt32BitsTwosComplement(float value)
        {
            var f = BitConverter.SingleToInt32Bits(value);
            var g = f >> 31;
            g = (int)((uint)g >> 1);
            f ^= g;
            return f;
        }
        #endregion
    }
}
