using System;

namespace MonoAudio.MathUtils
{
    /// <summary>
    /// Helps some calculations.
    /// </summary>
    public static class MathHelper
    {
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
        /// Calculates a greatest common divisor for <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static int Gcd(int a, int b)
        {
            if (a == b) return a;
            if (a > b) return GcdInternal(a, b);
            return GcdInternal(b, a);
        }

        private static int GcdInternal(int m, int n)
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
