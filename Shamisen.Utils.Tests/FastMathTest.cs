﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Shamisen.TestUtils;

namespace Shamisen.Utils.Tests
{
    [TestFixture]
    public class FastMathTest
    {
        #region Trigonometry
        [TestCase(MathF.PI / 2)]
        [TestCase(-MathF.PI / 2)]
        [TestCase(MathF.PI)]
        [TestCase(-MathF.PI)]
        [TestCase(0.0f)]
        [TestCase(float.Epsilon)]
        [TestCase(-float.Epsilon)]

        public void SinCalculatesCorrectly(float value)
        {
            var exp = (float)Math.Sin(value);
            var act = FastMath.Sin(value);
            Assert.That(act, Is.EqualTo(exp).Within(8.74227766E-08f));
            Console.WriteLine($"Expected: {exp}, Actual: {act}");
        }

        [Test]
        public void SinBruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(MathF.PI * 0.5f) + 1;
            var na = new NeumaierAccumulator();
            var maxerr = double.NegativeInfinity;
            ulong cnt = 0;
            for (var i = 0; i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = (float)Math.Sin(v);
                var act = FastMath.Sin(v);
                var diff = Math.Abs(exp - act);
                na += diff;
                maxerr = FastMath.Max(maxerr, diff);
                cnt++;
            }
            Console.WriteLine(na.Sum / cnt);
            Console.WriteLine(maxerr);
            Assert.Pass();
        }

        [TestCase(1.0f / 2)]
        [TestCase(-1.0f / 2)]
        [TestCase(1.0f)]
        [TestCase(-1.0f)]
        [TestCase(0.0f)]
        [TestCase(float.Epsilon)]
        [TestCase(-float.Epsilon)]

        public void SinPiCalculatesCorrectly(float value)
        {
            var exp = (float)Math.Sin(value * Math.PI);
            var act = FastMath.SinPi(value);
            Assert.That(act, Is.EqualTo(exp).Within(8.74227766E-08f));
            Console.WriteLine($"Expected: {exp}, Actual: {act}");
        }

