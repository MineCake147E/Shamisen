using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

using NUnit.Framework;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Synthesis;
using Shamisen.TestUtils;

namespace Shamisen.Core.Tests.CoreFx.Conversion.SampleToWaveConverters
{
    [TestFixture]
    public class SampleToPcm16ConverterTest
    {
        private static void CheckIntrinsicsConsistency(int sampleRate, int channels, X86Intrinsics x86Intrinsics, ArmIntrinsics armIntrinsics, bool accuracyNeeded = true, Endianness endianness = Endianness.Little)
        {
            Console.WriteLine($"Enabled x86-64 intrinsics: {x86Intrinsics}");
            Console.WriteLine($"Enabled ARM intrinsics: {armIntrinsics}");
            var Frequency = sampleRate / Math.PI;
            var format = new SampleFormat(channels, sampleRate);
            using var srcNoIntrinsics = new FilterTestSignalSource(format) { Frequency = Frequency };
            using var srcIntrinsics = new FilterTestSignalSource(format) { Frequency = Frequency };
            using var filterNoIntrinsics = new SampleToPcm16Converter(srcNoIntrinsics, false, x86Intrinsics, armIntrinsics, accuracyNeeded, endianness);
            using var filterIntrinsics = new SampleToPcm16Converter(srcIntrinsics, true, x86Intrinsics, armIntrinsics, accuracyNeeded, endianness);
            var bufferNoIntrinsics = new short[255 * channels];
            var bufferIntrinsics = new short[bufferNoIntrinsics.Length];

            filterNoIntrinsics.Read(MemoryMarshal.Cast<short, byte>(bufferNoIntrinsics));
            filterIntrinsics.Read(MemoryMarshal.Cast<short, byte>(bufferIntrinsics));
            NeumaierAccumulator sumdiff = default;
            for (var i = 0; i < bufferNoIntrinsics.Length; i++)
            {
                var simple = bufferNoIntrinsics[i];
                var optimized = bufferIntrinsics[i];
                float diff = simple - optimized;
                sumdiff += MathF.Abs(diff);
                Console.WriteLine($"{simple}, {optimized}, {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            var avgDiff = sumdiff.Sum / bufferNoIntrinsics.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, 1f - MathF.BitDecrement(1f));
        }

        [TestCase(1, false, Endianness.Little, (X86Intrinsics)X86IntrinsicsMask.Avx2)]
        [TestCase(1, false, Endianness.Big, (X86Intrinsics)X86IntrinsicsMask.Avx2)]
        [TestCase(1, false, Endianness.Little, (X86Intrinsics)X86IntrinsicsMask.Sse2)]
        [TestCase(1, false, Endianness.Big, (X86Intrinsics)X86IntrinsicsMask.Ssse3)]
        public void CF2Pcm16IntrinsicsConsistency(int channels, bool isAccuracyNeeded, Endianness endianness, X86Intrinsics x86Intrinsics = (X86Intrinsics)~0ul, ArmIntrinsics armIntrinsics = (ArmIntrinsics)~0ul)
        {
            const int SampleRate = 48000;
            x86Intrinsics &= IntrinsicsUtils.X86Intrinsics;
            armIntrinsics &= IntrinsicsUtils.ArmIntrinsics;
            CheckIntrinsicsConsistency(SampleRate, channels, x86Intrinsics, armIntrinsics, isAccuracyNeeded, endianness);
        }

        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessNormalStandardConvertsCorrectly(int length)
        {
            PrepareArraysNormal(length, out var src, out var exp, out var dst);
            SampleToPcm16Converter.ProcessNormalStandard(dst, src);
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
            SampleToPcm16Converter.ProcessNormalAvx2(dst, src);
            AssertArrayNormal(src, exp, dst);
        }

        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessNormalSse2ConvertsCorrectly(int length)
        {
            if (!Sse2.IsSupported)
            {
                Assert.Warn($"{nameof(Sse2)} is not supported!");
                return;
            }
            PrepareArraysNormal(length, out var src, out var exp, out var dst);
            SampleToPcm16Converter.ProcessNormalSse2(dst, src);
            AssertArrayNormal(src, exp, dst);
        }

        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessReversedAvx2ConvertsCorrectly(int length)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn($"{nameof(Avx2)} is not supported!");
                return;
            }
            PrepareArraysReversed(length, out var src, out var exp, out var dst);
            SampleToPcm16Converter.ProcessReversedAvx2(dst, src);
            AssertArrayReversed(src, exp, dst);
        }

        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ProcessReversedSsse3ConvertsCorrectly(int length)
        {
            if (!Ssse3.IsSupported)
            {
                Assert.Warn($"{nameof(Ssse3)} is not supported!");
                return;
            }
            PrepareArraysReversed(length, out var src, out var exp, out var dst);
            SampleToPcm16Converter.ProcessReversedSsse3(dst, src);
            AssertArrayReversed(src, exp, dst);
        }

        private static void AssertArrayNormal(float[] src, short[] exp, short[] dst)
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
        private static void AssertArrayReversed(float[] src, short[] exp, short[] dst)
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

        private static void PrepareArraysNormal(int length, out float[] src, out short[] exp, out short[] dst)
        {
            src = new float[length];
            exp = new short[src.Length];
            var rlen = 2.0 / (length - 1);
            var lhalf = (length - 1) / 2.0;
            for (var i = 0; i < src.Length; i++)
            {
                var v = src[i] = (float)((i - lhalf) * rlen);
                exp[i] = (short)Math.Round(Math.Min(short.MaxValue, Math.Max(v * 32768.0f, short.MinValue)));
            }
            dst = new short[src.Length];
        }

        private static void PrepareArraysReversed(int length, out float[] src, out short[] exp, out short[] dst)
        {
            src = new float[length];
            exp = new short[src.Length];
            var rlen = 2.0 / (length - 1);
            var lhalf = (length - 1) / 2.0;
            for (var i = 0; i < src.Length; i++)
            {
                var v = src[i] = (float)((i - lhalf) * rlen);
                exp[i] = BinaryPrimitives.ReverseEndianness((short)Math.Round(Math.Min(short.MaxValue, Math.Max(v * 32768.0f, short.MinValue))));
            }
            dst = new short[src.Length];
        }
    }
}
