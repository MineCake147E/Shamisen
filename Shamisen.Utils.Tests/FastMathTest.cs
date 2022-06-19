using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.TestUtils;

namespace Shamisen.Utils.Tests
{
    [TestFixture]
    public class FastMathTest
    {
        [TestCase(MathF.PI / 2)]
        [TestCase(-MathF.PI / 2)]
        [TestCase(MathF.PI)]
        [TestCase(-MathF.PI)]
        [TestCase(0.0f)]
        [TestCase(float.Epsilon)]
        [TestCase(-float.Epsilon)]

        public void SinCalculatesCorrectly(float value)
        {
            var exp = MathF.Sin(value);
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
                var exp = MathF.Sin(v);
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
    }
}
