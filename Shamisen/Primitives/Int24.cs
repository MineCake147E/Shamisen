using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen
{
    /// <summary>
	/// An simple representation of 24bit signed integer.
	/// </summary>
	/// <seealso cref="System.IEquatable{T}" />
	[StructLayout(LayoutKind.Explicit, Size = 3)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct Int24 : IComparable<Int24>, IEquatable<Int24>
    {
        private const int NegativeValueOrMask = -0x80_0000;
        private const int Mask = -0x7F80_0001;

        [FieldOffset(0)]
        private readonly byte tail;

        [FieldOffset(1)]
        private readonly byte middle;

        [FieldOffset(2)]
        private readonly byte head;

        /// <summary>
		/// Represents the largest possible value of an System.Int24. This field is constant.
		/// </summary>
		public static readonly Int24 MaxValue = (Int24)8388607;

        /// <summary>
        /// Represents the smallest possible value of System.Int24. This field is constant.
        /// </summary>
        public static readonly Int24 MinValue = (Int24)(-8388608);

        private bool IsNegative => (head & 0x80) == 0x80;

        /// <summary>
        /// Initializes a new instance of the <see cref="Int24"/> struct.
        /// </summary>
        /// <param name="value">The source <see cref="int"/> value. Mask:0x807fffff</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public Int24(int value)
        {
            value <<= 8;                        //shl edx, 0x8
            value >>= 8;                        //sar edx, 0x8
            tail = (byte)(value & 0xff);        //mov [rcx], dl
            value >>= 8;                        //sar edx, 0x8
            middle = (byte)(value & 0xff);      //mov [rcx+0x1], dl
            value >>= 8;                        //sar edx, 0x8
            head = (byte)(value & 0xff);        //mov [rcx+0x2], dl
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Int24"/> struct.
        /// </summary>
        /// <param name="head">The head.</param>
        /// <param name="middle">The middle.</param>
        /// <param name="tail">The tail.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public Int24(byte head, byte middle, byte tail)
        {
            this.head = head;
            this.middle = middle;
            this.tail = tail;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Int24"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator int(Int24 value)
        {
            unchecked
            {
                uint eax = value.head;
                eax <<= 8;
                eax |= value.middle;
                eax <<= 8;
                eax |= value.tail;
                eax <<= 8;
                return (int)eax >> 8;
            }
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="int"/> to <see cref="Int24"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator Int24(int value) => new(value);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Int24"/> objects are equal.
        /// </summary>
        /// <param name="int1">The first <see cref="Int24"/> to compare.</param>
        /// <param name="int2">The second <see cref="Int24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the value of int1 is the same as the value of int2; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(Int24 int1, Int24 int2) => int1.Equals(int2);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override string ToString() => ((int)this).ToString();

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 operator -(Int24 value) => new(-(int)value);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Int24"/> objects are not equal.
        /// </summary>
        /// <param name="int1">The first <see cref="Int24"/> to compare.</param>
        /// <param name="int2">The second <see cref="Int24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if int1 and int2 are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(Int24 int1, Int24 int2) => !(int1 == int2);

        /// <summary>
        /// Determines whether one specified <see cref="Int24"/> is less than another specified <see cref="Int24"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Int24"/> to compare.</param>
        /// <param name="right">The second <see cref="Int24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left is less than right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <(Int24 left, Int24 right) => (int)left < right;

        /// <summary>
        /// Determines whether one specified <see cref="Int24"/> is greater than another specified <see cref="Int24"/> value.
        /// </summary>
        /// <param name="left">The first <see cref="Int24"/> to compare.</param>
        /// <param name="right">The second <see cref="Int24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left is greater than right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >(Int24 left, Int24 right) => (int)left > right;

        /// <summary>
        /// Returns a value that indicates whether a specified <see cref="Int24"/> is less than or equal to another specified <see cref="Int24"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Int24"/> to compare.</param>
        /// <param name="right">The second <see cref="Int24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left is less than or equal to right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <=(Int24 left, Int24 right) => (int)left <= right;

        /// <summary>
        /// Determines whether one specified <see cref="Int24"/> is greater than or equal to another specified <see cref="Int24"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Int24"/> to compare.</param>
        /// <param name="right">The second  <see cref="Int24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if <see cref="Int24"/> is greater than or equal to <see cref="Int24"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >=(Int24 left, Int24 right) => (int)left >= right;

        /// <summary>
        /// Reverses endianness of the given <see cref="Int24"/> value.
        /// </summary>
        /// <param name="value">The value to reverse endianness.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 ReverseEndianness(Int24 value) => new(value.tail, value.middle, value.head);

        /// <summary>
        /// Compares the value of this instance to a specified <see cref="Int24"/> value and returns an integer that indicates whether this instance is less than, equal to, or greater than the specified <see cref="Int24"/> value.
        /// </summary>
        /// <param name="other">The <see cref="Int24"/> to compare to the current instance.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and the other parameter.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public int CompareTo(Int24 other) => ((int)this).CompareTo(other);

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override bool Equals(object? obj) => obj is Int24 @int && Equals(@int);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool Equals(Int24 other) => tail == other.tail &&
                   middle == other.middle &&
                   head == other.head;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override int GetHashCode()
        {
            int hashCode = -428595538;
            hashCode = hashCode * -1521134295 + tail.GetHashCode();
            hashCode = hashCode * -1521134295 + middle.GetHashCode();
            hashCode = hashCode * -1521134295 + head.GetHashCode();
            return hashCode;
        }

        private string GetDebuggerDisplay() => ToString();
    }
}
