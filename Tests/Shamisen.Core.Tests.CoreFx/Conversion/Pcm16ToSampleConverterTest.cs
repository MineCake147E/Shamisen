﻿// <auto-generated />
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

namespace Shamisen.Core.Tests.CoreFx.Conversion
{
    [TestFixture]
    public class Pcm16ToSampleConverterTest
    {


        private static void CheckResultNormal(Span<short> spanI, Span<float> spanF)
        {
            const float M = 1 / 32768.0f;
            var na = new NeumaierAccumulator(default, default);
            for (int i = 0; i < spanI.Length && i < spanF.Length; i++)
            {
                double d = (float)spanI[i] * M - spanF[i];
                na += d * d;
            }
            Assert.AreEqual(0.0, na.Sum);
        }
        private static void CheckResultReversed(Span<short> spanI, Span<float> spanF)
        {
            const float M = 1 / 32768.0f;
            var na = new NeumaierAccumulator(default, default);
            for (int i = 0; i < spanI.Length && i < spanF.Length; i++)
            {
                double d = (float)BinaryPrimitives.ReverseEndianness(spanI[i]) * M - spanF[i];
                na += d * d;
            }
            Assert.AreEqual(0.0, na.Sum);
        }

        private static void PrepareArrays(int frames, out Span<short> spanI, out Span<float> spanF)
        {
            byte[] input = new byte[frames * sizeof(short)];
            byte[] output = new byte[frames * sizeof(float)];
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
            Span<short> spanI;
            Span<float> spanF;
            PrepareArrays(frames, out spanI, out spanF);
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
            Span<short> spanI;
            Span<float> spanF;
            PrepareArrays(frames, out spanI, out spanF);
            Pcm16ToSampleConverter.ProcessNormalAvx2(spanF, spanI);
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
            Span<short> spanI;
            Span<float> spanF;
            PrepareArrays(frames, out spanI, out spanF);
            Pcm16ToSampleConverter.ProcessNormalSse41(spanF, spanI);
            CheckResultNormal(spanI, spanF);
        }
        [TestCase(2048)]
        [TestCase(2881)]
        public void ProcessReversedStandardConvertsCorrectly(int frames)
        {
            Span<short> spanI;
            Span<float> spanF;
            PrepareArrays(frames, out spanI, out spanF);
            Pcm16ToSampleConverter.ProcessReversed(spanI, spanF);
            CheckResultReversed(spanI, spanF);
        }


    }
}
