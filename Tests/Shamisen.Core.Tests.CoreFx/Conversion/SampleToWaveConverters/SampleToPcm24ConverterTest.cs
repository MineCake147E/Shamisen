using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Core.Tests.CoreFx.TestUtils;

namespace Shamisen.Core.Tests.CoreFx.Conversion.SampleToWaveConverters
{
    [TestFixture]
    public class SampleToPcm24ConverterTest
    {

        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessNormalStandardConvertsCorrectly(int length)
        {
            PrepareArraysNormal(length, out var src, out var exp, out var dst);
            SampleToPcm24Converter.ProcessNormalStandard(src, dst);
            AssertArrayNormal(src, exp, dst);
        }

        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessNormalAvx2ConvertsCorrectly(int length)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn($"{nameof(Avx2)} is not supported!");
                return;
            }
            PrepareArraysNormal(length, out var src, out var exp, out var dst);
            SampleToPcm24Converter.ProcessNormalAvx2(src, dst);
            AssertArrayNormal(src, exp, dst);
        }

        private static void AssertArrayNormal(float[] src, Int24[] exp, Int24[] dst)
        {
            NeumaierAccumulator sumdiff = default;
            for (var i = 0; i < dst.Length; i++)
            {
                var simple = exp[i];
                var optimized = dst[i];
                var diff = (simple - optimized) * (1.0 / 128);
                sumdiff += Math.Abs(diff);
                if (diff != 0)
                    Console.WriteLine($"{src[i]}, {simple}, {optimized}, {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            var avgDiff = sumdiff.Sum / src.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, 1f - MathF.BitDecrement(1f));
        }
        private static void AssertArrayReversed(float[] src, Int24[] exp, Int24[] dst)
        {
            NeumaierAccumulator sumdiff = default;
            for (var i = 0; i < dst.Length; i++)
            {
                var simple = BinaryPrimitives.ReverseEndianness(exp[i]);
                var optimized = BinaryPrimitives.ReverseEndianness(dst[i]);
                var diff = (simple - optimized) * (1.0 / 128);
                sumdiff += Math.Abs(diff);
                if (diff != 0)
                    Console.WriteLine($"{src[i]}, {simple}, {optimized}, {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            var avgDiff = sumdiff.Sum / src.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, 1f - MathF.BitDecrement(1f));
        }

        private static void PrepareArraysNormal(int length, out float[] src, out Int24[] exp, out Int24[] dst)
        {
            src = new float[length];
            exp = new Int24[src.Length];
            var rlen = 2.0 / (length - 1);
            var lhalf = (length - 1) / 2.0;
            for (var i = 0; i < src.Length; i++)
            {
                var v = src[i] = (float)((i - lhalf) * rlen);
                exp[i] = (Int24)Math.Round(Math.Min(8388607.0f, Math.Max(v * 8388608.0f, -8388608.0f)));
            }
            dst = new Int24[src.Length];
        }

        private static void PrepareArraysReversed(int length, out float[] src, out Int24[] exp, out Int24[] dst)
        {
            src = new float[length];
            exp = new Int24[src.Length];
            var rlen = 2.0 / (length - 1);
            var lhalf = (length - 1) / 2.0;
            for (var i = 0; i < src.Length; i++)
            {
                var v = src[i] = (float)((i - lhalf) * rlen);
                exp[i] = Int24.ReverseEndianness((Int24)Math.Round(Math.Min(8388607.0f, Math.Max(v * 8388608.0f, -8388608.0f))));
            }
            dst = new Int24[src.Length];
        }
    }
}
