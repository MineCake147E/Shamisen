using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.TestUtils;

namespace Shamisen.Core.Tests.CoreFx.Conversion.WaveToSampleConverters
{
    [TestFixture]
    public class RangedPcm32ToSampleConverterTest
    {
        private static int[] LengthValues => new[] { 4095, 4096, 4097 };

        private static IEnumerable<TestCaseData> DepthSpecificConversionTestCaseSource()
        {
            var lengthValues = LengthValues;
            var lebd = lengthValues.ToArray();
            return lebd.Select(c => new TestCaseData(c));
        }

        private static IEnumerable<TestCaseData> ConversionTestCaseSource()
        {
            var lengthValues = LengthValues;
            var bitDepthValues = Enumerable.Range(2, 31);   //1-bit variant is under design consideration.
            var lebd = bitDepthValues.SelectMany(chs => lengthValues.Select(r => (r, chs))).ToArray();
            return lebd.Select(c => new TestCaseData(c.r, c.chs));
        }

        private static IEnumerable<TestCaseData> MultiplicativeConversionTestCaseSource()
        {
            var lengthValues = LengthValues;
            var bitDepthValues = Enumerable.Range(24, 31 - 24);
            var lebd = bitDepthValues.SelectMany(chs => lengthValues.Select(r => (r, chs))).ToArray();
            return lebd.Select(c => new TestCaseData(c.r, c.chs));
        }

        private static IEnumerable<TestCaseData> AdditiveConversionTestCaseSource()
        {
            var lengthValues = new[] { 4095, 4096, 4097 };
            var bitDepthValues = Enumerable.Range(2, 23).Where(a => a % 8 != 0);
            var lebd = bitDepthValues.SelectMany(chs => lengthValues.Select(r => (r, chs))).ToArray();
            return lebd.Select(c => new TestCaseData(c.r, c.chs));
        }

        [TestCaseSource(nameof(ConversionTestCaseSource))]
        public void ProcessStandardConvertsCorrectly(int length, int effectiveBitDepth)
        {
            PrepareArraysNormal(length, effectiveBitDepth, out var src, out var exp, out var dst);
            RangedPcm32ToSampleConverter.ProcessEMoreThan24Standard(dst, src, effectiveBitDepth);
            AssertArrayNormal(src, exp, dst);
        }

        private static void PrepareArraysNormal(int length, int effectiveBitDepth, out int[] src, out float[] exp, out float[] dst)
        {
            exp = new float[length];
            src = new int[exp.Length];
            var max = (int)MathI.ZeroHighBits(effectiveBitDepth - 1, ~0u);
            var min = ~max;
            var maxf = (float)max;
            var minf = (float)min;
            var mulf = -minf;
            var divf = 1.0f / mulf;
            var rlen = 2.0 / (length - 1);
            var lhalf = (length - 1) / 2.0;
            for (var i = 0; i < src.Length; i++)
            {
                var v = (float)((i - lhalf) * rlen);
                var w = src[i] = (int)Math.Round(Math.Min(maxf, Math.Max(v * mulf, minf)));
                exp[i] = w * divf;
            }
            dst = new float[src.Length];
            src.AsSpan().CopyTo(MemoryMarshal.Cast<float, int>(dst));
        }
        private static void AssertArrayNormal(int[] src, float[] exp, float[] dst)
        {
            NeumaierAccumulator sumdiff = default;
            for (var i = 0; i < dst.Length; i++)
            {
                double simple = exp[i];
                double optimized = dst[i];
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
