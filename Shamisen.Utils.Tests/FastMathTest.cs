using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        #region Sin
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
            var res = ErrorUtils.EvaluateErrors(0.0f, MathF.PI * 0.5f,
                (a) =>
                {
                    nint i, length = a.Length;
                    ref var x9 = ref MemoryMarshal.GetReference(a);
                    var olen = length - 3;
                    for (i = 0; i < olen; i += 4)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        var s0 = x10;
                        var s1 = Unsafe.Add(ref x9, i + 1);
                        var s2 = Unsafe.Add(ref x9, i + 2);
                        var s3 = Unsafe.Add(ref x9, i + 3);
                        x10 = (float)Math.Sin(s0);
                        Unsafe.Add(ref x10, 1) = (float)Math.Sin(s1);
                        Unsafe.Add(ref x10, 2) = (float)Math.Sin(s2);
                        Unsafe.Add(ref x10, 3) = (float)Math.Sin(s3);
                    }
                    for (; i < length; i++)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        x10 = (float)Math.Sin(x10);
                    }
                },
                (a) =>
                {
                    nint i, length = a.Length;
                    ref var x9 = ref MemoryMarshal.GetReference(a);
                    var olen = length - 3;
                    for (i = 0; i < olen; i += 4)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        var s0 = x10;
                        var s1 = Unsafe.Add(ref x9, i + 1);
                        var s2 = Unsafe.Add(ref x9, i + 2);
                        var s3 = Unsafe.Add(ref x9, i + 3);
                        x10 = FastMath.Sin(s0);
                        Unsafe.Add(ref x10, 1) = FastMath.Sin(s1);
                        Unsafe.Add(ref x10, 2) = FastMath.Sin(s2);
                        Unsafe.Add(ref x10, 3) = FastMath.Sin(s3);
                    }
                    for (; i < length; i++)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        x10 = FastMath.Sin(x10);
                    }
                });
            ErrorUtils.WriteResult(res);
            var maxUlpError = res.MaxUlpError;
            var parameterAt = maxUlpError.ParameterAt;
            var check0 = (float)Math.Sin(parameterAt);
            var check1 = FastMath.Sin(parameterAt);
            Console.WriteLine($"Maximum Ulp Difference(Check): {ErrorUtils.CalculateAbsoluteUlpDifference(check0, check1)} at {parameterAt} ({Math.Abs(check0 - check1)})");
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
            var res = ErrorUtils.EvaluateErrors(0.0f, 0.5f,
                (a) =>
                {
                    nint i, length = a.Length;
                    ref var x9 = ref MemoryMarshal.GetReference(a);
                    var olen = length - 3;
                    for (i = 0; i < olen; i += 4)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        var s0 = x10;
                        var s1 = Unsafe.Add(ref x9, i + 1);
                        var s2 = Unsafe.Add(ref x9, i + 2);
                        var s3 = Unsafe.Add(ref x9, i + 3);
                        x10 = (float)Math.Sin(s0 * Math.PI);
                        Unsafe.Add(ref x10, 1) = (float)Math.Sin(s1 * Math.PI);
                        Unsafe.Add(ref x10, 2) = (float)Math.Sin(s2 * Math.PI);
                        Unsafe.Add(ref x10, 3) = (float)Math.Sin(s3 * Math.PI);
                    }
                    for (; i < length; i++)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        x10 = (float)Math.Sin(x10 * Math.PI);
                    }
                },
                (a) =>
                {
                    nint i, length = a.Length;
                    ref var x9 = ref MemoryMarshal.GetReference(a);
                    var olen = length - 3;
                    for (i = 0; i < olen; i += 4)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        var s0 = x10;
                        var s1 = Unsafe.Add(ref x9, i + 1);
                        var s2 = Unsafe.Add(ref x9, i + 2);
                        var s3 = Unsafe.Add(ref x9, i + 3);
                        x10 = FastMath.SinPi(s0);
                        Unsafe.Add(ref x10, 1) = FastMath.SinPi(s1);
                        Unsafe.Add(ref x10, 2) = FastMath.SinPi(s2);
                        Unsafe.Add(ref x10, 3) = FastMath.SinPi(s3);
                    }
                    for (; i < length; i++)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        x10 = FastMath.SinPi(x10);
                    }
                });
            ErrorUtils.WriteResult(res);
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
            var res = ErrorUtils.EvaluateErrors(0.0f, MathF.PI * 0.5f,
                (a) =>
                {
                    nint i, length = a.Length;
                    ref var x9 = ref MemoryMarshal.GetReference(a);
                    var olen = length - 3;
                    for (i = 0; i < olen; i += 4)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        var s0 = x10;
                        var s1 = Unsafe.Add(ref x9, i + 1);
                        var s2 = Unsafe.Add(ref x9, i + 2);
                        var s3 = Unsafe.Add(ref x9, i + 3);
                        x10 = MathF.Sin(s0);
                        Unsafe.Add(ref x10, 1) = MathF.Sin(s1);
                        Unsafe.Add(ref x10, 2) = MathF.Sin(s2);
                        Unsafe.Add(ref x10, 3) = MathF.Sin(s3);
                    }
                    for (; i < length; i++)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        x10 = MathF.Sin(x10);
                    }
                },
                (a) =>
                {
                    nint i, length = a.Length;
                    ref var x9 = ref MemoryMarshal.GetReference(a);
                    var olen = length - 3;
                    for (i = 0; i < olen; i += 4)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        var s0 = x10;
                        var s1 = Unsafe.Add(ref x9, i + 1);
                        var s2 = Unsafe.Add(ref x9, i + 2);
                        var s3 = Unsafe.Add(ref x9, i + 3);
                        x10 = FastMath.FastSin(s0);
                        Unsafe.Add(ref x10, 1) = FastMath.FastSin(s1);
                        Unsafe.Add(ref x10, 2) = FastMath.FastSin(s2);
                        Unsafe.Add(ref x10, 3) = FastMath.FastSin(s3);
                    }
                    for (; i < length; i++)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        x10 = FastMath.FastSin(x10);
                    }
                });
            ErrorUtils.WriteResult(res);
            Assert.Pass();
        }
        [TestCase(1.0f / 2)]
        [TestCase(-1.0f / 2)]
        [TestCase(1.0f)]
        [TestCase(-1.0f)]
        [TestCase(0.0f)]
        [TestCase(float.Epsilon)]
        [TestCase(-float.Epsilon)]

        public void FastSinPiCalculatesCorrectly(float value)
        {
            var exp = (float)Math.Sin(value * Math.PI);
            var act = FastMath.FastSinPi(value);
            Assert.That(act, Is.EqualTo(exp).Within(8.74227766E-08f));
            Console.WriteLine($"Expected: {exp}, Actual: {act}");
        }

        [Test]
        public void FastSinPiBruteForceAccuracyCheck()
        {
            var res = ErrorUtils.EvaluateErrors(0.0f, 0.5f,
                (a) =>
                {
                    nint i, length = a.Length;
                    ref var x9 = ref MemoryMarshal.GetReference(a);
                    var olen = length - 3;
                    for (i = 0; i < olen; i += 4)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        var s0 = x10;
                        var s1 = Unsafe.Add(ref x9, i + 1);
                        var s2 = Unsafe.Add(ref x9, i + 2);
                        var s3 = Unsafe.Add(ref x9, i + 3);
                        x10 = (float)Math.Sin(s0 * Math.PI);
                        Unsafe.Add(ref x10, 1) = (float)Math.Sin(s1 * Math.PI);
                        Unsafe.Add(ref x10, 2) = (float)Math.Sin(s2 * Math.PI);
                        Unsafe.Add(ref x10, 3) = (float)Math.Sin(s3 * Math.PI);
                    }
                    for (; i < length; i++)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        x10 = (float)Math.Sin(x10 * Math.PI);
                    }
                },
                (a) =>
                {
                    nint i, length = a.Length;
                    ref var x9 = ref MemoryMarshal.GetReference(a);
                    var olen = length - 3;
                    for (i = 0; i < olen; i += 4)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        var s0 = x10;
                        var s1 = Unsafe.Add(ref x9, i + 1);
                        var s2 = Unsafe.Add(ref x9, i + 2);
                        var s3 = Unsafe.Add(ref x9, i + 3);
                        x10 = FastMath.FastSinPi(s0);
                        Unsafe.Add(ref x10, 1) = FastMath.FastSinPi(s1);
                        Unsafe.Add(ref x10, 2) = FastMath.FastSinPi(s2);
                        Unsafe.Add(ref x10, 3) = FastMath.FastSinPi(s3);
                    }
                    for (; i < length; i++)
                    {
                        ref var x10 = ref Unsafe.Add(ref x9, i);
                        x10 = FastMath.FastSinPi(x10);
                    }
                });
            ErrorUtils.WriteResult(res);
            Assert.Pass();
        }

        #endregion

        #region Cos
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
            var res = ErrorUtils.EvaluateErrors(0.0f, MathF.PI * 0.5f,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = (float)Math.Cos(s0);
                         Unsafe.Add(ref x10, 1) = (float)Math.Cos(s1);
                         Unsafe.Add(ref x10, 2) = (float)Math.Cos(s2);
                         Unsafe.Add(ref x10, 3) = (float)Math.Cos(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = (float)Math.Cos(x10);
                     }
                 },
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.Cos(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.Cos(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.Cos(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.Cos(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.Cos(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
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
            var res = ErrorUtils.EvaluateErrors(0.0f, MathF.PI * 0.5f,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = MathF.Cos(s0);
                         Unsafe.Add(ref x10, 1) = MathF.Cos(s1);
                         Unsafe.Add(ref x10, 2) = MathF.Cos(s2);
                         Unsafe.Add(ref x10, 3) = MathF.Cos(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = MathF.Cos(x10);
                     }
                 },
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.FastCos(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.FastCos(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.FastCos(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.FastCos(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.FastCos(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
            Assert.Pass();
        }
        #endregion
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
            var res = ErrorUtils.EvaluateErrors(1.0f, 2.0f,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = (float)Math.Pow(2.0, s0);
                         Unsafe.Add(ref x10, 1) = (float)Math.Pow(2.0, s1);
                         Unsafe.Add(ref x10, 2) = (float)Math.Pow(2.0, s2);
                         Unsafe.Add(ref x10, 3) = (float)Math.Pow(2.0, s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = (float)Math.Pow(2.0, x10);
                     }
                 },
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.Exp2(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.Exp2(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.Exp2(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.Exp2(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.Exp2(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
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
            var res = ErrorUtils.EvaluateErrors(1.0f, 2.0f,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = (float)Math.Pow(2.0, s0);
                         Unsafe.Add(ref x10, 1) = (float)Math.Pow(2.0, s1);
                         Unsafe.Add(ref x10, 2) = (float)Math.Pow(2.0, s2);
                         Unsafe.Add(ref x10, 3) = (float)Math.Pow(2.0, s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = (float)Math.Pow(2.0, x10);
                     }
                 },
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.FastExp2(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.FastExp2(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.FastExp2(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.FastExp2(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.FastExp2(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
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
            var res = ErrorUtils.EvaluateErrors(1.0f, 2.0f,
                 GenerateLog2Reference,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.Log2AsNormal(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.Log2AsNormal(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.Log2AsNormal(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.Log2AsNormal(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.Log2AsNormal(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
            Assert.Pass();
        }

        [TestCase(0.5f)]
        [TestCase(-0.5f)]
        [TestCase(1.0f)]
        [TestCase(1.1754944E-38f)]
        [TestCase(7.83662853838e-39f)]
        [TestCase(MathF.PI)]
        [TestCase(0.0f)]
        [TestCase(1.0000027f, 3.9555955470941250298012288333906993583409943602314108378923E-6)]
        public void Log2CalculatesCorrectly(float value, double? expected = null)
        {
            var exp = expected ?? Math.Log2(MathF.Abs(value));
            var act = FastMath.Log2(value);
            var diffulp = MathI.Abs(BitConverter.SingleToInt32Bits((float)exp) - BitConverter.SingleToInt32Bits(act));
            Console.WriteLine($"Expected:   {exp}\t({(float)exp}f)\nActual: \t{(double)act}\t({act}f)\nOff by {exp - act}\t({diffulp}ulp)");
            Assert.That(BitConverter.SingleToUInt32Bits(act), Is.EqualTo(BitConverter.SingleToUInt32Bits((float)exp)).Within(1));
        }

        [Test]
        public void Log2BruteForceAccuracyCheck()
        {
            var res = ErrorUtils.EvaluateErrors(0.0f, 2.35098870164e-38f,
                 GenerateLog2Reference,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.Log2(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.Log2(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.Log2(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.Log2(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.Log2(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
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
            var res = ErrorUtils.EvaluateErrors(1.0f, 2.0f,
                 GenerateLog2Reference,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.FastLog2AsNormal(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.FastLog2AsNormal(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.FastLog2AsNormal(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.FastLog2AsNormal(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.FastLog2AsNormal(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
            Assert.Pass();
        }

        [TestCase(0.5f)]
        [TestCase(-0.5f)]
        [TestCase(1.0f)]
        [TestCase(1.1754944E-38f)]
        [TestCase(7.83662853838e-39f)]
        [TestCase(MathF.PI)]
        [TestCase(0.0f)]
        [TestCase(1.0000027f, 3.9555955470941250298012288333906993583409943602314108378923E-6)]
        public void FastLog2CalculatesCorrectly(float value, double? expected = null)
        {
            var exp = expected ?? Math.Log2(MathF.Abs(value));
            var act = FastMath.FastLog2(value);
            Console.WriteLine($"Expected:   {exp}\t({(float)exp}f)\nActual: \t{(double)act}\t({act}f)\nOff by {exp - act}\t({(float)exp - act}f)");
            Assert.That(BitConverter.SingleToUInt32Bits(act), Is.EqualTo(BitConverter.SingleToUInt32Bits((float)exp)).Within(1));
        }

        [Test]
        public void FastLog2BruteForceAccuracyCheck()
        {
            var res = ErrorUtils.EvaluateErrors(0.0f, 2.35098870164e-38f,
                 GenerateLog2Reference,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.FastLog2(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.FastLog2(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.FastLog2(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.FastLog2(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.FastLog2(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
            Assert.Pass();
        }

        private static void GenerateLog2Reference(Span<float> a)
        {
            nint i, length = a.Length;
            ref var x9 = ref MemoryMarshal.GetReference(a);
            var olen = length - 3;
            for (i = 0; i < olen; i += 4)
            {
                ref var x10 = ref Unsafe.Add(ref x9, i);
                var s0 = x10;
                var s1 = Unsafe.Add(ref x9, i + 1);
                var s2 = Unsafe.Add(ref x9, i + 2);
                var s3 = Unsafe.Add(ref x9, i + 3);
                x10 = (float)Math.Log2(s0);
                Unsafe.Add(ref x10, 1) = (float)Math.Log2(s1);
                Unsafe.Add(ref x10, 2) = (float)Math.Log2(s2);
                Unsafe.Add(ref x10, 3) = (float)Math.Log2(s3);
            }
            for (; i < length; i++)
            {
                ref var x10 = ref Unsafe.Add(ref x9, i);
                x10 = (float)Math.Log2(x10);
            }
        }
        #endregion

        #region Root
        [TestCase(0.0f, 1.1754944E-38f)]
        [TestCase(1.1754944E-38f, 1.1754944E-38f * 2.0f)]
        [TestCase(1.0f, 8.0f)]
        public void CbrtBruteForceAccuracyCheck(float start, float end)
        {
            var res = ErrorUtils.EvaluateErrors(start, end,
                 GenerateCbrtReference,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.Cbrt(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.Cbrt(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.Cbrt(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.Cbrt(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.Cbrt(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
            Assert.Pass();
        }
        [TestCase(0.0f, 1.1754944E-38f)]
        [TestCase(1.1754944E-38f, 1.1754944E-38f * 2.0f)]
        [TestCase(1.0f, 8.0f)]
        public void FastCbrtBruteForceAccuracyCheck(float start, float end)
        {
            var res = ErrorUtils.EvaluateErrors(start, end,
                 GenerateCbrtReference,
                 (a) =>
                 {
                     nint i, length = a.Length;
                     ref var x9 = ref MemoryMarshal.GetReference(a);
                     var olen = length - 3;
                     for (i = 0; i < olen; i += 4)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         var s0 = x10;
                         var s1 = Unsafe.Add(ref x9, i + 1);
                         var s2 = Unsafe.Add(ref x9, i + 2);
                         var s3 = Unsafe.Add(ref x9, i + 3);
                         x10 = FastMath.FastCbrt(s0);
                         Unsafe.Add(ref x10, 1) = FastMath.FastCbrt(s1);
                         Unsafe.Add(ref x10, 2) = FastMath.FastCbrt(s2);
                         Unsafe.Add(ref x10, 3) = FastMath.FastCbrt(s3);
                     }
                     for (; i < length; i++)
                     {
                         ref var x10 = ref Unsafe.Add(ref x9, i);
                         x10 = FastMath.FastCbrt(x10);
                     }
                 });
            ErrorUtils.WriteResult(res);
            Assert.Pass();
        }
        private static void GenerateCbrtReference(Span<float> a)
        {
            nint i, length = a.Length;
            ref var x9 = ref MemoryMarshal.GetReference(a);
            var olen = length - 3;
            for (i = 0; i < olen; i += 4)
            {
                ref var x10 = ref Unsafe.Add(ref x9, i);
                var s0 = x10;
                var s1 = Unsafe.Add(ref x9, i + 1);
                var s2 = Unsafe.Add(ref x9, i + 2);
                var s3 = Unsafe.Add(ref x9, i + 3);
                x10 = (float)Math.Cbrt(s0);
                Unsafe.Add(ref x10, 1) = (float)Math.Cbrt(s1);
                Unsafe.Add(ref x10, 2) = (float)Math.Cbrt(s2);
                Unsafe.Add(ref x10, 3) = (float)Math.Cbrt(s3);
            }
            for (; i < length; i++)
            {
                ref var x10 = ref Unsafe.Add(ref x9, i);
                x10 = (float)Math.Cbrt(x10);
            }
        }
        #endregion
    }
}
