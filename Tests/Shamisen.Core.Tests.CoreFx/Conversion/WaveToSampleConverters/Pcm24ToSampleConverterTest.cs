using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Core.Tests.CoreFx.TestUtils;

namespace Shamisen.Core.Tests.CoreFx.Conversion.WaveToSampleConverters
{
    [TestFixture]
    public class Pcm24ToSampleConverterTest
    {
        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessNormalStandardConvertsCorrectly(int length)
        {
            PrepareArraysNormal(length, out var src, out var exp, out var dst);
            Pcm24ToSampleConverter.ProcessNormalStandard(src, dst);
            AssertArrayNormal(src, exp, dst);
        }

        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessNormalAvx2PConvertsCorrectly(int length)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn($"{nameof(Avx2)} is not supported!");
                return;
            }
            PrepareArraysNormal(length, out var src, out var exp, out var dst);
            Pcm24ToSampleConverter.ProcessNormalAvx2P(src, dst);
            AssertArrayNormal(src, exp, dst);
        }

        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessNormalAvx2MConvertsCorrectly(int length)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn($"{nameof(Avx2)} is not supported!");
                return;
            }
            PrepareArraysNormal(length, out var src, out var exp, out var dst);
            Pcm24ToSampleConverter.ProcessNormalAvx2M(src, dst);
            AssertArrayNormal(src, exp, dst);
        }

        private static void PrepareArraysNormal(int length, out Int24[] src, out float[] exp, out float[] dst)
        {
            exp = new float[length];
            src = new Int24[exp.Length];
            var rlen = 2.0 / (length - 1);
            var lhalf = (length - 1) / 2.0;
            for (var i = 0; i < src.Length; i++)
            {
                var v = (float)((i - lhalf) * rlen);
                var w = src[i] = (Int24)Math.Round(Math.Min(8388607, Math.Max(v * 8388608.0f, -8388608)));
                exp[i] = w * (1f / 8388608.0f);
            }
            dst = new float[src.Length];
        }
        private static void AssertArrayNormal(Int24[] src, float[] exp, float[] dst)
        {
            NeumaierAccumulator sumdiff = default;
            for (var i = 0; i < dst.Length; i++)
            {
                var simple = exp[i];
                var optimized = dst[i];
                var diff = (simple - optimized) * (1.0 / 128);
                sumdiff += Math.Abs(diff);
                if (diff != 0)
                    Console.WriteLine($"{i}: {src[i]}, {simple}, {optimized}, {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            var avgDiff = sumdiff.Sum / src.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, 1f - MathF.BitDecrement(1f));
        }
    }
}
