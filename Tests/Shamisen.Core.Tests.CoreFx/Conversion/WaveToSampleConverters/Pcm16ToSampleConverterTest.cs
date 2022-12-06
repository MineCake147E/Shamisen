using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.TestUtils;

namespace Shamisen.Core.Tests.CoreFx.Conversion.WaveToSampleConverters
{
    [TestFixture]
    public class Pcm16ToSampleConverterTest
    {
        private static void CheckResultNormal(Span<short> spanI, Span<float> spanF)
        {
            const float M = 1 / 32768.0f;
            var na = new NeumaierAccumulator(default, default);
            for (var i = 0; i < spanI.Length && i < spanF.Length; i++)
            {
                var simple = spanI[i] * M;
                var optimized = spanF[i];
                double diff = simple - optimized;
                na += diff * diff;
                if (diff != 0)
                    Console.WriteLine($"{i}: {spanI[i]}, {simple}, {optimized}, {diff}");
            }
            Assert.AreEqual(0.0, na.Sum);
        }
        private static void CheckResultReversed(Span<short> spanI, Span<float> spanF)
        {
            spanI.ReverseEndianness();
            CheckResultNormal(spanI, spanF);
        }

        private static void PrepareArrays(int frames, out Span<short> spanI, out Span<float> spanF)
        {
            var input = new byte[frames * sizeof(short)];
            var output = new byte[frames * sizeof(float)];
            RandomNumberGenerator.Fill(input);
            spanI = MemoryMarshal.Cast<byte, short>(input.AsSpan());
            spanF = MemoryMarshal.Cast<byte, float>(output.AsSpan());
            Assert.AreEqual(spanI.Length, spanF.Length, "Length doesn't match! This is a bug!");
            spanI.CopyTo(MemoryMarshal.Cast<float, short>(spanF));
        }

        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessNormalStandardConvertsCorrectly(int frames)
        {
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm16ToSampleConverter.ProcessNormalStandard(spanF, spanI);
            CheckResultNormal(spanI, spanF);
        }
        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessNormalAvx2ConvertsCorrectly(int frames)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("AVX2 is not supported!");
                return;
            }
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm16ToSampleConverter.ProcessNormalAvx2(spanF, spanI);
            CheckResultNormal(spanI, spanF);
        }

        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessNormalAvx2AConvertsCorrectly(int frames)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("AVX2 is not supported!");
                return;
            }
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm16ToSampleConverter.ProcessNormalAvx2A(spanF, spanI);
            CheckResultNormal(spanI, spanF);
        }

        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessNormalSse41ConvertsCorrectly(int frames)
        {
            if (!Sse41.IsSupported)
            {
                Assert.Warn("SSE2 is not supported!");
                return;
            }
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm16ToSampleConverter.ProcessNormalSse41(spanF, spanI);
            CheckResultNormal(spanI, spanF);
        }
        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessReversedStandardConvertsCorrectly(int frames)
        {
            PrepareArrays(frames, out var spanI, out var spanF);
            var y = spanI.ToArray();
            Pcm16ToSampleConverter.ProcessReversed(y, spanF);
            CheckResultReversed(spanI, spanF);
        }
    }
}
