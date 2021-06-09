using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Contains some utility functions for manipulating <c>ref</c> values.
    /// </summary>
    public static class MathR
    {
        /// <summary>
        /// Adds the p0 and p1 and p2.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ref T Add<T>(ref T p0, IntPtr p1, IntPtr p2)
            => ref Unsafe.Add(ref Unsafe.Add(ref p0, p1), p2);

        /// <summary>
        /// Adds the <paramref name="right"/> to the <paramref name="left"/>.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static IntPtr Add(IntPtr left, IntPtr right) => left + (nint)right;

        /// <summary>
        /// Subtracts the <paramref name="right"/> from the <paramref name="left"/>.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>left - right.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static IntPtr Subtract(IntPtr left, IntPtr right) => left - (nint)right;

        /// <summary>
        /// Multiplies the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static IntPtr Multiply(IntPtr left, IntPtr right) => (nint)left * right;

        /// <summary>
        /// Converts the specified <see cref="IntPtr"/> value to <c>ref <see cref="byte"/></c>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ref byte ToRefByte(IntPtr value)
        {
            unsafe
            {
                return ref Unsafe.AsRef<byte>(value.ToPointer());
            }
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static IntPtr ToIntPtr(ref byte value)
        {
            unsafe
            {
                return new(Unsafe.AsPointer(ref value));
            }
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> to pointer.
        /// </summary>
        /// <param name="value">The <see cref="IntPtr"/> getting converted.</param>
        /// <returns>
        /// The converted pointer.
        /// </returns>
        public static unsafe void* ToPointer(this nint value) => ((IntPtr)value).ToPointer();
    }
}
