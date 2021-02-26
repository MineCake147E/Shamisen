using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Diagnostics;

namespace Shamisen
{
    /// <summary>
    /// Represents a result of <see cref="IReadableAudioSource{TSample, TFormat}.Read(Span{TSample})"/> operation.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct ReadResult : IEquatable<ReadResult>, IComparable<ReadResult>
    {
        [FieldOffset(0)]
        private readonly int value;

        /// <summary>
        /// Represents the fact that the source stream is empty right now.
        /// </summary>
        public static ReadResult EndOfStream
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new ReadResult(int.MinValue);
        }

        /// <summary>
        /// Represents the fact that the source stream is waiting for source's IO.
        /// </summary>
        public static ReadResult WaitingForSource
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new ReadResult(0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadResult"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadResult(int value)
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
            get => value == int.MinValue;
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
        public int Length
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => MathI.Rectify(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="ReadResult"/> to <see cref="uint"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator uint(ReadResult value) => (uint)value.Length;

        /// <summary>
        /// Performs an explicit conversion from <see cref="ReadResult"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator int(ReadResult value) => value.Length;

        /// <summary>
        /// Performs an implicit conversion from <see cref="int"/> to <see cref="ReadResult"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator ReadResult(int value) => new ReadResult(value);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override bool Equals(object? obj) => obj is ReadResult result && Equals(result);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool Equals(ReadResult other) => value == other.value;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override int GetHashCode() => value.GetHashCode();

        /// <summary>
        /// Compares this instance to a specified <see cref="ReadResult"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">An <see cref="ReadResult"/>  to compare.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public int CompareTo(ReadResult other) => value.CompareTo(other.value);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ReadResult"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ReadResult"/> to compare.</param>
        /// <param name="right">The second <see cref="ReadResult"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(ReadResult left, ReadResult right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ReadResult"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ReadResult"/> to compare.</param>
        /// <param name="right">The second  <see cref="ReadResult"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(ReadResult left, ReadResult right) => !(left == right);

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="ReadResult"/> is less than another specified <see cref="ReadResult"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <(ReadResult left, ReadResult right) => left.value < right.value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="ReadResult"/> is less than or equal to another specified <see cref="ReadResult"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator <=(ReadResult left, ReadResult right) => left.value <= right.value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="ReadResult"/> is greater than another specified <see cref="ReadResult"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >(ReadResult left, ReadResult right) => left.value > right.value;

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="ReadResult"/> is greater than or equal to another specified <see cref="ReadResult"/>.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator >=(ReadResult left, ReadResult right) => left.value >= right.value;

        /// <summary>
        /// Adds specified <see cref="ReadResult"/> value and <see cref="Int32"/> value.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>
        /// The result of adding <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ReadResult operator +(ReadResult left, int right) => new ReadResult(left.Length + right);

        private string GetDebuggerDisplay() => $"{nameof(Length)}: {Length}, {nameof(IsEndOfStream)}: {IsEndOfStream}, {nameof(HasData)}: {HasData}";

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// The fully qualified type name.
        /// </returns>
        public override string? ToString() => GetDebuggerDisplay();
    }
}
