using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Synthesis;
using NUnit.Framework;

//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Diagnostics;
using MonoAudio.Filters;

namespace MonoAudio.Core.Tests.CoreFx
{
    [TestFixture]
    public class BiQuadFilterTest
    {
        #region Dump

        private void BiQuadFilterDump(int sampleRate, BiQuadParameter parameter)
        {
            var src = new SquareWaveSource(new SampleFormat(1, sampleRate)) { Frequency = 2000 };
            var filter = new BiQuadFilter(src, parameter);
            var buffer = new float[128];
            filter.Read(buffer);
            foreach (var item in buffer)
            {
                Console.WriteLine(item);    //dump the transient part
            }
            filter.Read(buffer);
            foreach (var item in buffer)
            {
                Console.WriteLine(item);
            }
            filter.Dispose();
        }

        [Test]
        public void BiQuadLPFTwoFrameDump()
        {
            const int SampleRate = 48000;

            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateLPFParameter(SampleRate, 4000, Math.Sqrt(0.5)));
            Assert.Pass();
        }

        [Test]
        public void BiQuadHPFTwoFrameDump()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateHPFParameter(SampleRate, 4000, Math.Sqrt(0.5)));
            Assert.Pass();
        }

        [Test]
        public void BiQuadBPFTwoFrameDumpFromBandWidth()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate,    //Equivalent to Q=5.1875164769281621706495731692014
                BiQuadParameter.CreateBPFParameterFromBandWidth(SampleRate, 6000, 0.25, BpfGainKind.ZeroDBPeakGain));
            Assert.Pass();
        }

        [Test]
        public void BiQuadBPFTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateBPFParameterFromQuality(SampleRate, 6000, 1, BpfGainKind.ZeroDBPeakGain));
            Assert.Pass();
        }

        [Test]
        public void BiQuadAPFTwoFrameDump()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateAPFParameter(SampleRate, 6000, 1));
            Assert.Pass();
        }

        [Test]
        public void BiQuadNotchTwoFrameDumpFromBandWidth()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateNotchFilterParameterFromBandWidth(SampleRate, 6000, 0.25));
            Assert.Pass();
        }

        [Test]
        public void BiQuadNotchTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateNotchFilterParameterFromQuality(SampleRate, 6000, 1));
            Assert.Pass();
        }

        [Test]
        public void BiQuadPeakingEQTwoFrameDumpFromBandWidth()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreatePeakingEqualizerParameterFromBandWidth(SampleRate, 6000, 0.25, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadPeakingEQTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreatePeakingEqualizerParameterFromQuality(SampleRate, 6000, 1, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadHSFTwoFrameDumpFromSlope()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateHighShelfFilterParameterFromSlope(SampleRate, 6000, 6, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadHSFTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateHighShelfFilterParameterFromQuality(SampleRate, 6000, 1, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadLSFTwoFrameDumpFromSlope()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateLowShelfFilterParameterFromSlope(SampleRate, 6000, 6, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadLSFTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateLowShelfFilterParameterFromQuality(SampleRate, 6000, 2, -6));
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
            var bufferMono = new float[128];
            var bufferMulti = new float[bufferMono.Length * channels];

            filterMono.Read(bufferMono);
            filterMulti.Read(bufferMulti);
            double sumdiff = 0;

            for (int i = 0; i < bufferMono.Length; i++)
            {
                var mono = bufferMono[i];
                var multi = bufferMulti[i * channels];
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

        private static void CheckIntrinsicsConsistency(int sampleRate, int channels, BiQuadParameter parameter)
        {
            const int Frequency = 2000;
            var format = new SampleFormat(channels, sampleRate);
            using var srcNoIntrinsics = new SquareWaveSource(format) { Frequency = Frequency };
            using var srcIntrinsics = new SquareWaveSource(format) { Frequency = Frequency };
            using var filterNoIntrinsics = new BiQuadFilter(srcNoIntrinsics, parameter, false);
            using var filterIntrinsics = new BiQuadFilter(srcIntrinsics, parameter, true);
            var bufferNoIntrinsics = new float[128 * channels];
            var bufferIntrinsics = new float[bufferNoIntrinsics.Length];

            filterNoIntrinsics.Read(bufferNoIntrinsics);
            filterIntrinsics.Read(bufferIntrinsics);
            double sumdiff = 0;
            for (int i = 0; i < bufferNoIntrinsics.Length; i++)
            {
                var simple = bufferNoIntrinsics[i];
                var optimized = bufferIntrinsics[i];
                float diff = simple - optimized;
                sumdiff += MathF.Abs(diff);
                Console.WriteLine($"{simple}, {optimized}, {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff}");
            double avgDiff = sumdiff / bufferNoIntrinsics.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, 1f - MathF.BitDecrement(1f));
        }

        [TestCase(1)]
        [TestCase(2)]
        /*[TestCase(3)]
        [TestCase(4)]*/
        [TestCase(5)]
        [TestCase(6)]
        public void BiQuadLPFIntrinsicsConsistency(int channels)
        {
            const int SampleRate = 48000;
            CheckIntrinsicsConsistency(SampleRate, channels, BiQuadParameter.CreateLPFParameter(SampleRate, 4000, Math.Sqrt(0.5)));
            Assert.Pass();
        }

        #endregion Intrinsics Consistency
    }
}
