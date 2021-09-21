using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shamisen
{
    /// <summary>
    /// Represents a Fixed-Point Number in Signed Q0.63 format using <a href="https://en.wikipedia.org/wiki/Two%27s_complement">Two's Complement</a> format.<br/>
    /// Useful for precisely counting angles in multiples of <see cref="Math.PI"/>.<br/>
    /// For reference of "Qm.n" notation: See <a href="https://source.android.com/devices/audio/data_formats#fixed">here</a> and <a href="https://en.wikipedia.org/wiki/Q_(number_format)">here</a>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = sizeof(long))]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct Fixed64 : IEquatable<Fixed64>, IComparable<Fixed64>
    {
        /// <summary>
        /// The internal value stored in memory.
        /// </summary>
        [FieldOffset(0)]
        public readonly long Value;

        private const float V2FRatio = 1.0f / unchecked((ulong)long.MinValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="Fixed64"/> struct.
        /// </summary>
        /// <param name="internalValue">The internal value stored in memory.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public Fixed64(long internalValue) => Value = internalValue;

        /// <summary>
        /// Gets the value represented in <see cref="float"/>.
        /// </summary>
        /// <value>
        /// The float value.
        /// </value>
        public float FloatValue => Value * V2FRatio;

        /// <summary>
        /// Gets the value represented in <see cref="double"/>.
        /// </summary>
        /// <value>
        /// The float value.
        /// </value>
        public double DoubleValue => Value * (double)V2FRatio;

        /// <summary>
        /// Represents the largest possible value of <see cref="Fixed64"/>. This field is constant and read-only.
        /// </summary>
        public static Fixed64 MaxValue
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new(long.MaxValue);
        }

        /// <summary>
        /// Represents the smallest possible value of <see cref="Fixed64"/>. This field is constant and read-only.
        /// </summary>
        public static Fixed64 MinValue
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new(long.MaxValue);
        }

        /// <summary>
        /// Represents the number zero (0).
        /// </summary>
        public static Fixed64 Zero
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new(0);
        }

        /// <summary>
        /// Represents the smallest positive <see cref="Fixed64"/> value that is greater than zero. This field is constant and read-only.
        /// </summary>
        public static Fixed64 Epsilon
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new(1);
        }

        #region Arithmetics

        /// <summary>
        /// Adds two specified <see cref="Fixed64"/> values.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>
        /// The result of adding <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 operator +(Fixed64 left, Fixed64 right) => new(left.Value + right.Value);

        /// <summary>
        /// Subtracts two specified <see cref="Fixed64"/> values.
        /// </summary>
        /// <param name="left">The minuend.</param>
        /// <param name="right">The subtrahend.</param>
        /// <returns>
        /// The result of subtracting <paramref name="right"/> from <paramref name="left"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 operator -(Fixed64 left, Fixed64 right) => new(left.Value - right.Value);

        /// <summary>
        /// Multiplies two specified <see cref="Fixed64"/> values.
        /// </summary>
        /// <param name="left">The first value to multiply.</param>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>
        /// The result of multiplying <paramref name="left"/> by <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 operator *(Fixed64 left, Fixed64 right)
        {
            var (low, high) = MathI.BigMul(left.Value, right.Value);
            return new(high * 2 + (long)(low / (1ul << 63)));
        }

        /// <summary>
        /// Returns the specified value to the <paramref name="power"/>th power.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="power">The power.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 PowerOfInteger(Fixed64 value, uint power)
        {
            switch (power)
            {
                case 0:
                    return MinValue;
                case 1:
                    return value;
                case 2:
                    return Square(value);
                case 3:
                    return Square(value) * value;
                case 4:
                    return Square(Square(value));
                case 5:
                    return Square(Square(value)) * value;
                case 6:
                    return Square(Square(value) * value);
                default:
                    {
                        var q = value;
                        var r = value;
                        while (power > 0)
                        {
                            q = Square(q);
                            power >>= 1;
                            if ((power & 1u) > 0)
                            {
                                r *= q;
                            }
                        }
                        return r;
                    }
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Fixed64 PowerPOT(Fixed64 value, uint shift)
        {
            var t = value;
            for (uint i = 0; i < shift; i++)
            {
                t = Square(t);
            }
            return t;
        }

        /// <summary>
        /// Squares the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 Square(Fixed64 value) => value * value;

        /// <summary>
        /// Divides two specified <see cref="Fixed64"/> values.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>
        /// The result of dividing <paramref name="left"/> by <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double operator /(Fixed64 left, Fixed64 right) => (double)left.Value / right.Value;

        /// <summary>
        /// Returns the remainder resulting from dividing two specified <see cref="Fixed64"/> values.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>
        /// The remainder resulting from dividing <paramref name="left"/> by <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 operator %(Fixed64 left, Fixed64 right) => new(left.Value % right.Value);

        /// <summary>
        /// Returns the value of the <see cref="Fixed64"/> operand (the sign of the operand is unchanged).
        /// </summary>
        /// <param name="value">The operand to return.</param>
        /// <returns>
        /// The value of the operand, <paramref name="value"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 operator +(Fixed64 value) => value;

        /// <summary>
        /// Increments the <see cref="Fixed64"/> operand by <see cref="Epsilon"/>.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <returns>
        /// The value of <paramref name="value"/> incremented by <see cref="Epsilon"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 operator ++(Fixed64 value) => new(value.Value + 1);

        /// <summary>
        /// Negates the value of the specified <see cref="Fixed64"/> operand.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>
        /// The result of <paramref name="value"/> multiplied by negative one (-1).
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 operator -(Fixed64 value) => new(unchecked(-value.Value));

        /// <summary>
        /// Decrements the <see cref="Fixed64"/> operand by <see cref="Epsilon"/>.
        /// </summary>
        /// <param name="value">The value to decrement.</param>
        /// <returns>
        /// The value of <paramref name="value"/> decremented by <see cref="Epsilon"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Fixed64 operator --(Fixed64 value) => new(value.Value - 1);

        #endregion Arithmetics

        #region Conversion

        /// <summary>
        /// Performs an explicit conversion from <see cref="Fixed64"/> to <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator float(Fixed64 value) => value.FloatValue;

        /// <summary>
        /// Performs an explicit conversion from <see cref="Fixed64"/> to <see cref="double"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator double(Fixed64 value) => value.DoubleValue;

        /// <summary>
        /// Performs an explicit conversion from <see cref="float"/> to <see cref="Fixed64"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator Fixed64(float value) => new((long)(value * 9.223372036854776E+18f));

        /// <summary>
        /// Performs an explicit conversion from <see cref="double"/> to <see cref="Fixed64"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator Fixed64(double value)
        {
            if (value == 1.0) return Fixed64.MinValue; //-1 == 1
            return new((long)(value * 9.223372036854776E+18));
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Fixed64"/> to <see cref="long"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The internal <see cref="long"/> value of specified <paramref name="value"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator long(Fixed64 value) => value.Value;

        /// <summary>
        /// Performs an explicit conversion from <see cref="long"/> to <see cref="Fixed64"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator Fixed64(long value) => new(value);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override string? ToString() => GetDebuggerDisplay();

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private string GetDebuggerDisplay() => $"{DoubleValue}";

        #endregion Conversion

        #region Comparison

        /// <summary>
        /// Compares this instance to a specified 16-bit signed fixed-point number and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">An number to compare.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public int CompareTo(Fixed64 other) => other.Value.CompareTo(Value);

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Fixed64"/> is less than another specified <see cref="Fixed64"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <(Fixed64 left, Fixed64 right) => left.Value < right.Value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Fixed64"/> is less than or equal to another specified <see cref="Fixed64"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <=(Fixed64 left, Fixed64 right) => left.Value <= right.Value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Fixed64"/> is greater than another specified <see cref="Fixed64"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >(Fixed64 left, Fixed64 right) => left.Value > right.Value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Fixed64"/> is greater than or equal to another specified <see cref="Fixed64"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >=(Fixed64 left, Fixed64 right) => left.Value >= right.Value;

        #endregion Comparison

        #region Equality

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override bool Equals(object? obj) => obj is Fixed64 @fixed && Equals(@fixed);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool Equals(Fixed64 other) => Value == other.Value;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Fixed64"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Fixed64"/> to compare.</param>
        /// <param name="right">The second <see cref="Fixed64"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(Fixed64 left, Fixed64 right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Fixed64"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Fixed64"/> to compare.</param>
        /// <param name="right">The second  <see cref="Fixed64"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(Fixed64 left, Fixed64 right) => !(left == right);

        #endregion Equality
    }
}
