using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.TestUtils;

namespace Shamisen.Core.Tests.CoreFx.Conversion.SampleToWaveConverters
{
    [TestFixture]
    public class SampleToPcm32ConverterTest
    {
        [TestCase(4095)]
        public void ConvertsCorrectly(int length)
        {
            var src = new float[length];
            var exp = new int[src.Length];
            var rlen = 2.0 / (length - 1);
            var lhalf = (length - 1) / 2.0;
            for (var i = 0; i < src.Length; i++)
            {
                var v = src[i] = (float)((i - lhalf) * rlen);
                exp[i] = SampleToPcm32Converter.Convert(v);
            }
            var dst = new int[src.Length];
            var actual = MemoryMarshal.Cast<int, float>(dst.AsSpan());
            src.AsSpan().CopyTo(actual);
            SampleToPcm32Converter.ProcessNormalDirectStandard(actual);
            NeumaierAccumulator sumdiff = default;
            for (var i = 0; i < dst.Length; i++)
            {
                var simple = exp[i];
                var optimized = dst[i];
                var diff = (simple - optimized) * (1.0 / 2147483648);
                sumdiff += Math.Abs(diff);
                Console.WriteLine($"{simple}, {optimized}, {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            var avgDiff = sumdiff.Sum / src.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, 1f - MathF.BitDecrement(1f));
        }
    }
}
