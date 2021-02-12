using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    /// <summary>
    /// Helps throwing exceptions while in-lining aggressively.
    /// </summary>
    internal static class ThrowHelper
    {
        /// <summary>
        /// Throws the specified exception.
        /// </summary>
        /// <typeparam name="T">The type of exception.</typeparam>
        /// <param name="exception">The exception.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Throw<T>(this T exception) where T : Exception => throw exception;
    }
}
