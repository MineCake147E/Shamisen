using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Filters.Buffering;
using Shamisen.TestUtils;

namespace Shamisen.Core.Tests.CoreFx.Conversion.SampleToWaveConverters
{
    [TestFixture]
    public class SampleToALawConverterTest
    {
        [Test]
        public void AllValuesRoundtripsCorrectly()
        {
            var c = 0;
            for (var i = 0; i < 256; i++)
            {
                var u = (byte)(i);
                var e = ALawToSampleConverter.ConvertALawToSingle(u);
                var b = SampleToALawConverter.ConvertSingleToALaw(e);
                if (b != u)
                {
                    c++;
                    Console.WriteLine($"{u:X2}: {e}, {ALawToSampleConverter.ConvertALawToSingle(b)}({b:X2})");
                }
            }
            Assert.AreEqual(0, c);
        }

        [TestCase(255)]
        [TestCase(256)]
        [TestCase(257)]
        [TestCase(16385)]
        public void ProcessAvx2ConvertsCorrectly(int length)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn($"{nameof(Avx2)} is not supported!");
                return;
            }
            PrepareArraysNormal(length, out var src, out var exp, out var dst);
            SampleToALawConverter.ProcessAvx2(src, dst);
            AssertArrayNormal(src, exp, dst);
        }
        [TestCase(255)]
        [TestCase(256)]
        [TestCase(257)]
        [TestCase(16385)]
        public void ProcessStandardVectorizedConvertsCorrectly(int length)
        {
            PrepareArraysNormal(length, out var src, out var exp, out var dst);
            SampleToALawConverter.ProcessStandardVectorized(src, dst);
            AssertArrayNormal(src, exp, dst);
        }
        private static void PrepareArraysNormal(int length, out float[] src, out byte[] exp, out byte[] dst)
        {
            src = new float[length];
            exp = new byte[src.Length];
            var temp = new float[256];
            for (var i = 0; i < temp.Length; i++)
            {
                temp[i] = ALawToSampleConverter.ConvertALawToSingle((byte)~i);
            }
            var f = 255.0 / (length - 1);
            for (var i = 0; i < src.Length; i++)
            {
                var p = f * i;
                var a = MathI.Min((int)Math.Ceiling(p), temp.Length - 1);
                var b = MathI.Max((int)Math.Floor(p), 0);
                var x0 = temp[a];
                var x1 = temp[b];
                var ratio = a - p;

                var v = src[i] = (float)(ratio * x1 + (1 - ratio) * x0);
                exp[i] = SampleToALawConverter.ConvertSingleToALaw(v);
            }
            dst = new byte[src.Length];
        }

        private static void AssertArrayNormal(float[] src, byte[] exp, byte[] dst)
        {
            NeumaierAccumulator sumdiff = default;
            for (var i = 0; i < dst.Length; i++)
            {
                var simple = ALawToSampleConverter.ConvertALawToSingle(exp[i]);
                var optimized = ALawToSampleConverter.ConvertALawToSingle(dst[i]);
                var diff = (simple - optimized) * (1.0 / 128);
                sumdiff += Math.Abs(diff);
                if (diff != 0)
                    Console.WriteLine($"{i:X8}: {src[i]}, {simple}({exp[i]:X2}), {optimized}({dst[i]:X2}), {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            var avgDiff = sumdiff.Sum / src.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, 1f - MathF.BitDecrement(1f));
        }
    }
}
