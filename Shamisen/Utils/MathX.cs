using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using DSUtils = DivideSharp.Utils;

namespace Shamisen
{
    /// <summary>
    /// Contains some mathematical functions for Fixed-Point numbers.
    /// </summary>
    public static class MathX
    {
        private static readonly ReadOnlyMemory<Complex> PowerRootsOfUnity = new Complex[]
        {
            new Complex(0, 1),
            new Complex(0.7071067811865476, 0.7071067811865476),
            new Complex(0.9238795325112867, 0.3826834323650898),
            new Complex(0.9807852804032304, 0.19509032201612828),
            new Complex(0.9951847266721969, 0.0980171403295606),
            new Complex(0.9987954562051724, 0.049067674327418015),
            new Complex(0.9996988186962042, 0.024541228522912288),
            new Complex(0.9999247018391445, 0.012271538285719925),
            new Complex(0.9999811752826011, 0.006135884649154475),
            new Complex(0.9999952938095762, 0.003067956762965976),
            new Complex(0.9999988234517019, 0.0015339801862847657),
            new Complex(0.9999997058628822, 0.0007669903187427045),
            new Complex(0.9999999264657179, 0.00038349518757139556),
            new Complex(0.9999999816164293, 0.00019174759731070332),
            new Complex(0.9999999954041073, 9.587379909597734E-05),
            new Complex(0.9999999988510269, 4.793689960306688E-05),
            new Complex(0.9999999997127567, 2.396844980841822E-05),
            new Complex(0.9999999999281892, 1.1984224905069707E-05),
            new Complex(0.9999999999820472, 5.9921124526424275E-06),
            new Complex(0.9999999999955118, 2.996056226334661E-06),
            new Complex(0.999999999998878, 1.4980281131690111E-06),
            new Complex(0.9999999999997194, 7.490140565847157E-07),
            new Complex(0.9999999999999298, 3.7450702829238413E-07),
            new Complex(0.9999999999999825, 1.8725351414619535E-07),
            new Complex(0.9999999999999957, 9.362675707309808E-08),
            new Complex(0.9999999999999989, 4.6813378536549095E-08),
            new Complex(0.9999999999999998, 2.3406689268274554E-08),
            new Complex(0.9999999999999999, 1.1703344634137277E-08),
            new Complex(1, 5.8516723170686385E-09),
            new Complex(1, 2.9258361585343192E-09),
            new Complex(1, 1.4629180792671596E-09),
            new Complex(1, 7.314590396335798E-10),
            new Complex(1, 3.657295198167899E-10),
            new Complex(1, 1.8286475990839495E-10),
            new Complex(1, 9.143237995419748E-11),
            new Complex(1, 4.571618997709874E-11),
            new Complex(1, 2.285809498854937E-11),
            new Complex(1, 1.1429047494274685E-11),
            new Complex(1, 5.714523747137342E-12),
            new Complex(1, 2.857261873568671E-12),
            new Complex(1, 1.4286309367843356E-12),
            new Complex(1, 7.143154683921678E-13),
            new Complex(1, 3.571577341960839E-13),
            new Complex(1, 1.7857886709804195E-13),
            new Complex(1, 8.928943354902097E-14),
            new Complex(1, 4.4644716774510487E-14),
            new Complex(1, 2.2322358387255243E-14),
            new Complex(1, 1.1161179193627622E-14),
            new Complex(1, 5.580589596813811E-15),
            new Complex(1, 2.7902947984069054E-15),
            new Complex(1, 1.3951473992034527E-15),
            new Complex(1, 6.975736996017264E-16),
            new Complex(1, 3.487868498008632E-16),
            new Complex(1, 1.743934249004316E-16),
            new Complex(1, 8.71967124502158E-17),
            new Complex(1, 4.35983562251079E-17),
            new Complex(1, 2.179917811255395E-17),
            new Complex(1, 1.0899589056276974E-17),
            new Complex(1, 5.449794528138487E-18),
            new Complex(1, 2.7248972640692436E-18),
            new Complex(1, 1.3624486320346218E-18),
            new Complex(1, 6.812243160173109E-19),
            new Complex(1, 3.4061215800865545E-19),
            new Complex(1, 1.7030607900432772E-19)
        };

        /// <summary>
        /// Returns the <see cref="Complex"/> value with specified angle.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mask">The precision mask.</param>
        /// <returns></returns>
        public static Complex SinCosPi(Fixed64 value, ulong mask = ~0ul)
        {
            if (value == Fixed64.MinValue)
            {
                return PowerRootsOfUnity.Span[1];
            }
            var result = Complex.One;
            var g = (DSUtils.Abs(value.Value) << 1) & mask;
            ref var prouHead = ref MemoryMarshal.GetReference(PowerRootsOfUnity.Span);
            while (g > 0)
            {
                var index = MathI.LeadingZeroCount(g);
                g ^= 0x8000_0000_0000_0000u >> index;
                result *= Unsafe.Add(ref prouHead, index);
            }
            return value < Fixed64.Zero ? Complex.Conjugate(result) : result;
        }

        /// <summary>
        /// Calculates the <see cref="Math.Sin(double)"/> of the <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static double Sin(Fixed64 value)
        {
            if (value < Fixed64.Zero) return -Sin(value);   //Wrap negative numbers
            if ((long)value > 0x3fff_ffff_ffff_ffff)    //Wrap sinusoid
            {
                value = (Fixed64)(long)(0x8000_0000_0000_0000ul - (ulong)value);    //1 - value
            }
            return SinInternal(value);
        }

        private static double SinInternal(Fixed64 value) => Math.Sin((double)value);

#if NETSTANDARD2_0
        /// <summary>
        /// Calculates the <see cref="Math.Sin(double)"/> of the <paramref name="value"/> and converts the value to <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static float SinF(Fixed64 value)
#else

        /// <summary>
        /// Calculates the <see cref="MathF.Sin(float)"/> of the <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static float SinF(Fixed64 value)
#endif
        {
            int u = (int)(value.Value >> 32);
            uint a = MathI.Abs(u);
            a = Math.Min(a, 0x8000_0000u - a);
            return Math.Sign(u) * SinFInternal32((int)a);
        }
        const float PiOverTwoToThe31stPower = (float)(-Math.PI / int.MinValue);

        internal const float C0 = 0.0f;
        internal const float C1 = 3.1415926535897932385f;
        internal const float C2 = 0.0f;
        internal const float C3 = -5.1677127800499700292f;
        internal const float C4 = 0.0f;
        internal const float C5 = 2.5496493437571871309f;
        internal const float C6 = 0.0049683640241416525772f;
        internal const float C7 = -0.61803287266375526383f;
        internal const float C8 = 0.033708615781302465869f;
        internal const float C9 = 0.056476450995301634261f;
        private static float SinFInternal32(int value)
        {
            unchecked
            {
                const float OneOverTwoToThe31stPower = -1.0f / int.MinValue;
                float x = OneOverTwoToThe31stPower * value;
                float x2 = x * x;
                float res = C9;
                res = res * x + C8;
                res = res * x + C7;
                res = res * x + C6;
                res = res * x + C5;
                res = res * x2 + C3;
                res = res * x2 + C1;
                return res * x;
            }
        }
        private static float SinFInternal(Fixed64 value)
        {
            unchecked
            {
#if NETSTANDARD2_0
                return (float)Math.Sin(Math.PI * (double)value);
#else
                return MathF.Sin(MathF.PI * (float)(double)value);
#endif
            }
        }
    }
}
