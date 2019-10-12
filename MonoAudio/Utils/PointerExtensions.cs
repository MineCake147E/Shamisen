using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    /// <summary>
    /// Provides some extensions for <code>ref T</code>s,  <code>void*</code>s, and <see cref="IntPtr"/>s.
    /// </summary>
    public static class PointerExtensions
    {
        /// <summary>
        /// Increments the specified <see cref="IntPtr"/> value.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <returns>The incremented <see cref="IntPtr"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Increment(this IntPtr value) => IntPtr.Add(value, 1);

        /// <summary>
        /// Increments the specified <see cref="UIntPtr"/> value.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <returns>The incremented <see cref="UIntPtr"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UIntPtr Increment(this UIntPtr value) => UIntPtr.Add(value, 1);
    }
}