        [Test]
        public void SinPiBruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(0.5f) + 1;
            var na = new NeumaierAccumulator();
            var maxerr = double.NegativeInfinity;
            ulong cnt = 0;
            for (var i = 0; i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = (float)Math.Sin(v * Math.PI);
                var act = FastMath.SinPi(v);
                var diff = Math.Abs(exp - act);
                na += diff;
                maxerr = FastMath.Max(maxerr, diff);
                cnt++;
            }
            Console.WriteLine(na.Sum / cnt);
            Console.WriteLine(maxerr);
            Assert.Pass();
        }

        [TestCase(MathF.PI / 2)]
        [TestCase(-MathF.PI / 2)]
        [TestCase(MathF.PI)]
        [TestCase(-MathF.PI)]
        [TestCase(0.0f)]
        [TestCase(float.Epsilon)]
        [TestCase(-float.Epsilon)]

        public void FastSinCalculatesCorrectly(float value)
        {
            var exp = MathF.Sin(value);
            var act = FastMath.FastSin(value);
            Assert.That(act, Is.EqualTo(exp).Within(8.74227766E-08f));
            Console.WriteLine($"Expected: {exp}, Actual: {act}");
        }

        [Test]
        public void FastSinBruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(MathF.PI * 0.5f) + 1;
            var na = new NeumaierAccumulator();
            var maxerr = double.NegativeInfinity;
            ulong cnt = 0;
            for (var i = 0; i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = MathF.Sin(v);
                var act = FastMath.FastSin(v);
                var diff = Math.Abs(exp - act);
                na += diff;
                maxerr = FastMath.Max(maxerr, diff);
                cnt++;
            }
            Console.WriteLine(na.Sum / cnt);
            Console.WriteLine(maxerr);
            Assert.Pass();
        }
        [TestCase(MathF.PI / 2)]
        [TestCase(-MathF.PI / 2)]
        [TestCase(MathF.PI)]
        [TestCase(-MathF.PI)]
        [TestCase(0.0f)]
        [TestCase(float.Epsilon)]
        [TestCase(-float.Epsilon)]

        public void CosCalculatesCorrectly(float value)
        {
            var exp = MathF.Cos(value);
            var act = FastMath.Cos(value);
            Assert.That(act, Is.EqualTo(exp).Within(8.74227766E-08f));
            Console.WriteLine($"Expected: {exp}, Actual: {act}");
        }

        [Test]
        public void CosBruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(MathF.PI * 0.5f) + 1;
            var na = new NeumaierAccumulator();
            var maxerr = double.NegativeInfinity;
            ulong cnt = 0;
            for (var i = 0; i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = MathF.Cos(v);
                var act = FastMath.Cos(v);
                var diff = Math.Abs(exp - act);
                na += diff;
                maxerr = FastMath.Max(maxerr, diff);
                cnt++;
            }
            Console.WriteLine(na.Sum / cnt);
            Console.WriteLine(maxerr);
            Assert.Pass();
        }

        [TestCase(MathF.PI / 2)]
        [TestCase(-MathF.PI / 2)]
        [TestCase(MathF.PI)]
        [TestCase(-MathF.PI)]
        [TestCase(0.0f)]
        [TestCase(float.Epsilon)]
        [TestCase(-float.Epsilon)]

        public void FastCosCalculatesCorrectly(float value)
        {
            var exp = MathF.Cos(value);
            var act = FastMath.FastCos(value);
            Assert.That(act, Is.EqualTo(exp).Within(8.74227766E-08f));
            Console.WriteLine($"Expected: {exp}, Actual: {act}");
        }

        [Test]
        public void FastCosBruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(MathF.PI * 0.5f) + 1;
            var na = new NeumaierAccumulator();
            var maxerr = double.NegativeInfinity;
            ulong cnt = 0;
            for (var i = 0; i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = MathF.Cos(v);
                var act = FastMath.FastCos(v);
                var diff = Math.Abs(exp - act);
                na += diff;
                maxerr = FastMath.Max(maxerr, diff);
                cnt++;
            }
            Console.WriteLine(na.Sum / cnt);
            Console.WriteLine(maxerr);
            Assert.Pass();
        }
        #endregion

        #region Exponential
        [TestCase(0.5f)]
        [TestCase(-0.5f)]
        [TestCase(1.0f)]
        [TestCase(-1.0f)]
        [TestCase(0.0f)]
        [TestCase(MathF.PI)]
        [TestCase(-MathF.PI)]

        public void Exp2CalculatesCorrectly(float value)
        {
            var exp = Math.Pow(2.0, value);
            var act = FastMath.Exp2(value);
            Console.WriteLine($"Expected:   {exp}\t({(float)exp}f)\nActual: \t{(double)act}\t({act}f)\nOff by {exp - act}({(float)exp - act}f)");
            Assert.That(BitConverter.SingleToUInt32Bits(act), Is.EqualTo(BitConverter.SingleToUInt32Bits((float)exp)).Within(1));
        }

        [Test]
        public void Exp2BruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(2.0f) + 1;
            var na = 0ul;
            var errPerUlp = new uint[16];
            var maxerr = 0ul;
            var lastmax = 0;
            ulong cnt = 0;
            for (var i = BitConverter.SingleToInt32Bits(1.0f); i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = (float)Math.Pow(2.0, v);
                var act = FastMath.Exp2(v);
                var diff = MathI.Abs(BitConverter.SingleToInt32Bits(exp) - BitConverter.SingleToInt32Bits(act));
                na += diff;
                errPerUlp[MathI.Min(errPerUlp.Length - 1, diff)]++;
                if (maxerr < diff)
                {
                    maxerr = diff;
                    lastmax = i;
                }
                cnt++;
            }
            var inv = 1.0 / cnt;
            Console.WriteLine($"Total errors: {na} ulp / {cnt} values ({na * inv:P})");
            var errt = errPerUlp.AsSpan().SliceWhileIfLongerThan(maxerr + 1);
            for (var i = 0; i < errt.Length; i++)
            {
                var err = errt[i];
                Console.WriteLine($"Values with {i}ulp error: {err} values ({err * inv:P})");
            }
            Console.WriteLine($"Maximum Error: {maxerr} ulp");
            Console.WriteLine($"at {lastmax} ({BitConverter.Int32BitsToSingle(lastmax)})");
            Assert.Pass();
        }

        [TestCase(0.5f)]
        [TestCase(-0.5f)]
        [TestCase(1.0f)]
        [TestCase(-1.0f)]
        [TestCase(0.0f)]
        [TestCase(MathF.PI)]
        [TestCase(-MathF.PI)]

        public void FastExp2CalculatesCorrectly(float value)
        {
            var exp = Math.Pow(2.0, value);
            var act = FastMath.FastExp2(value);
            Console.WriteLine($"Expected:   {exp}\t({(float)exp}f)\nActual: \t{(double)act}\t({act}f)\nOff by {exp - act}({(float)exp - act}f)");
            Assert.That(BitConverter.SingleToUInt32Bits(act), Is.EqualTo(BitConverter.SingleToUInt32Bits((float)exp)).Within(1));
        }

        [Test]
        public void FastExp2BruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(2.0f) + 1;
            var na = 0ul;
            var errPerUlp = new uint[16];
            var maxerr = 0ul;
            var lastmax = 0;
            ulong cnt = 0;
            for (var i = BitConverter.SingleToInt32Bits(1.0f); i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = (float)Math.Pow(2.0, v);
                var act = FastMath.FastExp2(v);
                var diff = MathI.Abs(BitConverter.SingleToInt32Bits(exp) - BitConverter.SingleToInt32Bits(act));
                na += diff;
                errPerUlp[MathI.Min(errPerUlp.Length - 1, diff)]++;
                if (maxerr < diff)
                {
                    maxerr = diff;
                    lastmax = i;
                }
                cnt++;
            }
            var inv = 1.0 / cnt;
            Console.WriteLine($"Total errors: {na} ulp / {cnt} values ({na * inv:P})");
            var errt = errPerUlp.AsSpan().SliceWhileIfLongerThan(maxerr + 1);
            for (var i = 0; i < errt.Length; i++)
            {
                var err = errt[i];
                Console.WriteLine($"Values with {i}ulp error: {err} values ({err * inv:P})");
            }
            Console.WriteLine($"Maximum Error: {maxerr} ulp");
            Console.WriteLine($"at {lastmax} ({BitConverter.Int32BitsToSingle(lastmax)})");
            Assert.Pass();
        }
        #endregion

        #region Logarithm

        [TestCase(0.5f)]
        [TestCase(-0.5f)]
        [TestCase(1.0f)]
        [TestCase(1.1754944E-38f)]
        [TestCase(MathF.PI)]

        public void Log2AsNormalCalculatesCorrectly(float value)
        {
            var exp = Math.Log2(MathF.Abs(value));
            var act = FastMath.Log2AsNormal(value);
            Console.WriteLine($"Expected:   {exp}\t({(float)exp}f)\nActual: \t{(double)act}\t({act}f)\nOff by {exp - act}\t({(float)exp - act}f)");
            Assert.That(BitConverter.SingleToUInt32Bits(act), Is.EqualTo(BitConverter.SingleToUInt32Bits((float)exp)).Within(1));
        }

        [Test]
        public void Log2AsNormalBruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(2.0f) + 1;
            var na = 0ul;
            var errPerUlp = new uint[16];
            var maxerr = 0ul;
            var lastmax = 0;
            ulong cnt = 0;
            for (var i = BitConverter.SingleToInt32Bits(1.0f); i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = (float)Math.Log2(v);  //MathF.Log2 is quite inaccurate if v were normal, while it's more than 4x slower than FastMath.Log2AsNormal.
                var act = FastMath.Log2AsNormal(v);     //If you want to verify the statement above, just replace FastMath.Log2AsNormal with MathF.Log2, and see what happens.
                var diff = MathI.Abs(BitConverter.SingleToInt32Bits(exp) - BitConverter.SingleToInt32Bits(act));
                na += diff;
                errPerUlp[MathI.Min(errPerUlp.Length - 1, diff)]++;
                if (maxerr < diff)
                {
                    maxerr = diff;
                    lastmax = i;
                }
                cnt++;
            }
            var inv = 1.0 / cnt;
            Console.WriteLine($"Total errors: {na} ulp / {cnt} values ({na * inv:P})");
            var errt = errPerUlp.AsSpan().SliceWhileIfLongerThan(maxerr + 1);
            for (var i = 0; i < errt.Length; i++)
            {
                var err = errt[i];
                Console.WriteLine($"Values with {i}ulp error: {err} values ({err * inv:P})");
            }
            Console.WriteLine($"Maximum Error: {maxerr} ulp");
            Console.WriteLine($"at {lastmax} ({BitConverter.Int32BitsToSingle(lastmax)})");
            Assert.Pass();
        }

        [TestCase(0.5f)]
        [TestCase(-0.5f)]
        [TestCase(1.0f)]
        [TestCase(1.1754944E-38f)]
        [TestCase(7.83662853838e-39f)]
        [TestCase(MathF.PI)]
        [TestCase(0.0f)]

        public void Log2CalculatesCorrectly(float value)
        {
            var exp = Math.Log2(MathF.Abs(value));
            var act = FastMath.Log2(value);
            Console.WriteLine($"Expected:   {exp}\t({(float)exp}f)\nActual: \t{(double)act}\t({act}f)\nOff by {exp - act}\t({(float)exp - act}f)");
            Assert.That(BitConverter.SingleToUInt32Bits(act), Is.EqualTo(BitConverter.SingleToUInt32Bits((float)exp)).Within(1));
        }

        [Test]
        public void Log2BruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(2.35098870164e-38f) + 1;
            var na = 0ul;
            var errPerUlp = new uint[16];
            var maxerr = 0ul;
            var lastmax = 0;
            ulong cnt = 0;
            for (var i = 0; i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = (float)Math.Log2(v);  //MathF.Log2 is quite inaccurate, while it's more than 2x slower than FastMath.Log2.
                var act = FastMath.Log2(v);     //If you want to verify the statement above, just replace FastMath.Log2 with MathF.Log2, and see what happens.
                var diff = MathI.Abs(BitConverter.SingleToInt32Bits(exp) - BitConverter.SingleToInt32Bits(act));
                na += diff;
                errPerUlp[MathI.Min(errPerUlp.Length - 1, diff)]++;
                if (maxerr < diff)
                {
                    maxerr = diff;
                    lastmax = i;
                }
                cnt++;
            }
            var inv = 1.0 / cnt;
            Console.WriteLine($"Total errors: {na} ulp / {cnt} values ({na * inv:P})");
            var errt = errPerUlp.AsSpan().SliceWhileIfLongerThan(maxerr + 1);
            for (var i = 0; i < errt.Length; i++)
            {
                var err = errt[i];
                Console.WriteLine($"Values with {i}ulp error: {err} values ({err * inv:P})");
            }
            Console.WriteLine($"Maximum Error: {maxerr} ulp");
            Console.WriteLine($"at {lastmax} ({BitConverter.Int32BitsToSingle(lastmax)})");
            Assert.Pass();
        }

        [TestCase(0.5f)]
        [TestCase(-0.5f)]
        [TestCase(1.0f)]
        [TestCase(1.1754944E-38f)]
        [TestCase(MathF.PI)]

        public void FastLog2AsNormalCalculatesCorrectly(float value)
        {
            var exp = Math.Log2(MathF.Abs(value));
            var act = FastMath.FastLog2AsNormal(value);
            Console.WriteLine($"Expected:   {exp}\t({(float)exp}f)\nActual: \t{(double)act}\t({act}f)\nOff by {exp - act}\t({(float)exp - act}f)");
            Assert.That(BitConverter.SingleToUInt32Bits(act), Is.EqualTo(BitConverter.SingleToUInt32Bits((float)exp)).Within(1));
        }

        [Test]
        public void FastLog2AsNormalBruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(2.0f) + 1;
            var na = 0ul;
            var errPerUlp = new uint[16];
            var maxerr = 0ul;
            var lastmax = 0;
            ulong cnt = 0;
            for (var i = BitConverter.SingleToInt32Bits(1.0f); i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = (float)Math.Log2(v);          //MathF.Log2 is quite inaccurate if v were normal, while it's more than 6x slower than FastMath.FastLog2AsNormal.
                var act = FastMath.FastLog2AsNormal(v); //If you want to verify the statement above, just replace FastMath.FastLog2AsNormal with MathF.Log2, and see what happens.
                var diff = MathI.Abs(BitConverter.SingleToInt32Bits(exp) - BitConverter.SingleToInt32Bits(act));
                na += diff;
                errPerUlp[MathI.Min(errPerUlp.Length - 1, diff)]++;
                if (maxerr < diff)
                {
                    maxerr = diff;
                    lastmax = i;
                }
                cnt++;
            }
            var inv = 1.0 / cnt;
            Console.WriteLine($"Total errors: {na} ulp / {cnt} values ({na * inv:P})");
            var errt = errPerUlp.AsSpan().SliceWhileIfLongerThan(maxerr + 1);
            for (var i = 0; i < errt.Length; i++)
            {
                var err = errt[i];
                Console.WriteLine($"Values with {i}ulp error: {err} values ({err * inv:P})");
            }
            Console.WriteLine($"Maximum Error: {maxerr} ulp");
            Console.WriteLine($"at {lastmax} ({BitConverter.Int32BitsToSingle(lastmax)})");
            Assert.Pass();
        }

        [TestCase(0.5f)]
        [TestCase(-0.5f)]
        [TestCase(1.0f)]
        [TestCase(1.1754944E-38f)]
        [TestCase(7.83662853838e-39f)]
        [TestCase(MathF.PI)]
        [TestCase(0.0f)]

        public void FastLog2CalculatesCorrectly(float value)
        {
            var exp = Math.Log2(MathF.Abs(value));
            var act = FastMath.FastLog2(value);
            Console.WriteLine($"Expected:   {exp}\t({(float)exp}f)\nActual: \t{(double)act}\t({act}f)\nOff by {exp - act}\t({(float)exp - act}f)");
            Assert.That(BitConverter.SingleToUInt32Bits(act), Is.EqualTo(BitConverter.SingleToUInt32Bits((float)exp)).Within(1));
        }

        [Test]
        public void FastLog2BruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(2.35098870164e-38f) + 1;
            var na = 0ul;
            var errPerUlp = new uint[16];
            var maxerr = 0ul;
            var lastmax = 0;
            ulong cnt = 0;
            for (var i = 0; i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = (float)Math.Log2(v);  //MathF.Log2 is quite inaccurate, while it's more than 4x slower than FastMath.FastLog2.
                var act = FastMath.FastLog2(v); //If you want to verify the statement above, just replace FastMath.FastLog2 with MathF.Log2, and see what happens.
                var diff = MathI.Abs(BitConverter.SingleToInt32Bits(exp) - BitConverter.SingleToInt32Bits(act));
                na += diff;
                errPerUlp[MathI.Min(errPerUlp.Length - 1, diff)]++;
                if (maxerr < diff)
                {
                    maxerr = diff;
                    lastmax = i;
                }
                cnt++;
            }
            var inv = 1.0 / cnt;
            Console.WriteLine($"Total errors: {na} ulp / {cnt} values ({na * inv:P})");
            var errt = errPerUlp.AsSpan().SliceWhileIfLongerThan(maxerr + 1);
            for (var i = 0; i < errt.Length; i++)
            {
                var err = errt[i];
                Console.WriteLine($"Values with {i}ulp error: {err} values ({err * inv:P})");
            }
            Console.WriteLine($"Maximum Error: {maxerr} ulp");
            Console.WriteLine($"at {lastmax} ({BitConverter.Int32BitsToSingle(lastmax)})");
            Assert.Pass();
        }
        #endregion
    }
}
