using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

using NUnit.Framework;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class BiQuadFilterTest
    {
        #region Dump

        private static void BiQuadFilterManyFramesDump(int sampleRate, BiQuadParameter parameter, int frameLen = 2047, int framesToWrite = 64, [CallerMemberName] string caller = null)
        {
            var rng = new RandomWaveSource(new WaveFormat(sampleRate, 32, 1, AudioEncoding.LinearPcm), new RandomDataSource(0, 85, 0));
            var src = new Pcm32ToSampleConverter(rng);
            var filter = new BiQuadFilter(src, parameter);
            string path = $"{caller}_{1}ch_{filter.Format.SampleRate}Hz_{frameLen}fpb_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}";
            TestHelper.DumpSampleSource(frameLen, framesToWrite, filter, path);
            filter.Dispose();
        }

        [Test]
        public void BiQuadLPFManyFrameDump()
        {
            const int SampleRate = 48000;

            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateLPFParameter(SampleRate, 4000, Math.Sqrt(0.5)));
            Assert.Pass();
        }

        [Test]
        public void BiQuadHPFManyFrameDump()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateHPFParameter(SampleRate, 4000, Math.Sqrt(0.5)));
            Assert.Pass();
        }

        [Test]
        public void BiQuadBPFManyFrameDumpFromBandWidth()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate,    //Equivalent to Q=5.1875164769281621706495731692014
                BiQuadParameter.CreateBPFParameterFromBandWidth(SampleRate, 6000, 0.25, BpfGainKind.ZeroDBPeakGain));
            Assert.Pass();
        }

        [Test]
        public void BiQuadBPFManyFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateBPFParameterFromQuality(SampleRate, 6000, 1, BpfGainKind.ZeroDBPeakGain));
            Assert.Pass();
        }

        [Test]
        public void BiQuadAPFManyFrameDump()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateAPFParameter(SampleRate, 6000, 1));
            Assert.Pass();
        }

        [Test]
        public void BiQuadNotchManyFrameDumpFromBandWidth()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateNotchFilterParameterFromBandWidth(SampleRate, 6000, 0.25));
            Assert.Pass();
        }

        [Test]
        public void BiQuadNotchManyFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateNotchFilterParameterFromQuality(SampleRate, 6000, 1));
            Assert.Pass();
        }

        [Test]
        public void BiQuadPeakingEQManyFrameDumpFromBandWidth()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreatePeakingEqualizerParameterFromBandWidth(SampleRate, 6000, 0.25, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadPeakingEQManyFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreatePeakingEqualizerParameterFromQuality(SampleRate, 6000, 1, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadHSFManyFrameDumpFromSlope()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateHighShelfFilterParameterFromSlope(SampleRate, 6000, 6, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadHSFManyFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateHighShelfFilterParameterFromQuality(SampleRate, 6000, 1, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadLSFManyFrameDumpFromSlope()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateLowShelfFilterParameterFromSlope(SampleRate, 6000, 6, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadLSFManyFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterManyFramesDump(SampleRate, BiQuadParameter.CreateLowShelfFilterParameterFromQuality(SampleRate, 6000, 2, -6));
            Assert.Pass();
        }

        #endregion Dump

        #region Multi-channel Consistency

        private static void CheckChannelConsistency(int sampleRate, int channels, BiQuadParameter parameter)
        {
            const int Frequency = 2000;
            using var srcMono = new SquareWaveSource(new SampleFormat(1, sampleRate)) { Frequency = Frequency };
            using var srcMulti = new SquareWaveSource(new SampleFormat(channels, sampleRate)) { Frequency = Frequency };
            using var filterMono = new BiQuadFilter(srcMono, parameter);
            using var filterMulti = new BiQuadFilter(srcMulti, parameter);
            float[] bufferMono = new float[128];
            float[] bufferMulti = new float[bufferMono.Length * channels];

            filterMono.Read(bufferMono);
            filterMulti.Read(bufferMulti);
            double sumdiff = 0;

            for (int i = 0; i < bufferMono.Length; i++)
            {
                float mono = bufferMono[i];
                float multi = bufferMulti[i * channels];
                float diff = mono - multi;
                sumdiff += MathF.Abs(diff);
                Console.WriteLine($"{mono}, {multi}, {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff}");
            double avgDiff = sumdiff / bufferMono.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, 1f - MathF.BitDecrement(1f));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BiQuadLPFChannelConsistency(int channels)
        {
            const int SampleRate = 48000;
            CheckChannelConsistency(SampleRate, channels, BiQuadParameter.CreateLPFParameter(SampleRate, 4000, Math.Sqrt(0.5)));
        }

        #endregion Multi-channel Consistency

        #region Intrinsics Consistency

        private static void CheckIntrinsicsConsistency(int sampleRate, int channels, BiQuadParameter parameter, X86Intrinsics x86Intrinsics, ArmIntrinsics armIntrinsics)
        {
            Console.WriteLine($"Enabled x86-64 intrinsics: {x86Intrinsics}");
            Console.WriteLine($"Enabled ARM intrinsics: {armIntrinsics}");
            if (!IntrinsicsUtils.ArmIntrinsics.HasAllFeatures(armIntrinsics))
            {
                Assert.Warn($"The Arm intrinsics \"{(armIntrinsics & IntrinsicsUtils.ArmIntrinsics) ^ armIntrinsics}\" are not supported on this machine!");
                return;
            }
            if (!IntrinsicsUtils.X86Intrinsics.HasAllFeatures(x86Intrinsics))
            {
                Assert.Warn($"The X86 intrinsics \"{(x86Intrinsics & IntrinsicsUtils.X86Intrinsics) ^ x86Intrinsics}\" are not supported on this machine!");
                return;
            }
            const int Frequency = 2000;
            var format = new SampleFormat(channels, sampleRate);
            using var srcNoIntrinsics = new FilterTestSignalSource(format) { Frequency = Frequency };
            using var srcIntrinsics = new FilterTestSignalSource(format) { Frequency = Frequency };
            using var filterNoIntrinsics = new BiQuadFilter(srcNoIntrinsics, parameter, false);
            using var filterIntrinsics = new BiQuadFilter(srcIntrinsics, parameter, true, x86Intrinsics, armIntrinsics);
            float[] bufferNoIntrinsics = new float[128];
            float[] bufferIntrinsics = new float[bufferNoIntrinsics.Length];

            _ = filterNoIntrinsics.Read(bufferNoIntrinsics);
            _ = filterIntrinsics.Read(bufferIntrinsics);
            NeumaierAccumulator sumdiff = default;
            for (int i = 0; i < bufferNoIntrinsics.Length; i++)
            {
                double simple = bufferNoIntrinsics[i];
                double optimized = bufferIntrinsics[i];
                double diff = simple - optimized;
                sumdiff += Math.Abs(diff);
                Console.WriteLine($"{simple}, {optimized}, {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            double avgDiff = sumdiff.Sum / bufferNoIntrinsics.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, -1f / short.MinValue);
        }

        [TestCase(1, (X86Intrinsics)X86IntrinsicsMask.Sse42)]
        [TestCase(1, (X86Intrinsics)X86IntrinsicsMask.Avx2)]
        [TestCase(2, (X86Intrinsics)X86IntrinsicsMask.Sse42)]
        [TestCase(2, (X86Intrinsics)X86IntrinsicsMask.Avx2)]
        /*[TestCase(3)]
        [TestCase(4)]*/
        [TestCase(5)]
        [TestCase(6)]
        public void BiQuadLPFIntrinsicsConsistency(int channels, X86Intrinsics x86Intrinsics = (X86Intrinsics)~0ul, ArmIntrinsics armIntrinsics = (ArmIntrinsics)~0ul)
        {
            const int SampleRate = 48000;
            if (x86Intrinsics == (X86Intrinsics)~0ul)
            {
                x86Intrinsics = IntrinsicsUtils.X86Intrinsics;
            }
            if (armIntrinsics == (ArmIntrinsics)~0ul)
            {
                armIntrinsics = IntrinsicsUtils.ArmIntrinsics;
            }
            CheckIntrinsicsConsistency(SampleRate, channels, BiQuadParameter.CreateLPFParameter(SampleRate, 4000, Math.Sqrt(0.5)), x86Intrinsics, armIntrinsics);
            Assert.Pass();
        }

        #endregion Intrinsics Consistency
    }
}
