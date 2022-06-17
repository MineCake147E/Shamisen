using System;
using System.Numerics;

namespace Shamisen.Mathematics
{
    /// <summary>
    /// Helps some calculations.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// The double pi.
        /// </summary>
        public const double DoublePI = 6.2831853071795864769252867665590057683943387987502116419498891846;

        /// <summary>
        /// Minimizes the divisor.
        /// </summary>
        /// <param name="mul">The multiplier.</param>
        /// <param name="div">The divisor.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">div</exception>
        public static (int mul, int div) MinimizeDivisor(int mul, int div)
        {
            if (div == 0) throw new ArgumentOutOfRangeException(nameof(div), $"{nameof(div)} must not be 0!");
            if (mul == div) return (1, 1);
            var gcd = Gcd(mul, div);
            return (mul / gcd, div / gcd);
        }

        /// <summary>
        /// Minimizes the divisor.
        /// </summary>
        /// <param name="mul">The multiplier.</param>
        /// <param name="div">The divisor.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">div</exception>
        public static (ulong mul, ulong div) MinimizeDivisor(ulong mul, ulong div)
        {
            if (div == 0) throw new ArgumentOutOfRangeException(nameof(div), $"{nameof(div)} must not be 0!");
            if (mul == div) return (1, 1);
            var gcd = Gcd(mul, div);
            return (mul / gcd, div / gcd);
        }

        /// <summary>
        /// Calculates a greatest common divisor for <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static int Gcd(int a, int b) => GcdInternalI32(a, b);

        private static int GcdInternalI32(int m, int n)
        {
            return (int)Gcd((uint)m, (uint)n);
        }

        /// <summary>
        /// Calculates a greatest common divisor for <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static uint Gcd(uint a, uint b)
        {
            var m = Math.Min(a, b);
            if (m == 0) return Math.Max(a, b);
            var i = MathI.TrailingZeroCount(a);
            a >>= i;
            var j = MathI.TrailingZeroCount(b);
            b >>= j;
            var k = Math.Min(i, j);
            while (true)
            {
                if (a > b)
                {
                    (b, a) = (a, b);
                }
                b -= a;
                if (b == 0)
                {
                    return a << k;
                }
                b >>= MathI.TrailingZeroCount(b);
            }
        }

        /// <summary>
        /// Calculates a greatest common divisor for <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static ulong Gcd(ulong a, ulong b)
        {
            var m = Math.Min(a, b);
            if (m == 0) return Math.Max(a, b);
            var i = MathI.TrailingZeroCount(a);
            a >>= i;
            var j = MathI.TrailingZeroCount(b);
            b >>= j;
            var k = Math.Min(i, j);
            while (true)
            {
                if (a > b)
                {
                    (b, a) = (a, b);
                }
                b -= a;
                if (b == 0)
                {
                    return a << k;
                }
                b >>= MathI.TrailingZeroCount(b);
            }
        }

    }
}
