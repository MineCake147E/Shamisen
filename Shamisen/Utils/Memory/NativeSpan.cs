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
    public readonly unsafe ref struct NativeSpan<T> where T : unmanaged
    {
        private readonly nint headPointer;
        private readonly nint length;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeSpan{T}"/> struct.
        /// </summary>
        /// <param name="headPointer">The head pointer.</param>
        /// <param name="length">The length.</param>
        public NativeSpan(nint headPointer, nint length)
        {
            this.headPointer = headPointer;
            this.length = length;
        }

        private ref T Head
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => ref Unsafe.AsRef<T>(((IntPtr)headPointer).ToPointer());
        }

        /// <summary>
        /// The length of this <see cref="NativeSpan{T}"/>.
        /// </summary>
        public nint Length => length;

        /// <summary>
        /// Returns the <paramref name="index"/>th element of this <see cref="NativeSpan{T}"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ref T this[nint index]
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => ref Unsafe.Add(ref Head, index);
        }

        /// <summary>
        /// Slices the <see cref="NativeSpan{T}"/> with the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        public NativeSpan<T> Slice(nint start)
            => new(headPointer + start, length - start);

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
        /// Indicates whether the values of two specified <see cref="NativeSpan{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="NativeSpan{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="NativeSpan{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(NativeSpan<T> left, NativeSpan<T> right) => left.length == right.length && left.headPointer == right.headPointer;

        /// <summary>
        /// Indicates whether the values of two specified <see cref="NativeSpan{T}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="NativeSpan{T}"/> to compare.</param>
        /// <param name="right">The second  <see cref="NativeSpan{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(NativeSpan<T> left, NativeSpan<T> right) => !(left == right);
    }
}
