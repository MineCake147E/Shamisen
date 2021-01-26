using System;

namespace MonoAudio.Mathematics
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
        public static int Gcd(int a, int b) => a == b ? a : a > b ? GcdInternalI32(a, b) : GcdInternalI32(b, a);

        private static int GcdInternalI32(int m, int n)
        {
            while (n != 0)
            {
                var k = n;
                n = m % n;
                m = k;
            }
            return m;
        }

        /// <summary>
        /// Calculates a greatest common divisor for <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static ulong Gcd(ulong a, ulong b) => a == b ? a : a > b ? GcdInternalU64(a, b) : GcdInternalU64(b, a);

        private static ulong GcdInternalU64(ulong m, ulong n)
        {
            while (n != 0)
            {
                var k = n;
                n = m % n;
                m = k;
            }
            return m;
        }
    }
}
