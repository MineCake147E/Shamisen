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
    /// Represents a result of read operation.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct NativeReadResult : IEquatable<NativeReadResult>, IComparable<NativeReadResult>
    {
        [FieldOffset(0)]
        private readonly nint value;

        /// <summary>
        /// Represents the fact that the source stream is empty right now.
        /// </summary>
        public static NativeReadResult EndOfStream
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new(nint.MinValue);
        }

        /// <summary>
        /// Represents the fact that the source stream is waiting for source's IO.
        /// </summary>
        public static NativeReadResult WaitingForSource
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new(0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeReadResult"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public NativeReadResult(nint value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets a value indicating whether the source has already reached the end of stream.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the source has already reached the end of stream; otherwise, <c>false</c>.
        /// </value>
        public bool IsEndOfStream
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => value == nint.MinValue;
        }

        /// <summary>
        /// Gets a value indicating whether the buffer has data.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the buffer has data; otherwise, <c>false</c>.
        /// </value>
        public bool HasData
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => value > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the buffer has no data.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the buffer has no data; otherwise, <c>false</c>.
        /// </value>
        public bool HasNoData
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => value <= 0;
        }

        /// <summary>
        /// Gets the actual length read.
        /// </summary>
        /// <value>
        /// The actual length read.
        /// </value>
        public nint Length
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => MathI.Rectify(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="NativeReadResult"/> to <see cref="nuint"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator nuint(NativeReadResult value) => (nuint)value.Length;

        /// <summary>
        /// Performs an explicit conversion from <see cref="NativeReadResult"/> to <see cref="nint"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator nint(NativeReadResult value) => value.Length;

        /// <summary>
        /// Performs an implicit conversion from <see cref="nint"/> to <see cref="NativeReadResult"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator NativeReadResult(nint value) => new(value);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override bool Equals(object? obj) => obj is NativeReadResult result && Equals(result);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool Equals(NativeReadResult other) => value == other.value;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override int GetHashCode() => value.GetHashCode();

        /// <summary>
        /// Compares this instance to a specified <see cref="NativeReadResult"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">An <see cref="NativeReadResult"/>  to compare.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public int CompareTo(NativeReadResult other) => value.CompareTo(other.value);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="NativeReadResult"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="NativeReadResult"/> to compare.</param>
        /// <param name="right">The second <see cref="NativeReadResult"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(NativeReadResult left, NativeReadResult right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="NativeReadResult"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="NativeReadResult"/> to compare.</param>
        /// <param name="right">The second  <see cref="NativeReadResult"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(NativeReadResult left, NativeReadResult right) => !(left == right);

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="NativeReadResult"/> is less than another specified <see cref="NativeReadResult"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <(NativeReadResult left, NativeReadResult right) => left.value < right.value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="NativeReadResult"/> is less than or equal to another specified <see cref="NativeReadResult"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <=(NativeReadResult left, NativeReadResult right) => left.value <= right.value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="NativeReadResult"/> is greater than another specified <see cref="NativeReadResult"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >(NativeReadResult left, NativeReadResult right) => left.value > right.value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="NativeReadResult"/> is greater than or equal to another specified <see cref="NativeReadResult"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >=(NativeReadResult left, NativeReadResult right) => left.value >= right.value;

        /// <summary>
        /// Adds specified <see cref="NativeReadResult"/> value and <see cref="nint"/> value.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>
        /// The result of adding <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static NativeReadResult operator +(NativeReadResult left, nint right) => new(left.Length + right);

        /// <summary>
        /// Multiplies specified <see cref="NativeReadResult"/> value and <see cref="nint"/> value.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static NativeReadResult operator *(NativeReadResult left, nint right) => left.HasData ? left.Length * right : left;

        /// <summary>
        /// Divides specified <see cref="NativeReadResult"/> value with <see cref="nint"/> value.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static NativeReadResult operator /(NativeReadResult left, nint right) => left.HasData ? left.Length / right : left;

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private string GetDebuggerDisplay() => $"{nameof(Length)}: {Length}, {nameof(IsEndOfStream)}: {IsEndOfStream}, {nameof(HasData)}: {HasData}";

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// The fully qualified type name.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override string? ToString() => GetDebuggerDisplay();
    }
}
