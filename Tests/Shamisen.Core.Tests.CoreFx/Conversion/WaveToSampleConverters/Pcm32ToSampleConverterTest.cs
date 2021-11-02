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

namespace Shamisen.Core.Tests.CoreFx.Conversion.WaveToSampleConverters
{
    [TestFixture]
    public class Pcm32ToSampleConverterTest
    {


        private static void CheckResultNormal(Span<int> spanI, Span<float> spanF)
        {
            const float M = 1.0f / 2147483648.0f;
            var na = new NeumaierAccumulator(default, default);
            for (var i = 0; i < spanI.Length && i < spanF.Length; i++)
            {
                double d = spanI[i] * M - spanF[i];
                na += d * d;
            }
            Assert.AreEqual(0.0, na.Sum);
        }
        private static void CheckResultReversed(Span<int> spanI, Span<float> spanF)
        {
            const float M = 1.0f / 2147483648.0f;
            var na = new NeumaierAccumulator(default, default);
            for (var i = 0; i < spanI.Length && i < spanF.Length; i++)
            {
                double d = BinaryPrimitives.ReverseEndianness(spanI[i]) * M - spanF[i];
                na += d * d;
            }
            Assert.AreEqual(0.0, na.Sum);
        }

        private static void PrepareArrays(int frames, out Span<int> spanI, out Span<float> spanF)
        {
            var input = new byte[frames * sizeof(int)];
            var output = new byte[frames * sizeof(float)];
            RandomNumberGenerator.Fill(input);
            spanI = MemoryMarshal.Cast<byte, int>(input.AsSpan());
            spanF = MemoryMarshal.Cast<byte, float>(output.AsSpan());
            Assert.AreEqual(spanI.Length, spanF.Length, "Length doesn't match! This is a bug!");
            spanI.CopyTo(MemoryMarshal.Cast<float, int>(spanF));
        }

        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessNormalStandardConvertsCorrectly(int frames)
        {
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm32ToSampleConverter.ProcessNormalStandard(spanF);
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
            Pcm32ToSampleConverter.ProcessNormalAvx2(spanF);
            CheckResultNormal(spanI, spanF);
        }
        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessNormalAvx2ExtremeUnrollConvertsCorrectly(int frames)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("AVX2 is not supported!");
                return;
            }
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm32ToSampleConverter.ProcessNormalAvx2ExtremeUnroll(spanF);
            CheckResultNormal(spanI, spanF);
        }
        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessNormalSse2ConvertsCorrectly(int frames)
        {
            if (!Sse2.IsSupported)
            {
                Assert.Warn("SSE2 is not supported!");
                return;
            }
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm32ToSampleConverter.ProcessNormalSse2(spanF);
            CheckResultNormal(spanI, spanF);
        }
        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessReversedStandardConvertsCorrectly(int frames)
        {
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm32ToSampleConverter.ProcessReversedStandard(spanF);
            CheckResultReversed(spanI, spanF);
        }
        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessReversedAvx2ConvertsCorrectly(int frames)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("AVX2 is not supported!");
                return;
            }
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm32ToSampleConverter.ProcessReversedAvx2(spanF);
            CheckResultReversed(spanI, spanF);
        }
        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessReversedSsse3ConvertsCorrectly(int frames)
        {
            if (!Ssse3.IsSupported)
            {
                Assert.Warn("SSSE3 is not supported!");
                return;
            }
            PrepareArrays(frames, out var spanI, out var spanF);
            Pcm32ToSampleConverter.ProcessReversedSsse3(spanF);
            CheckResultReversed(spanI, spanF);
        }

    }
}
