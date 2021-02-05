using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen
{
    /// <summary>
    /// Represents a Fixed-Point Number in Signed Q0.15 format using <a href="https://en.wikipedia.org/wiki/Two%27s_complement">Two's Complement</a> format.<br/>
    /// For reference of "Qm.n" notation: See <a href="https://source.android.com/devices/audio/data_formats#fixed">here</a> and <a href="https://en.wikipedia.org/wiki/Q_(number_format)">here</a>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public readonly struct Fixed16 : IEquatable<Fixed16>, IComparable<Fixed16>
    {
        /// <summary>
        /// The internal value stored in memory.
        /// </summary>
        [FieldOffset(0)]
        public readonly short Value;

        #region Constants

        private const float V2FRatio = 0.000030517578125f;

        /// <summary>
        /// Represents the largest possible value of <see cref="OffsetSByte"/>. This field is constant and read-only.
        /// </summary>
        public static readonly Fixed16 MaxValue = new Fixed16(short.MaxValue);

        /// <summary>
        /// Represents the smallest possible value of <see cref="OffsetSByte"/>. This field is constant and read-only.
        /// </summary>
        public static readonly Fixed16 MinValue = new Fixed16(short.MaxValue);

        /// <summary>
        /// Represents the number zero (0).
        /// </summary>
        public static readonly Fixed16 Zero = new Fixed16(0);

        /// <summary>
        /// Represents the smallest positive <see cref="Fixed16"/> value that is greater than zero. This field is constant and read-only.
        /// </summary>
        public static readonly Fixed16 Epsilon = new Fixed16(1);

        #endregion Constants

        /// <summary>
        /// Initializes a new instance of the <see cref="Fixed16"/> struct.
        /// </summary>
        /// <param name="internalValue">The internal value stored in memory.</param>
        public Fixed16(short internalValue)
        {
            Value = internalValue;
        }

        /// <summary>
        /// Gets the value represented in <see cref="float"/>.
        /// </summary>
        /// <value>
        /// The float value.
        /// </value>
        public float FloatValue => Value * V2FRatio;

        #region Arithmetics

        /// <summary>
        /// Adds two specified <see cref="Fixed16"/> values.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>
        /// The result of adding <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        public static Fixed16 operator +(Fixed16 left, Fixed16 right) => new Fixed16((short)(left.Value + right.Value));

        /// <summary>
        /// Subtracts two specified <see cref="Fixed16"/> values.
        /// </summary>
        /// <param name="left">The minuend.</param>
        /// <param name="right">The subtrahend.</param>
        /// <returns>
        /// The result of subtracting <paramref name="right"/> from <paramref name="left"/>.
        /// </returns>
        public static Fixed16 operator -(Fixed16 left, Fixed16 right) => new Fixed16((short)(left.Value - right.Value));

        /// <summary>
        /// Multiplies two specified <see cref="Fixed16"/> values.
        /// </summary>
        /// <param name="left">The first value to multiply.</param>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>
        /// The result of multiplying <paramref name="left"/> by <paramref name="right"/>.
        /// </returns>
        public static Fixed16 operator *(Fixed16 left, Fixed16 right) => new Fixed16((short)(left.Value * right.Value >> 15));

        /// <summary>
        /// Divides two specified <see cref="Fixed16"/> values.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>
        /// The result of dividing <paramref name="left"/> by <paramref name="right"/>.
        /// </returns>
        public static double operator /(Fixed16 left, Fixed16 right) => (double)left.Value / right.Value;

        /// <summary>
        /// Returns the remainder resulting from dividing two specified <see cref="Fixed16"/> values.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>
        /// The remainder resulting from dividing <paramref name="left"/> by <paramref name="right"/>.
        /// </returns>
        public static Fixed16 operator %(Fixed16 left, Fixed16 right) => new Fixed16((short)(left.Value % right.Value));

        /// <summary>
        /// Returns the value of the <see cref="Fixed16"/> operand (the sign of the operand is unchanged).
        /// </summary>
        /// <param name="value">The operand to return.</param>
        /// <returns>
        /// The value of the operand, <paramref name="value"/>.
        /// </returns>
        public static Fixed16 operator +(Fixed16 value) => value;

        /// <summary>
        /// Increments the <see cref="Fixed16"/> operand by <see cref="Epsilon"/>.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <returns>
        /// The value of <paramref name="value"/> incremented by <see cref="Epsilon"/>.
        /// </returns>
        public static Fixed16 operator ++(Fixed16 value) => new Fixed16((short)(value.Value + 1));

        /// <summary>
        /// Negates the value of the specified <see cref="Fixed16"/> operand.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>
        /// The result of <paramref name="value"/> multiplied by negative one (-1).
        /// </returns>
        public static Fixed16 operator -(Fixed16 value) => new Fixed16((short)-value.Value);

        /// <summary>
        /// Decrements the <see cref="Fixed16"/> operand by <see cref="Epsilon"/>.
        /// </summary>
        /// <param name="value">The value to decrement.</param>
        /// <returns>
        /// The value of <paramref name="value"/> decremented by <see cref="Epsilon"/>.
        /// </returns>
        public static Fixed16 operator --(Fixed16 value) => new Fixed16((short)(value.Value - 1));

        #endregion Arithmetics

        #region Conversion

        /// <summary>
        /// Performs an implicit conversion from <see cref="Fixed16"/> to <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator float(Fixed16 value) => value.FloatValue;

        /// <summary>
        /// Performs an explicit conversion from <see cref="Fixed16"/> to <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator double(Fixed16 value) => value.FloatValue;

        /// <summary>
        /// Performs an explicit conversion from <see cref="float"/> to <see cref="Fixed16"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Fixed16(float value) => new Fixed16((short)(value * 32768.0f));

        /// <summary>
        /// Performs an explicit conversion from <see cref="double"/> to <see cref="Fixed16"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Fixed16(double value) => new Fixed16((short)(value * 32768.0));

        #endregion Conversion

        #region Comparison

        /// <summary>
        /// Compares this instance to a specified 16-bit signed fixed-point number and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">An number to compare.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.</returns>
        public int CompareTo(Fixed16 other) => other.Value.CompareTo(Value);

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Fixed16"/> is less than another specified <see cref="Fixed16"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator <(Fixed16 left, Fixed16 right) => left.Value < right.Value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Fixed16"/> is less than or equal to another specified <see cref="Fixed16"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator <=(Fixed16 left, Fixed16 right) => left.Value <= right.Value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Fixed16"/> is greater than another specified <see cref="Fixed16"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator >(Fixed16 left, Fixed16 right) => left.Value > right.Value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Fixed16"/> is greater than or equal to another specified <see cref="Fixed16"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator >=(Fixed16 left, Fixed16 right) => left.Value >= right.Value;

        #endregion Comparison

        #region Equality

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is Fixed16 @fixed && Equals(@fixed);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Fixed16 other) => Value == other.Value;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => -1937169414 + Value.GetHashCode();

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Fixed16"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Fixed16"/> to compare.</param>
        /// <param name="right">The second <see cref="Fixed16"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(Fixed16 left, Fixed16 right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Fixed16"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Fixed16"/> to compare.</param>
        /// <param name="right">The second  <see cref="Fixed16"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(Fixed16 left, Fixed16 right) => !(left == right);

        #endregion Equality
    }
}
