using System;
using System.Collections.Generic;
using System.Diagnostics;
//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

using NUnit.Framework;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Synthesis;
using Shamisen.TestUtils;

namespace Shamisen.Core.Tests.CoreFx.Conversion.SampleToWaveConverters
{
    [TestFixture]
    public class SampleToPcm8ConverterTest
    {
        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessNormalStandardConvertsCorrectly(int length)
        {
            PrepareArrays(length, out var src, out var exp, out var dst);
            SampleToPcm8Converter.ProcessNormalStandard(src, dst);
            AssertArray(src, exp, dst);
        }
        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessNormalAvx2AConvertsCorrectly(int length)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("Avx2 is not supported!");
                return;
            }
            PrepareArrays(length, out var src, out var exp, out var dst);
            SampleToPcm8Converter.ProcessNormalAvx2A(src, dst);
            AssertArray(src, exp, dst);
        }

        private static void AssertArray(float[] src, byte[] exp, byte[] dst)
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

        private static void PrepareArrays(int length, out float[] src, out byte[] exp, out byte[] dst)
        {
            src = new float[length];
            exp = new byte[src.Length];
            var rlen = 2.0 / (length - 1);
            var lhalf = (length - 1) / 2.0;
            for (var i = 0; i < src.Length; i++)
            {
                var v = src[i] = (float)((i - lhalf) * rlen);
                exp[i] = (byte)((byte)Math.Round(Math.Min(sbyte.MaxValue, Math.Max(v * 128, sbyte.MinValue))) + 128);
            }
            dst = new byte[src.Length];
        }
    }
}
