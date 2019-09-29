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

        /// <summary>
        /// Aligns the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int newLength, int remainder) FloorStepRem(int value, int step)
        {
            var m = value % step;
            return (value - m, m);
        }

        /// <summary>
        /// Returns the largest power-of-two number less than or equals to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static uint LargestPowerOfTwoLessThanOrEqualsTo(uint value)
        {
            uint result = 1;
            if ((value & 0xFFFF_0000u) > 0)
            {
                result <<= 16;
                value >>= 16;
            }
            if ((value & 0xFF00u) > 0)
            {
                result <<= 8;
                value >>= 8;
            }
            if ((value & 0b11110000u) > 0)
            {
                result <<= 4;
                value >>= 4;
            }
            if ((value & 0b1100u) > 0)
            {
                result <<= 2;
                value >>= 2;
            }
            if ((value & 0b10u) > 0)
            {
                result <<= 1;
            }
            return result;
        }
    }
}
