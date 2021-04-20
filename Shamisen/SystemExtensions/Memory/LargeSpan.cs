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
    public unsafe readonly ref struct LargeSpan        <T> where T: unmanaged
    {
        readonly IntPtr headPointer;
        readonly IntPtr length;
        private ref T Head
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => ref Unsafe.AsRef<T>(headPointer.ToPointer());
        }

        /// <summary>
        /// The length of this <see cref="LargeSpan{T}"/>.
        /// </summary>
        public IntPtr Length => length;

        /// <summary>
        /// Returns the <paramref name="index"/>th element of this <see cref="LargeSpan{T}"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ref T this[IntPtr index]
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => ref Unsafe.Add(ref Head, index);
        }
        ///
        public override bool Equals(object? obj) => false;
        public override int GetHashCode() => HashCode.Combine(headPointer, length);

        public static bool operator ==(LargeSpan<T> left, LargeSpan<T> right) => left.length == right.length && left.headPointer == right.headPointer;
        public static bool operator !=(LargeSpan<T> left, LargeSpan<T> right) => !(left == right);
    }
}
