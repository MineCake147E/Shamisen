using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    public readonly partial struct Int24 : IBinaryInteger<Int24>, ISignedNumber<Int24>
    {
        /// <inheritdoc/>
        public static Int24 AdditiveIdentity => Zero;

        /// <inheritdoc/>
        public static Int24 MultiplicativeIdentity => One;

        /// <inheritdoc/>
        public static Int24 NegativeOne => (Int24)(-1);

        /// <inheritdoc/>
        public static Int24 One => (Int24)1;
        /// <inheritdoc/>
        public static int Radix => 2;
        /// <inheritdoc/>
        public static Int24 Zero => (Int24)0;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 Abs(Int24 value)
        {
            var j = value.head;
            var sign = (sbyte)j >> 31;
            var res = (int)value;
            return new(sign ^ (res + sign));
        }
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsCanonical(Int24 value) => true;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsComplexNumber(Int24 value) => false;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsEvenInteger(Int24 value) => ((int)value & 1) == 0;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsFinite(Int24 value) => true;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsImaginaryNumber(Int24 value) => false;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsInfinity(Int24 value) => false;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsInteger(Int24 value) => true;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsNaN(Int24 value) => false;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsNegativeInfinity(Int24 value) => false;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsNormal(Int24 value) => value != 0;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsOddInteger(Int24 value) => ((int)value & 1) != 0;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsPositive(Int24 value) => (value.head & 0x80) == 0;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsPositiveInfinity(Int24 value) => false;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsPow2(Int24 value) => MathI.IsPowerOfTwo(value);
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsRealNumber(Int24 value) => true;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsSubnormal(Int24 value) => value == 0;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsZero(Int24 value) => value == 0;
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 Log2(Int24 value) => (Int24)MathI.LogBase2(checked((uint)(int)value));
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 MaxMagnitude(Int24 x, Int24 y) => (Int24)int.MaxMagnitude(x, y);
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 MaxMagnitudeNumber(Int24 x, Int24 y) => MaxMagnitude(x, y);
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 MinMagnitude(Int24 x, Int24 y) => (Int24)int.MinMagnitude(x, y);
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 MinMagnitudeNumber(Int24 x, Int24 y) => MinMagnitude(x, y);

        #region Operator Overloads
        #region Logical
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator ~(Int24 value) => new((ushort)~value.midtail, (byte)~value.head);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator ^(Int24 left, Int24 right) => new((ushort)(left.midtail ^ right.midtail), (byte)(left.head ^ right.head));

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator &(Int24 left, Int24 right) => new((ushort)(left.midtail & right.midtail), (byte)(left.head & right.head));

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator |(Int24 left, Int24 right) => new((ushort)(left.midtail | right.midtail), (byte)(left.head | right.head));

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator >>>(Int24 value, int shiftAmount) => (Int24)((int)value >>> shiftAmount);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator <<(Int24 value, int shiftAmount) => (Int24)((int)value << shiftAmount);

        #endregion

        #region Arithmetic
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator +(Int24 value) => value;

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator -(Int24 value) => new(~value + 1);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator ++(Int24 value) => (Int24)((int)value + 1);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator --(Int24 value) => (Int24)((int)value - 1);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator *(Int24 left, Int24 right) => (Int24)(left * (int)right);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator /(Int24 left, Int24 right) => (Int24)(left / (int)right);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator %(Int24 left, Int24 right) => (Int24)(left % (int)right);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator +(Int24 left, Int24 right) => (Int24)(left + (int)right);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator -(Int24 left, Int24 right) => (Int24)(left - (int)right);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator >>(Int24 value, int shiftAmount) => (Int24)((int)value >> shiftAmount);
        #endregion
        #endregion

        /// <inheritdoc/>
        public static Int24 Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => (Int24)int.Parse(s, style, provider);
        /// <inheritdoc/>
        public static Int24 Parse(string s, NumberStyles style, IFormatProvider? provider) => (Int24)int.Parse(s, style, provider);
        /// <inheritdoc/>
        public static Int24 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => (Int24)int.Parse(s, provider);
        /// <inheritdoc/>
        public static Int24 Parse(string s, IFormatProvider? provider) => (Int24)int.Parse(s, provider);
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 PopCount(Int24 value) => (Int24)MathI.PopCount((uint)(int)value);
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 TrailingZeroCount(Int24 value) => (Int24)MathI.TrailingZeroCount((uint)(int)value);

        /// <inheritdoc/>
        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Int24 result)
        {
            var success = int.TryParse(s, style, provider, out var res);
            result = (Int24)res;
            return success;
        }
        /// <inheritdoc/>
        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out Int24 result)
        {
            var success = int.TryParse(s, style, provider, out var res);
            result = (Int24)res;
            return success;
        }

        /// <inheritdoc/>
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Int24 result)
        {
            var success = int.TryParse(s, provider, out var res);
            result = (Int24)res;
            return success;
        }

        /// <inheritdoc/>
        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Int24 result)
        {
            var success = int.TryParse(s, provider, out var res);
            result = (Int24)res;
            return success;
        }

        /// <inheritdoc/>
        static bool IBinaryInteger<Int24>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Int24 value) => throw new NotImplementedException();
        /// <inheritdoc/>
        static bool IBinaryInteger<Int24>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Int24 value) => throw new NotImplementedException();

        /// <inheritdoc/>
        public int CompareTo(object? obj) => obj is Int24 value ? value.CompareTo(obj) : 0;

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public int GetByteCount() => 3;

        /// <inheritdoc/>
        public int GetShortestBitLength() => MathI.GetShortestBitLength(this);

        /// <inheritdoc/>
        public string ToString(string? format, IFormatProvider? formatProvider) => ((int)this).ToString(format, formatProvider);

        /// <inheritdoc/>
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => ((int)this).TryFormat(destination, out charsWritten, format, provider);

        /// <inheritdoc/>
        bool IBinaryInteger<Int24>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();

        /// <inheritdoc/>
        bool IBinaryInteger<Int24>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();

        /// <inheritdoc/>
        static bool INumberBase<Int24>.IsNegative(Int24 value) => value.IsNegative;
        static bool INumberBase<Int24>.TryConvertFromChecked<TOther>(TOther value, out Int24 result) => throw new NotImplementedException();
        static bool INumberBase<Int24>.TryConvertFromSaturating<TOther>(TOther value, out Int24 result) => throw new NotImplementedException();
        static bool INumberBase<Int24>.TryConvertFromTruncating<TOther>(TOther value, out Int24 result) => throw new NotImplementedException();
        static bool INumberBase<Int24>.TryConvertToChecked<TOther>(Int24 value, out TOther result) => throw new NotImplementedException();
        static bool INumberBase<Int24>.TryConvertToSaturating<TOther>(Int24 value, out TOther result) => throw new NotImplementedException();
        static bool INumberBase<Int24>.TryConvertToTruncating<TOther>(Int24 value, out TOther result) => throw new NotImplementedException();
    }
}
