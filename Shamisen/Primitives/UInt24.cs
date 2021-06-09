using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Represents an unsigned 24-bit number.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct UInt24 : IEquatable<UInt24>, IComparable<UInt24>
    {
        [FieldOffset(0)]
        private readonly byte tail;

        [FieldOffset(1)]
        private readonly byte middle;

        [FieldOffset(2)]
        private readonly byte head;

        /// <summary>
		/// Represents the largest possible value of an System.Int24. This field is constant.
		/// </summary>
		public static readonly UInt24 MaxValue = (UInt24)16777215;

        /// <summary>
        /// Represents the smallest possible value of System.Int24. This field is constant.
        /// </summary>
        public static readonly UInt24 MinValue = (UInt24)0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt24"/> struct.
        /// </summary>
        /// <param name="value">The source <see cref="uint"/> value. Mask:0x00ffffff</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UInt24(uint value)
        {
            tail = (byte)value;        //mov [rcx], dl
            value >>= 8;                        //sar edx, 0x8
            middle = (byte)value;      //mov [rcx+0x1], dl
            value >>= 8;                        //sar edx, 0x8
            head = (byte)value;        //mov [rcx+0x2], dl
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt24"/> struct.
        /// </summary>
        /// <param name="head">The head.</param>
        /// <param name="middle">The middle.</param>
        /// <param name="tail">The tail.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UInt24(byte head, byte middle, byte tail)
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
        public static implicit operator uint(UInt24 value)
        {
            unchecked
            {
                uint eax = value.head;
                eax <<= 8;
                eax |= value.middle;
                eax <<= 8;
                eax |= value.tail;
                return eax;
            }
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="uint"/> to <see cref="UInt24"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator UInt24(uint value) => new(value);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt24"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt24"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(UInt24 left, UInt24 right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt24"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt24"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(UInt24 left, UInt24 right) => !(left == right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is UInt24 @int && Equals(@int);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(UInt24 other) => tail == other.tail && middle == other.middle && head == other.head;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(tail, middle, head);

        /// <summary>
        /// Determines whether one specified <see cref="UInt24"/> is less than another specified <see cref="UInt24"/>.
        /// </summary>
        /// <param name="left">The first <see cref="UInt24"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left is less than right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <(UInt24 left, UInt24 right) => (uint)left < right;

        /// <summary>
        /// Determines whether one specified <see cref="UInt24"/> is greater than another specified <see cref="UInt24"/> value.
        /// </summary>
        /// <param name="left">The first <see cref="UInt24"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left is greater than right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >(UInt24 left, UInt24 right) => (uint)left > right;

        /// <summary>
        /// Returns a value that indicates whether a specified <see cref="UInt24"/> is less than or equal to another specified <see cref="UInt24"/>.
        /// </summary>
        /// <param name="left">The first <see cref="UInt24"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left is less than or equal to right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <=(UInt24 left, UInt24 right) => (uint)left <= right;

        /// <summary>
        /// Determines whether one specified <see cref="UInt24"/> is greater than or equal to another specified <see cref="UInt24"/>.
        /// </summary>
        /// <param name="left">The first <see cref="UInt24"/> to compare.</param>
        /// <param name="right">The second  <see cref="UInt24"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if <see cref="UInt24"/> is greater than or equal to <see cref="UInt24"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >=(UInt24 left, UInt24 right) => (uint)left >= right;

        /// <summary>
        /// Reverses endianness of the given <see cref="UInt24"/> value.
        /// </summary>
        /// <param name="value">The value to reverse endianness.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 ReverseEndianness(UInt24 value) => new(value.tail, value.middle, value.head);

        /// <summary>
        /// Compares the value of this instance to a specified <see cref="UInt24"/> value and returns an integer that indicates whether this instance is less than, equal to, or greater than the specified <see cref="UInt24"/> value.
        /// </summary>
        /// <param name="other">The <see cref="UInt24"/> to compare to the current instance.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and the other parameter.
        /// </returns>
        public int CompareTo(UInt24 other) => ((uint)this).CompareTo(other);

        /// <summary>
        /// Gets the debugger display.
        /// </summary>
        /// <returns></returns>
        private string GetDebuggerDisplay() => $"{(uint)this}";

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns></returns>
        public override string? ToString() => GetDebuggerDisplay();
    }
}
