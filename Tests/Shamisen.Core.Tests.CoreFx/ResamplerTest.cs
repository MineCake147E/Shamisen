using System;
using System.Buffers;
using System.Collections.Generic;
//using CSCodec.Filters.Transformation;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

using NUnit.Framework;

using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Data;
using Shamisen.Filters;
using Shamisen.Filters.Buffering;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class ResamplerTest
    {
        private const double Freq = 523;

        [Test]
        public void UpSamplingDoesNotThrow()
        {
            const int Channels = 3;
            const int SourceSampleRate = 44100;
            const int DestinationSampleRate = 48000;
            var src = new SinusoidSource(new SampleFormat(Channels, SourceSampleRate)) { Frequency = Freq };
            var resampler = new SplineResampler(src, DestinationSampleRate);
            var buffer = new float[Channels * 1024];
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
            var buffer = new float[Channels * 1024];
            Assert.DoesNotThrow(() => resampler.Read(buffer));
            resampler.Dispose();
        }

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
            if (Sse2.IsSupported)
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
                Assert.Warn("SSE2 is not supported!");
            }
        }

        [Test]
        public void UpSamplingFrameDump()
        {
            const int SourceSampleRate = 44100;
            const int DestinationSampleRate = 192000;
            var src = new SinusoidSource(new SampleFormat(1, SourceSampleRate)) { Frequency = Freq };
            var resampler = new SplineResampler(src, DestinationSampleRate);
            var buffer = new float[256];
            resampler.Read(buffer); //Trash the data because the first one contains transient part.
            resampler.Read(buffer);
            foreach (var item in buffer)
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
            var buffer = new float[256];
            resampler.Read(buffer); //Trash the data because the first one contains transient part.
            resampler.Read(buffer);
            foreach (var item in buffer)
            {
                Console.WriteLine(item);
            }
            resampler.Read(buffer);
            foreach (var item in buffer)
            {
                Console.WriteLine(item);
            }
            Assert.Pass();
            resampler.Dispose();
        }

        private static IEnumerable<TestCaseData> UpSamplingManyFrameDumpTestCaseSource()
        {
            var ratios = new (int before, int after)[]
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
            var channels = Enumerable.Range(1, 8)/*.Concat(new int[] { })*/.ToArray();
            return channels.SelectMany(chs => ratios.Select(r => new TestCaseData(chs, r.before, r.after, 1024, 64)));
        }

        [TestCaseSource(nameof(UpSamplingManyFrameDumpTestCaseSource))]
        [TestCase(2, 24000, 192000, 1021)]      //Odd length of blocks
        [TestCase(2, 23000, 192000, 192, 4096)] //Small blocks
        [TestCase(2, 48000, 192000, 1025)]      //Odd length of blocks
        public void UpSamplingManyFrameDump(int channels, int sourceSampleRate, int destinationSampleRate, int frameLen = 1024, int framesToWrite = 64)
        {
            var src = new SinusoidSource(new SampleFormat(channels, sourceSampleRate)) { Frequency = Freq };
            var resampler = new SplineResampler(src, destinationSampleRate);
            var path = new FileInfo($"./dumps/SplineResamplerDump_{channels}ch_{sourceSampleRate}to{destinationSampleRate}_{frameLen}fpb_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}.wav");
            Console.WriteLine(path.FullName);
            if (!Directory.Exists("./dumps")) _ = Directory.CreateDirectory("./dumps");
            using var dc = new AudioCache<float, SampleFormat>(resampler.Format);
            float[] buffer = new float[(ulong)frameLen * (ulong)channels];
            for (int i = 0; i < framesToWrite; i++)
            {
                var q = resampler.Read(buffer);
                dc.Write(buffer.AsSpan(0, q.Length));
            }
            var trunc = new LengthTruncationSource<float, SampleFormat>(dc, (ulong)framesToWrite * (ulong)frameLen);
            using (var ssink = new StreamDataSink(path.OpenWrite(), false, true))
            {
                Assert.DoesNotThrow(() =>
                SimpleWaveEncoder.Instance.Encode(new SampleToFloat32Converter(trunc), ssink));
            }
            Assert.Pass();
            resampler.Dispose();
        }

    }
}
