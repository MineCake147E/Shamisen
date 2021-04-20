using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public static ref T Add<T>(ref T p0, IntPtr p1, IntPtr p2)
            => ref Unsafe.Add(ref Unsafe.Add(ref p0, p1), p2);
    }
}
