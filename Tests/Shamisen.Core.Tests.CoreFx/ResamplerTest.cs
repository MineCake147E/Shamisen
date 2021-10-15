using System;
using System.Buffers;
using System.Collections.Generic;
//using CSCodec.Filters.Transformation;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

using FastEnumUtility;

using NUnit.Framework;

using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Data;
using Shamisen.Filters;
using Shamisen.Filters.Buffering;
using Shamisen.Optimization;
using Shamisen.Synthesis;
using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class ResamplerTest
    {
        private const double Freq = 523;
        #region DoesNotThrow

        [Test]
        public void UpSamplingDoesNotThrow()
        {
            const int Channels = 3;
            const int SourceSampleRate = 44100;
            const int DestinationSampleRate = 48000;
            var src = new SinusoidSource(new SampleFormat(Channels, SourceSampleRate)) { Frequency = Freq };
            var resampler = new SplineResampler(src, DestinationSampleRate);
            float[] buffer = new float[Channels * 1024];
            Assert.DoesNotThrow(() => resampler.Read(buffer));
            resampler.Dispose();
        }

        [Test]
        public void DownSamplingDoesNotThrow()
        {
            const int Channels = 3;
            const int SourceSampleRate = 48000;
            const int DestinationSampleRate = 44100;
            var src = new SinusoidSource(new SampleFormat(Channels, SourceSampleRate)) { Frequency = Freq };
            var resampler = new SplineResampler(src, DestinationSampleRate);
            float[] buffer = new float[Channels * 1024];
            Assert.DoesNotThrow(() => resampler.Read(buffer));
            resampler.Dispose();
        }
        #endregion
        #region UpSamplingCoeffsIntrinsicsConsistency

        [TestCase(2, 0.5f)]
        [TestCase(3, 1.0f / 3)]
        [TestCase(4, 0.25f)]
        [TestCase(1024, 1.0f / 1024)]
        [TestCase(1027, 1.0f / 1027)]
        public void UpSamplingCoeffsSse2Consistency(int length, float rateMulInverse)
        {
            if (Sse2.IsSupported)
            {
                using var std = new PooledArray<Vector4>(length);
                using var cmp = new PooledArray<Vector4>(length);
                SplineResampler.GenerateCoeffsStandard(std.Span, rateMulInverse);
                SplineResampler.GenerateCoeffsSse2(cmp.Span, rateMulInverse);
                NeumaierAccumulator sumd = new(0, 0);
                var sstd = std.Span;
                var scmp = cmp.Span;
                for (int i = 0; i < sstd.Length && i < scmp.Length; i++)
                {
                    var d = sstd[i] - scmp[i];
                    sumd += d.LengthSquared();
                }
                Assert.AreEqual(0.0, sumd.Sum, 1.0 / (1u << 24));
                Console.WriteLine($"Difference: {sumd.Sum}");
            }
            else
            {
                Assert.Warn("SSE2 is not supported!");
            }
        }

        [TestCase(2, 0.5f)]
        [TestCase(3, 1.0f / 3)]
        [TestCase(4, 0.25f)]
        [TestCase(1024, 1.0f / 1024)]
        [TestCase(1027, 1.0f / 1027)]
        public void UpSamplingCoeffsFma128Consistency(int length, float rateMulInverse)
        {
            if (Fma.IsSupported)
            {
                using var std = new PooledArray<Vector4>(length);
                using var cmp = new PooledArray<Vector4>(length);
                SplineResampler.GenerateCoeffsStandard(std.Span, rateMulInverse);
                SplineResampler.GenerateCoeffsFma128(cmp.Span, rateMulInverse);
                NeumaierAccumulator sumd = new(0, 0);
                var sstd = std.Span;
                var scmp = cmp.Span;
                for (int i = 0; i < sstd.Length && i < scmp.Length; i++)
                {
                    var d = sstd[i] - scmp[i];
                    sumd += d.LengthSquared();
                }
                Assert.AreEqual(0.0, sumd.Sum, 1.0 / (1u << 24));
                Console.WriteLine($"Difference: {sumd.Sum}");
            }
            else
            {
                Assert.Warn("FMA is not supported!");
            }
        }
        #endregion
        #region UpSamplingIntrinsicsConsistency
        private static IEnumerable<TestCaseData> UpSamplingIntrinsicsConsistencyTestCaseSource()
        {
            var ratios = GenerateConversionRatios();
            var channelsWithIntrinsics = Enumerable.Range(1, 2);
            var x86Intr = FastEnum.GetValues<X86IntrinsicsMask>().Where(a => a != X86IntrinsicsMask.None && IntrinsicsUtils.X86Intrinsics.HasAllFeatures(a)).Select(a => (X86Intrinsics)a);
            var armIntr = FastEnum.GetValues<ArmIntrinsicsMask>().Where(a => a != ArmIntrinsicsMask.None && IntrinsicsUtils.ArmIntrinsics.HasAllFeatures(a)).Select(a => (ArmIntrinsics)a);
            var chsr = channelsWithIntrinsics.SelectMany(chs => ratios.Select(r => (chs, r.before, r.after))).ToArray();
            var x86Cases = x86Intr.SelectMany(intr => chsr.Select(c => new TestCaseData(c.chs, c.before, c.after, intr, ArmIntrinsics.None)));
            var armCases = armIntr.SelectMany(intr => chsr.Select(c => new TestCaseData(c.chs, c.before, c.after, X86Intrinsics.None, intr)));
            return x86Cases.Concat(armCases);
        }

        [TestCaseSource(nameof(UpSamplingIntrinsicsConsistencyTestCaseSource))]
        public void UpSamplingIntrinsicsConsistency(int channels, int sourceSampleRate = 48000, int destinationSampleRate = 192000, X86Intrinsics x86Intrinsics = (X86Intrinsics)~0ul, ArmIntrinsics armIntrinsics = (ArmIntrinsics)~0ul)
        {
            if (x86Intrinsics == (X86Intrinsics)~0ul)
            {
                x86Intrinsics = IntrinsicsUtils.X86Intrinsics;
            }
            if (armIntrinsics == (ArmIntrinsics)~0ul)
            {
                armIntrinsics = IntrinsicsUtils.ArmIntrinsics;
            }
            CheckIntrinsicsConsistency(sourceSampleRate, channels, destinationSampleRate, x86Intrinsics, armIntrinsics);
            Assert.Pass();
        }

        private static void CheckIntrinsicsConsistency(int sourceSampleRate, int channels, int destinationSampleRate, X86Intrinsics x86Intrinsics, ArmIntrinsics armIntrinsics)
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
            var format = new SampleFormat(channels, sourceSampleRate);
            using var srcNoIntrinsics = new FilterTestSignalSource(format) { Frequency = Frequency };
            using var srcIntrinsics = new FilterTestSignalSource(format) { Frequency = Frequency };
            using var filterNoIntrinsics = new SplineResampler(srcNoIntrinsics, destinationSampleRate, X86Intrinsics.None);
            using var filterIntrinsics = new SplineResampler(srcIntrinsics, destinationSampleRate, x86Intrinsics);
            float[] bufferNoIntrinsics = new float[16383 * channels];
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
            }
            Console.WriteLine($"Total difference for 1st block: {sumdiff.Sum}");
            double avgDiff = sumdiff.Sum / bufferNoIntrinsics.Length;
            Console.WriteLine($"Average difference for 1st block: {avgDiff}");
            if (avgDiff != 0.0)
            {
                float[] bufferStereo = new float[bufferNoIntrinsics.Length * 2];
                //Even channels contain bufferNoIntrinsics, and odd channels contain bufferIntrinsics
                AudioUtils.InterleaveStereo(MemoryMarshal.Cast<float, int>(bufferStereo), MemoryMarshal.Cast<float, int>(bufferNoIntrinsics), MemoryMarshal.Cast<float, int>(bufferIntrinsics));
                using var dc = new AudioCache<float, SampleFormat>(new SampleFormat(filterNoIntrinsics.Format.Channels * 2, filterNoIntrinsics.Format.SampleRate));
                dc.Write(bufferStereo);
                TestHelper.DumpSamples(dc, $"CheckIntrinsicsConsistencyDifferenceDump_{channels}ch_{sourceSampleRate}to{destinationSampleRate}_{x86Intrinsics}_{armIntrinsics}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}");
                Assert.AreEqual(0.0, avgDiff);
            }
            _ = filterNoIntrinsics.Read(bufferNoIntrinsics);
            _ = filterIntrinsics.Read(bufferIntrinsics);
            NeumaierAccumulator sumdiff2 = default;
            for (int i = 0; i < bufferNoIntrinsics.Length; i++)
            {
                double simple = bufferNoIntrinsics[i];
                double optimized = bufferIntrinsics[i];
                double diff = simple - optimized;
                sumdiff2 += Math.Abs(diff);
            }
            Console.WriteLine($"Total difference for 2nd block: {sumdiff2.Sum}");
            double avgDiff2 = sumdiff2.Sum / bufferNoIntrinsics.Length;
            Console.WriteLine($"Average difference for 2nd block: {avgDiff2}");
            if (avgDiff2 != 0.0)
            {
                float[] bufferStereo = new float[bufferNoIntrinsics.Length * 2];
                //Even channels contain bufferNoIntrinsics, and odd channels contain bufferIntrinsics
                AudioUtils.InterleaveStereo(MemoryMarshal.Cast<float, int>(bufferStereo), MemoryMarshal.Cast<float, int>(bufferNoIntrinsics), MemoryMarshal.Cast<float, int>(bufferIntrinsics));
                using var dc = new AudioCache<float, SampleFormat>(new SampleFormat(filterNoIntrinsics.Format.Channels * 2, filterNoIntrinsics.Format.SampleRate));
                dc.Write(bufferStereo);
                TestHelper.DumpSamples(dc, $"CheckIntrinsicsConsistencyDifferenceDump_{channels}ch_{sourceSampleRate}to{destinationSampleRate}_{x86Intrinsics}_{armIntrinsics}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}");
                Assert.AreEqual(0.0, avgDiff2);
            }

        }
        #endregion
        #region UpSamplingChannelsConsistency
        private static IEnumerable<TestCaseData> UpSamplingChannelConsistencyTestCaseSource()
        {
            var ratios = GenerateConversionRatios();
            var channelsWithIntrinsics = Enumerable.Range(2, 16);
            var chsr = channelsWithIntrinsics.SelectMany(chs => ratios.Select(r => (chs, r.before, r.after))).ToArray();
            return chsr.Select(c => new TestCaseData(c.chs, c.before, c.after));
        }
        private static void CheckChannelConsistency(int sourceSampleRate, int channels, int destinationSampleRate)
        {
            const int Frequency = 2000;
            using var srcMono = new SawtoothWaveSource(new SampleFormat(1, sourceSampleRate)) { Frequency = Frequency };
            using var srcMulti = new SawtoothWaveSource(new SampleFormat(channels, sourceSampleRate)) { Frequency = Frequency };
            using var filterMono = new SplineResampler(srcMono, destinationSampleRate);
            using var filterMulti = new SplineResampler(srcMulti, destinationSampleRate);
            float[] bufferMono = new float[255];
            float[] bufferMulti = new float[bufferMono.Length * channels];
            using var dcMono = new AudioCache<float, SampleFormat>(filterMono.Format);
            using var dcMulti = new AudioCache<float, SampleFormat>(filterMulti.Format);
            _ = filterMono.Read(bufferMono);
            _ = filterMulti.Read(bufferMulti);
            dcMono.Write(bufferMono);
            dcMulti.Write(bufferMulti);
            var sumdiff = CheckFrame(channels, bufferMono, bufferMulti);
            Console.WriteLine($"1st frame:");
            double avgDiff = WriteDifference(bufferMono, sumdiff);
            _ = filterMono.Read(bufferMono);
            _ = filterMulti.Read(bufferMulti);
            dcMono.Write(bufferMono);
            dcMulti.Write(bufferMulti);
            var sumdiff2 = CheckFrame(channels, bufferMono, bufferMulti);
            Console.WriteLine($"2nd frame:");
            double avgDiff2 = WriteDifference(bufferMono, sumdiff);
            if (avgDiff != 0 || avgDiff2 != 0)
            {
                TestHelper.DumpSamples(dcMono, $"CheckChannelConsistencyExpectedDump_{channels}ch_{sourceSampleRate}to{destinationSampleRate}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}");
                TestHelper.DumpSamples(dcMulti, $"CheckChannelConsistencyActualDump_{channels}ch_{sourceSampleRate}to{destinationSampleRate}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}");
            }
            Assert.AreEqual(0f, avgDiff);
            Assert.AreEqual(0f, avgDiff2);
        }

        private static double WriteDifference(float[] bufferMono, NeumaierAccumulator sumdiff)
        {
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            double avgDiff = sumdiff.Sum / bufferMono.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            return avgDiff;
        }

        private static NeumaierAccumulator CheckFrame(int channels, float[] bufferMono, float[] bufferMulti)
        {
            NeumaierAccumulator sumdiff = default;
            for (int i = 0; i < bufferMono.Length; i++)
            {
                float mono = bufferMono[i];
                float multi = bufferMulti[i * channels];
                float diff = mono - multi;
                sumdiff += MathF.Abs(diff);
                //Console.WriteLine($"{mono}, {multi}, {diff}");
            }
            return sumdiff;
        }

        [TestCaseSource(nameof(UpSamplingChannelConsistencyTestCaseSource))]
        public void UpSamplingChannelConsistency(int channels, int sourceSampleRate = 176400, int destinationSampleRate = 192000) => CheckChannelConsistency(sourceSampleRate, channels, destinationSampleRate);
        #endregion
        #region Dump

        [Test]
        public void UpSamplingFrameDump()
        {
            const int SourceSampleRate = 44100;
            const int DestinationSampleRate = 192000;
            var src = new SinusoidSource(new SampleFormat(1, SourceSampleRate)) { Frequency = Freq };
            var resampler = new SplineResampler(src, DestinationSampleRate);
            float[] buffer = new float[256];
            _ = resampler.Read(buffer); //Trash the data because the first one contains transient part.
            _ = resampler.Read(buffer);
            foreach (float item in buffer)
            {
                Console.WriteLine(item);
            }
            Assert.Pass();
            resampler.Dispose();
        }

        [TestCase(44100, 192000)]
        [TestCase(48000, 192000)]
        [TestCase(24000, 154320)]
        [TestCase(96000, 192000)]
        public void UpSamplingTwoFrameDump(int sourceSampleRate, int destinationSampleRate)
        {
            var src = new SinusoidSource(new SampleFormat(1, sourceSampleRate)) { Frequency = Freq };
            var resampler = new SplineResampler(src, destinationSampleRate);
            float[] buffer = new float[256];
            _ = resampler.Read(buffer); //Trash the data because the first one contains transient part.
            _ = resampler.Read(buffer);
            foreach (float item in buffer)
            {
                Console.WriteLine(item);
            }
            _ = resampler.Read(buffer);
            foreach (float item in buffer)
            {
                Console.WriteLine(item);
            }
            Assert.Pass();
            resampler.Dispose();
        }

        private static IEnumerable<TestCaseData> UpSamplingManyFrameDumpTestCaseSource()
        {
            var ratios = GenerateConversionRatios();
            int[] channels = Enumerable.Range(1, 8)/*.Concat(new int[] { })*/.ToArray();
            return channels.SelectMany(chs => ratios.Select(r => new TestCaseData(chs, r.before, r.after, 1024, 64)));
        }

        private static (int before, int after)[] GenerateConversionRatios() => new (int before, int after)[]
            {
                (8000, 192000),  	//Smoothness test
                (24000, 154320),  	//CachedWrappedOdd
                (44100, 154320),  	//Direct
                (44100, 192000),  	//CachedWrappedEven
                (44100, 48000),  	//CachedDirect
                (48000, 192000),  	//CachedDirect QuadrupleRate
                (96000, 192000),  	//CachedDirect DoubleRate
                (64000, 192000),  	//CachedDirect IntegerRate
            };

        [TestCaseSource(nameof(UpSamplingManyFrameDumpTestCaseSource))]
        [TestCase(2, 24000, 192000, 1021)]      //Odd length of blocks
        [TestCase(2, 23000, 192000, 192, 4096)] //Small blocks
        [TestCase(2, 48000, 192000, 1025)]      //Odd length of blocks
        public void UpSamplingManyFrameDump(int channels, int sourceSampleRate, int destinationSampleRate, int frameLen = 1024, int framesToWrite = 64)
        {
            var src = new SinusoidSource(new SampleFormat(channels, sourceSampleRate)) { Frequency = Freq };
            var resampler = new SplineResampler(src, destinationSampleRate);
            string path = $"SplineResamplerDump_{channels}ch_{sourceSampleRate}to{destinationSampleRate}_{frameLen}fpb_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}";
            TestHelper.DumpSampleSource(frameLen, framesToWrite, resampler, path);
            resampler.Dispose();
            Assert.Pass();
        }

        #endregion
    }
}
