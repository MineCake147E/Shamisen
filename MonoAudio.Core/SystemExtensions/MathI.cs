using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    /// <summary>
    /// Provides constants and static methods for bitwise, arithmetic, and other common mathematical functions.
    /// </summary>
    public static partial class MathI
    {
        /// <summary>
        /// Aligns the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorStep(int value, int step) => value - (value % step);
    }
}
