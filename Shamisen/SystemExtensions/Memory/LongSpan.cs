using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.SystemExtensions.Memory
{
    /// <summary>
    /// Represents a continuous region of memory.
    /// </summary>
    /// <typeparam name="T">The type of contents.</typeparam>
    public unsafe readonly ref struct LongSpan<T> where T : unmanaged
    {
        private readonly IntPtr headPointer;
        private readonly IntPtr length;

        /// <summary>
        /// Initializes a new instance of the <see cref="LongSpan{T}"/> struct.
        /// </summary>
        /// <param name="headPointer">The head pointer.</param>
        /// <param name="length">The length.</param>
        public LongSpan(IntPtr headPointer, IntPtr length)
        {
            this.headPointer = headPointer;
            this.length = length;
        }

        private ref T Head
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => ref Unsafe.AsRef<T>(headPointer.ToPointer());
        }

        /// <summary>
        /// The length of this <see cref="LongSpan{T}"/>.
        /// </summary>
        public IntPtr Length => length;

        /// <summary>
        /// Returns the <paramref name="index"/>th element of this <see cref="LongSpan{T}"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ref T this[IntPtr index]
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => ref Unsafe.Add(ref Head, index);
        }

        /// <summary>
        /// Slices the <see cref="LongSpan{T}"/> with the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        public LongSpan<T> Slice(IntPtr start)
            => new((nint)headPointer + start, (nint)length - start);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => false;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(headPointer, length);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="LongSpan{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="LongSpan{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="LongSpan{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(LongSpan<T> left, LongSpan<T> right) => left.length == right.length && left.headPointer == right.headPointer;

        /// <summary>
        /// Indicates whether the values of two specified <see cref="LongSpan{T}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="LongSpan{T}"/> to compare.</param>
        /// <param name="right">The second  <see cref="LongSpan{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(LongSpan<T> left, LongSpan<T> right) => !(left == right);
    }
}
