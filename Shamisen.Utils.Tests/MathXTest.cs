using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.TestUtils;

namespace Shamisen.Utils.Tests
{
    [TestFixture]
    public class MathXTest
    {
        [TestCase(long.MaxValue)]
        [TestCase(long.MinValue)]
        [TestCase(long.MaxValue / 2 + 1)]
        [TestCase(long.MinValue / 2)]
        [TestCase(0L)]
        [TestCase(1L)]
        [TestCase(-1L)]

        public void SinFCalculatesCorrectly(long value)
        {
            var q = new Fixed64(value);
            var exp = Math.Sin((double)q * Math.PI);
            var act = MathX.SinF(q);
            Assert.That((double)act, Is.EqualTo(exp).Within(1.2246469E-16));
            Console.WriteLine($"Expected: {exp}, Actual: {act}");
        }

        [Test]
        public void SinFBruteForceAccuracyCheck()
        {
            var max = BitConverter.SingleToInt32Bits(0.5f) + 1;
            var na = new NeumaierAccumulator();
            var maxerr = double.NegativeInfinity;
            ulong cnt = 0;
            for (var i = 0; i < max; i++)
            {
                var v = BitConverter.Int32BitsToSingle(i);
                var exp = (float)Math.Sin((double)v * Math.PI);
                var act = MathX.SinFInternalF32(v);
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
