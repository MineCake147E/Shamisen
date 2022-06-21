#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Provides constants and static methods for trigonometric, logarithmic, and other common mathematical functions.
    /// </summary>
    public static class MathF
    {
        /// <inheritdoc cref="Math.PI"/>
        public const float PI = (float)Math.PI;

        /// <inheritdoc cref="Math.E"/>
        public const float E = (float)Math.E;

        /// <summary>
        /// Calculates the integral part of a specified single-precision floating-point number.
        /// </summary>
        /// <param name="d">A number to truncate.</param>
        /// <returns>
        /// The integral part of <paramref name="d"/>; that is, the number that remains after any fractional digits have been discarded, or one of the values listed in the following table.<br/>
        ///    <paramref name="d"/> – Return value<br/>
        ///    <see cref="float.NaN"/> –<see cref="float.NaN"/><br/>
        ///    <see cref="float.NegativeInfinity"/> –<see cref="float.NegativeInfinity"/><br/>
        ///    <see cref="float.PositiveInfinity"/> –<see cref="float.PositiveInfinity"/><br/>
        /// </returns>
        public static float Truncate(float d) => (float)Math.Truncate(d);

        /// <inheritdoc cref="Math.Sin(double)"/>
        public static float Sin(float a) => (float)Math.Sin(a);

        /// <inheritdoc cref="Math.Cos(double)"/>
        public static float Cos(float a) => (float)Math.Cos(a);

        /// <inheritdoc cref="Math.Floor(double)"/>
        public static float Floor(float d) => (float)Math.Floor(d);
    }
}

#endif