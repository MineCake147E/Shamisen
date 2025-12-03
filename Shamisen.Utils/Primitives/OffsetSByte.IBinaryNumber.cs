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
    public readonly partial struct OffsetSByte : IBinaryInteger<OffsetSByte>, ISignedNumber<OffsetSByte>
    {
        /// <inheritdoc/>
        public static OffsetSByte AdditiveIdentity => Zero;

        /// <inheritdoc/>
        public static OffsetSByte MultiplicativeIdentity => One;

        /// <inheritdoc/>
        public static OffsetSByte NegativeOne => (OffsetSByte)(-1);

        /// <inheritdoc/>
        public static int Radix => 2;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte Abs(OffsetSByte value) => (OffsetSByte)sbyte.Abs((sbyte)value);
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsCanonical(OffsetSByte value) => true;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsComplexNumber(OffsetSByte value) => false;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsEvenInteger(OffsetSByte value) => ((int)value & 1) == 0;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsFinite(OffsetSByte value) => true;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsImaginaryNumber(OffsetSByte value) => false;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsInfinity(OffsetSByte value) => false;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsInteger(OffsetSByte value) => true;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsNaN(OffsetSByte value) => false;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsNegativeInfinity(OffsetSByte value) => false;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsNormal(OffsetSByte value) => value != (OffsetSByte)0;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsOddInteger(OffsetSByte value) => ((int)value & 1) != 0;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsPositive(OffsetSByte value) => (value.value & 0x80) == 1;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsPositiveInfinity(OffsetSByte value) => false;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsPow2(OffsetSByte value) => MathI.IsPowerOfTwo((sbyte)value);
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsRealNumber(OffsetSByte value) => true;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsSubnormal(OffsetSByte value) => value == Zero;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool IsZero(OffsetSByte value) => value == Zero;
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte Log2(OffsetSByte value) => (OffsetSByte)MathI.LogBase2(checked((uint)(int)value));
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte MaxMagnitude(OffsetSByte x, OffsetSByte y) => (OffsetSByte)int.MaxMagnitude((sbyte)x, (sbyte)y);
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte MaxMagnitudeNumber(OffsetSByte x, OffsetSByte y) => MaxMagnitude(x, y);
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte MinMagnitude(OffsetSByte x, OffsetSByte y) => (OffsetSByte)int.MinMagnitude((sbyte)x, (sbyte)y);
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte MinMagnitudeNumber(OffsetSByte x, OffsetSByte y) => MinMagnitude(x, y);

        #region Operator Overloads
        #region Logical
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator ~(OffsetSByte value) => new((byte)~value.value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator ^(OffsetSByte left, OffsetSByte right) => new((byte)(left.value ^ right.value));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator &(OffsetSByte left, OffsetSByte right) => new((byte)(left.value & right.value));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator |(OffsetSByte left, OffsetSByte right) => new((byte)(left.value | right.value));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator >>>(OffsetSByte value, int shiftAmount) => (OffsetSByte)((int)value >>> shiftAmount);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator <<(OffsetSByte value, int shiftAmount) => (OffsetSByte)((int)value << shiftAmount);

        #endregion

        #region Arithmetic
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator +(OffsetSByte value) => value;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator -(OffsetSByte value) => new((sbyte)-(sbyte)value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator ++(OffsetSByte value) => (OffsetSByte)((int)value + 1);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator --(OffsetSByte value) => (OffsetSByte)((int)value - 1);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator *(OffsetSByte left, OffsetSByte right) => (OffsetSByte)((int)left * (int)right);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator /(OffsetSByte left, OffsetSByte right) => (OffsetSByte)((int)left / (int)right);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator %(OffsetSByte left, OffsetSByte right) => (OffsetSByte)((int)left % (int)right);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator +(OffsetSByte left, OffsetSByte right) => (OffsetSByte)((int)left + (int)right);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator -(OffsetSByte left, OffsetSByte right) => (OffsetSByte)((int)left - (int)right);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte operator >>(OffsetSByte value, int shiftAmount) => (OffsetSByte)((int)value >> shiftAmount);
        #endregion
        #endregion

        /// <inheritdoc/>
        public static OffsetSByte Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => (OffsetSByte)int.Parse(s, style, provider);
        /// <inheritdoc/>
        public static OffsetSByte Parse(string s, NumberStyles style, IFormatProvider? provider) => (OffsetSByte)int.Parse(s, style, provider);
        /// <inheritdoc/>
        public static OffsetSByte Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => (OffsetSByte)int.Parse(s, provider);
        /// <inheritdoc/>
        public static OffsetSByte Parse(string s, IFormatProvider? provider) => (OffsetSByte)int.Parse(s, provider);
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte PopCount(OffsetSByte value) => (OffsetSByte)MathI.PopCount((uint)(int)value);
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static OffsetSByte TrailingZeroCount(OffsetSByte value) => (OffsetSByte)MathI.TrailingZeroCount((uint)(int)value);

        /// <inheritdoc/>
        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out OffsetSByte result)
        {
            var success = int.TryParse(s, style, provider, out var res);
            result = (OffsetSByte)res;
            return success;
        }
        /// <inheritdoc/>
        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out OffsetSByte result)
        {
            var success = int.TryParse(s, style, provider, out var res);
            result = (OffsetSByte)res;
            return success;
        }

        /// <inheritdoc/>
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out OffsetSByte result)
        {
            var success = int.TryParse(s, provider, out var res);
            result = (OffsetSByte)res;
            return success;
        }

        /// <inheritdoc/>
        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out OffsetSByte result)
        {
            var success = int.TryParse(s, provider, out var res);
            result = (OffsetSByte)res;
            return success;
        }

        /// <inheritdoc/>
        static bool IBinaryInteger<OffsetSByte>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out OffsetSByte value) => throw new NotImplementedException();
        /// <inheritdoc/>
        static bool IBinaryInteger<OffsetSByte>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out OffsetSByte value) => throw new NotImplementedException();

        /// <inheritdoc/>
        public int CompareTo(object? obj) => obj is OffsetSByte value ? value.CompareTo(obj) : 0;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int GetByteCount() => 3;

        /// <inheritdoc/>
        public int GetShortestBitLength() => MathI.GetShortestBitLength((sbyte)this);

        /// <inheritdoc/>
        public string ToString(string? format, IFormatProvider? formatProvider) => ((int)this).ToString(format, formatProvider);

        /// <inheritdoc/>
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => ((int)this).TryFormat(destination, out charsWritten, format, provider);

        /// <inheritdoc/>
        bool IBinaryInteger<OffsetSByte>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();

        /// <inheritdoc/>
        bool IBinaryInteger<OffsetSByte>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();

        /// <inheritdoc/>
        static bool INumberBase<OffsetSByte>.IsNegative(OffsetSByte value) => (sbyte)value < 0;
        static bool INumberBase<OffsetSByte>.TryConvertFromChecked<TOther>(TOther value, out OffsetSByte result)
        {
            if (TOther.TryConvertToChecked(value, out sbyte al))
            {
                result = (OffsetSByte)al;
                return true;
            }
            if (TOther.TryConvertToChecked(value, out short ax))
            {
                result = (OffsetSByte)checked((sbyte)ax);
                return true;
            }
            if (TOther.TryConvertToChecked(value, out int eax))
            {
                result = (OffsetSByte)checked((sbyte)eax);
                return true;
            }
            if (TOther.TryConvertToChecked(value, out long rax))
            {
                result = (OffsetSByte)checked((sbyte)rax);
                return true;
            }
            result = default;
            return false;
        }
        static bool INumberBase<OffsetSByte>.TryConvertFromSaturating<TOther>(TOther value, out OffsetSByte result)
        {
            if (TOther.TryConvertToSaturating(value, out sbyte al))
            {
                result = (OffsetSByte)al;
                return true;
            }
            if (TOther.TryConvertToSaturating(value, out nint nv))
            {
                result = (OffsetSByte)sbyte.CreateSaturating(nv);
                return true;
            }
            if (TOther.TryConvertToSaturating(value, out int eax))
            {
                result = (OffsetSByte)sbyte.CreateSaturating(eax);
                return true;
            }
            if (TOther.TryConvertToSaturating(value, out long rax))
            {
                result = (OffsetSByte)sbyte.CreateSaturating(rax);
                return true;
            }
            result = default;
            return false;
        }
        static bool INumberBase<OffsetSByte>.TryConvertFromTruncating<TOther>(TOther value, out OffsetSByte result)
        {
            if (TOther.TryConvertToTruncating(value, out sbyte al))
            {
                result = (OffsetSByte)al;
                return true;
            }
            if (TOther.TryConvertToTruncating(value, out nint nv))
            {
                result = (OffsetSByte)sbyte.CreateTruncating(nv);
                return true;
            }
            if (TOther.TryConvertToTruncating(value, out int eax))
            {
                result = (OffsetSByte)sbyte.CreateTruncating(eax);
                return true;
            }
            if (TOther.TryConvertToTruncating(value, out long rax))
            {
                result = (OffsetSByte)sbyte.CreateTruncating(rax);
                return true;
            }
            result = default;
            return false;
        }
        static bool INumberBase<OffsetSByte>.TryConvertToChecked<TOther>(OffsetSByte value, out TOther result)
        {
            sbyte sv = (sbyte)value;
            return TOther.TryConvertFromChecked(sv, out result!) || TOther.TryConvertFromChecked((short)sv, out result!) || TOther.TryConvertFromChecked((int)sv, out result!) || TOther.TryConvertFromChecked((long)sv, out result!) || TOther.TryConvertFromChecked((float)sv, out result!) || TOther.TryConvertFromChecked((double)sv, out result!);
        }

        static bool INumberBase<OffsetSByte>.TryConvertToSaturating<TOther>(OffsetSByte value, out TOther result)
        {
            sbyte sv = (sbyte)value;
            return TOther.TryConvertFromSaturating(sv, out result!) || TOther.TryConvertFromSaturating((short)sv, out result!) || TOther.TryConvertFromSaturating((int)sv, out result!) || TOther.TryConvertFromSaturating((long)sv, out result!) || TOther.TryConvertFromSaturating((float)sv, out result!) || TOther.TryConvertFromSaturating((double)sv, out result!);
        }

        static bool INumberBase<OffsetSByte>.TryConvertToTruncating<TOther>(OffsetSByte value, out TOther result)
        {
            sbyte sv = (sbyte)value;
            return TOther.TryConvertFromTruncating(sv, out result!) || TOther.TryConvertFromTruncating((short)sv, out result!) || TOther.TryConvertFromTruncating((int)sv, out result!) || TOther.TryConvertFromTruncating((long)sv, out result!) || TOther.TryConvertFromTruncating((float)sv, out result!) || TOther.TryConvertFromTruncating((double)sv, out result!);
        }
    }
}
