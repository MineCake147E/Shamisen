using System;
using System.Buffers.Binary;
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
    public readonly partial struct UInt24 : IEquatable<UInt24>, IComparable<UInt24>
    {
        /// <summary>
        /// In Big-Endianed systems, "middle-tail" becomes like "head-middle"
        /// </summary>
        [FieldOffset(0)]
        private readonly ushort midtail;

        [FieldOffset(2)]
        private readonly byte head;

        private byte Tail
        {
            get => (byte)midtail;
            init => midtail = (ushort)((midtail & 0xff00) | value);
        }

        private byte Middle
        {
            get => (byte)(midtail >> 8);
            init => midtail = (ushort)((value << 8) | ((byte)midtail));
        }

        /// <summary>
        /// Represents the largest possible value of an System.Int24. This field is constant.
        /// </summary>
        public static UInt24 MaxValue => (UInt24)16777215;

        /// <summary>
        /// Represents the smallest possible value of System.Int24. This field is constant.
        /// </summary>
        public static UInt24 MinValue => (UInt24)0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt24"/> struct.
        /// </summary>
        /// <param name="value">The source <see cref="uint"/> value. Mask:0x00ffffff</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UInt24(uint value)
        {
            Unsafe.SkipInit(out this);
            if (BitConverter.IsLittleEndian)
            {
                midtail = (ushort)value;
                head = (byte)(value >> 16);
            }
            else
            {
                head = (byte)value;
                midtail = (ushort)(value >> 8);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt24"/> struct.
        /// </summary>
        /// <param name="midtail">The first 2 bytes of the value.</param>
        /// <param name="head">The last byte of the value.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UInt24(ushort midtail, byte head)
        {
            Unsafe.SkipInit(out this);
            this.midtail = midtail;
            this.head = head;
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
            Unsafe.SkipInit(out this);
            this.head = head;
            Middle = middle;
            Tail = tail;
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
                if (BitConverter.IsLittleEndian)
                {
                    var eax = (uint)value.head << 16;
                    var ebx = (uint)value.midtail;
                    eax |= ebx;
                    return eax >> 8;
                }
                else
                {
                    var eax = (uint)value.midtail << 16;
                    var ebx = (uint)value.head << 8;
                    eax |= ebx;
                    return BinaryPrimitives.ReverseEndianness(eax);
                }
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
        public bool Equals(UInt24 other) => Tail == other.Tail && Middle == other.Middle && head == other.head;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(Tail, Middle, head);

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
        public static UInt24 ReverseEndianness(UInt24 value) => new(value.Tail, value.Middle, value.head);

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
