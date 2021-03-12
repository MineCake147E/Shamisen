using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen
{
    /// <summary>
    /// Represents a value that is offset 128 inside 8-bit PCM.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct OffsetSByte : IEquatable<OffsetSByte>, IComparable<OffsetSByte>
    {
        private const byte Inverter = 0x80;

        [FieldOffset(0)]
        private readonly byte value;

        /// <summary>
        /// Represents the largest possible value of <see cref="OffsetSByte"/>. This field is constant and read-only.
        /// </summary>
        public static readonly OffsetSByte MaxValue = new OffsetSByte(byte.MaxValue);

        /// <summary>
        /// Represents the smallest possible value of <see cref="OffsetSByte"/>. This field is constant and read-only.
        /// </summary>
        public static readonly OffsetSByte MinValue = new OffsetSByte(byte.MinValue);

        /// <summary>
        /// Represents the number zero (0).
        /// </summary>
        public static readonly OffsetSByte Zero = new OffsetSByte(128);

        /// <summary>
        /// Represents the number one (1).
        /// </summary>
        public static readonly OffsetSByte One = new OffsetSByte(129);

        /// <summary>
        /// Represents the number negative one (-1).
        /// </summary>
        public static readonly OffsetSByte MinusOne = new OffsetSByte(127);

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetSByte"/> struct.
        /// </summary>
        /// <param name="value">The represented value.</param>
        public OffsetSByte(sbyte value)
        {
            unchecked
            {
                byte vp = (byte)value;

                this.value = (byte)(vp ^ Inverter);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetSByte"/> struct.
        /// </summary>
        /// <param name="value">The stored value.</param>
        public OffsetSByte(byte value)
        {
            this.value = value;
        }

        private string GetDebuggerDisplay() => ToString();

        #region Equality

        /// <summary>
        /// Compares this instance to a specified 32-bit signed integer and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">An integer to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value.
        /// </returns>
        public int CompareTo(OffsetSByte other) => value - other.value;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is OffsetSByte @byte && Equals(@byte);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(OffsetSByte other) => value == other.value;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => -1584136870 + value.GetHashCode();

        /// <summary>
        /// Indicates whether the values of two specified <see cref="OffsetSByte"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="OffsetSByte"/> to compare.</param>
        /// <param name="right">The second <see cref="OffsetSByte"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(OffsetSByte left, OffsetSByte right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="OffsetSByte"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="OffsetSByte"/> to compare.</param>
        /// <param name="right">The second  <see cref="OffsetSByte"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(OffsetSByte left, OffsetSByte right) => !(left == right);

        #endregion Equality

        #region Conversion

        /// <summary>
        /// Performs an explicit conversion from <see cref="OffsetSByte"/> to <see cref="sbyte"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator sbyte(OffsetSByte value)
        {
            unchecked
            {
                return (sbyte)(value.value ^ Inverter);
            }
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="OffsetSByte"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator int(OffsetSByte value) => (sbyte)value;

        /// <summary>
        /// Performs an explicit conversion from <see cref="OffsetSByte"/> to <see cref="byte"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator byte(OffsetSByte value) => value.value;

        /// <summary>
        /// Performs an explicit conversion from <see cref="sbyte"/> to <see cref="OffsetSByte"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator OffsetSByte(sbyte value) => new OffsetSByte(value);

        /// <summary>
        /// Performs an explicit conversion from <see cref="int"/> to <see cref="OffsetSByte"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator OffsetSByte(int value) => (OffsetSByte)unchecked((sbyte)value);

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents the value of this instance.
        /// </returns>
        public override string ToString() => ((sbyte)this).ToString();

        /// <summary>
        /// Converts the string representation of a number to its <see cref="OffsetSByte"/> equivalent.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <returns>The equivalent to the number contained in <paramref name="s"/>.</returns>
        public static OffsetSByte Parse(string s) => new OffsetSByte(sbyte.Parse(s));

        /// <summary>
        /// Converts the string representation of a number to its <see cref="OffsetSByte"/> equivalent.<br/>
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <param name="result">When this method returns, contains the <see cref="OffsetSByte"/> number that is equivalent to the numeric value contained in <paramref name="s"/>, if the conversion succeeded, or zero if the conversion failed.
        /// The conversion fails if the <paramref name="s"/> parameter is <c>null</c> or <see cref="string.Empty"/>, is not a number in a valid format, or represents a number less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
        /// This parameter is passed uninitialized; any value originally supplied in result is overwritten.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParse(string s, out OffsetSByte result)
        {
            var g = sbyte.TryParse(s, out var b);
            result = new OffsetSByte(b);
            return g;
        }

        #endregion Conversion

        #region Comparison

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="OffsetSByte"/> is less than another specified <see cref="OffsetSByte"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator <(OffsetSByte left, OffsetSByte right) => left.value < right.value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="OffsetSByte"/> is less than or equal to another specified <see cref="OffsetSByte"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator <=(OffsetSByte left, OffsetSByte right) => left.value <= right.value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="OffsetSByte"/> is greater than another specified <see cref="OffsetSByte"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator >(OffsetSByte left, OffsetSByte right) => left.value > right.value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="OffsetSByte"/> is greater than or equal to another specified <see cref="OffsetSByte"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator >=(OffsetSByte left, OffsetSByte right) => left.value >= right.value;

        #endregion Comparison
    }
}
