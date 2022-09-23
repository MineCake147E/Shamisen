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
        /// Determines whether the <paramref name="value"/> is in specified <paramref name="range"/>.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
        /// <param name="range">The range to test.</param>
        /// <param name="value">The address to test.</param>
        /// <returns>The value which indicates whether the <paramref name="value"/> is in specified <paramref name="range"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsAddressInRange<T>(NativeSpan<T> range, ref T value)
        {
            unsafe
            {
                var b = (nuint)Unsafe.ByteOffset(ref range.Head, ref value);
                return b < (nuint)range.Length * (nuint)Unsafe.SizeOf<T>();
            }
        }
        /// <summary>
        /// Determines whether the <paramref name="value"/> is in specified region starts from <paramref name="start"/> with specified <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="value">The address to test.</param>
        /// <returns>The value which indicates whether the <paramref name="value"/> is in specified region.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool IsAddressInRange<T>(ref T start, nuint length, ref T value)
        {
            unsafe
            {
                var b = (nuint)Unsafe.ByteOffset(ref start, ref value);
                return b < length * (nuint)Unsafe.SizeOf<T>();
            }
        }
    }
}
