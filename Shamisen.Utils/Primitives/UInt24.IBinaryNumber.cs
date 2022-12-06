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
    public readonly partial struct UInt24 : IBinaryInteger<UInt24>, IUnsignedNumber<UInt24>
    {
        /// <inheritdoc/>
        public static UInt24 One { get; }
        /// <inheritdoc/>
        public static int Radix { get; }
        /// <inheritdoc/>
        public static UInt24 Zero { get; }
        /// <inheritdoc/>
        public static UInt24 AdditiveIdentity { get; }
        /// <inheritdoc/>
        public static UInt24 MultiplicativeIdentity { get; }

        /// <inheritdoc/>
        public static UInt24 Abs(UInt24 value) => value;
        /// <inheritdoc/>
        public static bool IsCanonical(UInt24 value) => true;
        /// <inheritdoc/>
        public static bool IsComplexNumber(UInt24 value) => false;
        /// <inheritdoc/>
        public static bool IsEvenInteger(UInt24 value) => (value & 1) == 0;
        /// <inheritdoc/>
        public static bool IsFinite(UInt24 value) => true;
        /// <inheritdoc/>
        public static bool IsImaginaryNumber(UInt24 value) => false;
        /// <inheritdoc/>
        public static bool IsInfinity(UInt24 value) => false;
        /// <inheritdoc/>
        public static bool IsInteger(UInt24 value) => true;
        /// <inheritdoc/>
        public static bool IsNaN(UInt24 value) => false;
        /// <inheritdoc/>
        public static bool IsNegative(UInt24 value) => false;
        /// <inheritdoc/>
        public static bool IsNegativeInfinity(UInt24 value) => false;
        /// <inheritdoc/>
        public static bool IsNormal(UInt24 value) => value != 0;
        /// <inheritdoc/>
        public static bool IsOddInteger(UInt24 value) => (value & 1) > 0;
        /// <inheritdoc/>
        public static bool IsPositive(UInt24 value) => value > 0;
        /// <inheritdoc/>
        public static bool IsPositiveInfinity(UInt24 value) => false;
        /// <inheritdoc/>
        public static bool IsPow2(UInt24 value) => MathI.IsPowerOfTwo(value);
        /// <inheritdoc/>
        public static bool IsRealNumber(UInt24 value) => true;
        /// <inheritdoc/>
        public static bool IsSubnormal(UInt24 value) => value == 0;
        /// <inheritdoc/>
        public static bool IsZero(UInt24 value) => value == 0;
        /// <inheritdoc/>
        public static UInt24 Log2(UInt24 value) => (UInt24)(uint)MathI.LogBase2(value);
        /// <inheritdoc/>
        public static UInt24 MaxMagnitude(UInt24 x, UInt24 y) => (UInt24)MathI.Max(x, y);
        /// <inheritdoc/>
        public static UInt24 MaxMagnitudeNumber(UInt24 x, UInt24 y) => (UInt24)MathI.Max(x, y);
        /// <inheritdoc/>
        public static UInt24 MinMagnitude(UInt24 x, UInt24 y) => (UInt24)MathI.Min(x, y);
        /// <inheritdoc/>
        public static UInt24 MinMagnitudeNumber(UInt24 x, UInt24 y) => (UInt24)MathI.Min(x, y);
        /// <inheritdoc/>
        public static UInt24 Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => (UInt24)uint.Parse(s, style, provider);
        /// <inheritdoc/>
        public static UInt24 Parse(string s, NumberStyles style, IFormatProvider? provider) => (UInt24)uint.Parse(s, style, provider);
        /// <inheritdoc/>
        public static UInt24 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => (UInt24)uint.Parse(s, provider);
        /// <inheritdoc/>
        public static UInt24 Parse(string s, IFormatProvider? provider) => (UInt24)uint.Parse(s, provider);
        /// <inheritdoc/>
        public static UInt24 PopCount(UInt24 value) => (UInt24)(uint)MathI.PopCount(value);
        /// <inheritdoc/>
        public static UInt24 TrailingZeroCount(UInt24 value) => (UInt24)(uint)MathI.TrailingZeroCount(value);
        /// <inheritdoc/>
        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out UInt24 result) => throw new NotImplementedException();
        /// <inheritdoc/>
        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out UInt24 result) => throw new NotImplementedException();
        /// <inheritdoc/>
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out UInt24 result) => throw new NotImplementedException();
        /// <inheritdoc/>
        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out UInt24 result) => throw new NotImplementedException();
        /// <inheritdoc/>
        public static bool TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt24 value) => throw new NotImplementedException();
        /// <inheritdoc/>
        public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt24 value) => throw new NotImplementedException();

        /// <inheritdoc/>
        public int CompareTo(object? obj) => obj is UInt24 uv ? CompareTo(uv) : 0;
        /// <inheritdoc/>
        public int GetByteCount() => 3;
        /// <inheritdoc/>
        public int GetShortestBitLength() => MathI.LogBase2(this);
        /// <inheritdoc/>
        public string ToString(string? format, IFormatProvider? formatProvider) => ((uint)this).ToString(format, formatProvider);
        /// <inheritdoc/>
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => ((uint)this).TryFormat(destination, out charsWritten, format, provider);
        /// <inheritdoc/>
        public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
        /// <inheritdoc/>
        public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
        #region Operator Overloads
        #region Logical
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator ~(UInt24 value) => new((ushort)~value.midtail, (byte)~value.head);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator ^(UInt24 left, UInt24 right) => new((ushort)(left.midtail ^ right.midtail), (byte)(left.head ^ right.head));

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator &(UInt24 left, UInt24 right) => new((ushort)(left.midtail & right.midtail), (byte)(left.head & right.head));

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator |(UInt24 left, UInt24 right) => new((ushort)(left.midtail | right.midtail), (byte)(left.head | right.head));

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator >>>(UInt24 value, int shiftAmount) => (UInt24)((uint)value >>> shiftAmount);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator >>(UInt24 value, int shiftAmount) => (UInt24)((uint)value >> shiftAmount);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator <<(UInt24 value, int shiftAmount) => (UInt24)((uint)value << shiftAmount);
        #endregion

        #region Arithmetic

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator +(UInt24 value) => value;

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator -(UInt24 value) => new(~value + 1);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator ++(UInt24 value) => (UInt24)((uint)value + 1);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator --(UInt24 value) => (UInt24)((uint)value - 1);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator *(UInt24 left, UInt24 right) => (UInt24)(left * (uint)right);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator /(UInt24 left, UInt24 right) => (UInt24)(left / (uint)right);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator %(UInt24 left, UInt24 right) => (UInt24)(left % (uint)right);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator +(UInt24 left, UInt24 right) => (UInt24)(left + (uint)right);

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 operator -(UInt24 left, UInt24 right) => (UInt24)(left - (uint)right);
        #endregion
        #endregion

        static bool INumberBase<UInt24>.TryConvertFromChecked<TOther>(TOther value, out UInt24 result) => throw new NotImplementedException();
        static bool INumberBase<UInt24>.TryConvertFromSaturating<TOther>(TOther value, out UInt24 result) => throw new NotImplementedException();
        static bool INumberBase<UInt24>.TryConvertFromTruncating<TOther>(TOther value, out UInt24 result) => throw new NotImplementedException();
        static bool INumberBase<UInt24>.TryConvertToChecked<TOther>(UInt24 value, out TOther result) => throw new NotImplementedException();
        static bool INumberBase<UInt24>.TryConvertToSaturating<TOther>(UInt24 value, out TOther result) => throw new NotImplementedException();
        static bool INumberBase<UInt24>.TryConvertToTruncating<TOther>(UInt24 value, out TOther result) => throw new NotImplementedException();
    }
}
