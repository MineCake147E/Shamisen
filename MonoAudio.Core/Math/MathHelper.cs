using System;
namespace MonoAudio.Math
{
    /// <summary>
    /// Helps some calculations.
    /// </summary>
    public static class MathHelper
    {

        public static (int mul, int div) SanitizeFraction(int mul, int div)
        {
            if (div == 0) throw new ArgumentOutOfRangeException(nameof(div), $"{nameof(div)} must not be 0!");
            if (mul == div) return (1, 1);
            var gcd = Gcd(mul, div);
            return (mul / gcd, div / gcd);
        }

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
