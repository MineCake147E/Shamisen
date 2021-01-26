using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Represents a length of some data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DataLength : IEquatable<DataLength>, IComparable<DataLength>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataLength" /> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public DataLength(ulong value)
        {
            Length = value;
        }

        /// <summary>
        /// Represents the fact that the source stream is infinitely long.
        /// </summary>
        public static DataLength Infinity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new DataLength(ulong.MaxValue);
        }

        /// <summary>
        /// Gets the actual length available.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public ulong Length { get; }

        /// <summary>
        /// Gets a value indicating whether the available length of data is infinity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this <see cref="Length"/> is infinity; otherwise, <c>false</c>.
        /// </value>
        public bool IsInfinity { get => Length == ulong.MaxValue; }

        /// <summary>
        /// Casts the size value of this instance.
        /// </summary>
        /// <typeparam name="TFrom">The type of from.</typeparam>
        /// <typeparam name="TTo">The type of to.</typeparam>
        /// <returns></returns>
        public DataLength Cast<TFrom, TTo>() where TFrom : unmanaged where TTo : unmanaged
            => IsInfinity ? Infinity : Length * (uint)Unsafe.SizeOf<TFrom>() / (uint)Unsafe.SizeOf<TTo>();

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static DataLength operator /(DataLength left, ulong right) => left.IsInfinity ? Infinity : new DataLength(left.Length / right);

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static DataLength operator *(DataLength left, ulong right)
        {
            if (left.IsInfinity) return Infinity;
            return left.Length >= ulong.MaxValue / right ? Infinity : new DataLength(left.Length * right);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="long" /> to <see cref="DataLength" />.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator DataLength(ulong value) => new DataLength(value);

        /// <summary>
        /// Performs an explicit conversion from <see cref="DataLength"/> to <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator ulong(DataLength value) => value.Length;

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(DataLength other) => Length.CompareTo(other.Length);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is DataLength length && Equals(length);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DataLength other) => Length == other.Length;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => -1584136870 + Length.GetHashCode();

        /// <summary>
        /// Indicates whether the values of two specified <see cref="DataLength" /> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="DataLength" /> to compare.</param>
        /// <param name="right">The second <see cref="DataLength" /> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(DataLength left, DataLength right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="DataLength"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="DataLength"/> to compare.</param>
        /// <param name="right">The second  <see cref="DataLength"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(DataLength left, DataLength right) => !(left == right);

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(DataLength left, DataLength right) => left.Length < right.Length;

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(DataLength left, DataLength right) => left.Length <= right.Length;

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(DataLength left, DataLength right) => left.Length > right.Length;

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(DataLength left, DataLength right) => left.Length >= right.Length;
    }
}
