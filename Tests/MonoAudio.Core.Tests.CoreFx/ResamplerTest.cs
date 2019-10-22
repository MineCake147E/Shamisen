using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Synthesis;
using NUnit.Framework;
using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Diagnostics;

namespace MonoAudio.Core.Tests.CoreFx
{
    [TestFixture]
    public class ResamplerTest
    {
        [Test]
        public void UpSamplingDoesNotThrow()
        {
            const int Channels = 3;
            const int SourceSampleRate = 44100;
            const int DestinationSampleRate = 48000;
            var src = new SinusoidSource(new SampleFormat(Channels, SourceSampleRate)) { Frequency = 20000 };
            var resampler = new SplineResampler(src, DestinationSampleRate);
            var buffer = new float[Channels * 1024];
            Assert.DoesNotThrow(() =>
            {
                resampler.Read(buffer);
            });
            resampler.Dispose();
        }

        [Test]
        public void DownSamplingDoesNotThrow()
        {
            const int Channels = 3;
            const int SourceSampleRate = 48000;
            const int DestinationSampleRate = 44100;
            var src = new SinusoidSource(new SampleFormat(Channels, SourceSampleRate)) { Frequency = 20000 };
            var resampler = new SplineResampler(src, DestinationSampleRate);
            var buffer = new float[Channels * 1024];
            Assert.DoesNotThrow(() =>
            {
                resampler.Read(buffer);
            });
            resampler.Dispose();
        }

        [Test]
        public void UpSamplingFrameDump()
        {
            const int SourceSampleRate = 44100;
            const int DestinationSampleRate = 192000;
            var src = new SinusoidSource(new SampleFormat(1, SourceSampleRate)) { Frequency = 6000 };
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
            var src = new SinusoidSource(new SampleFormat(1, sourceSampleRate)) { Frequency = 6000 };
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

        [TestCase(1)]   //Monaural
        [TestCase(2)]   //Stereo
        [TestCase(3)]   //2.1ch / 3ch Surround
        [TestCase(4)]   //3.1ch / 4ch Surround
        [TestCase(5)]   //4.1ch / 5ch Surround
        [TestCase(6)]   //5.1ch Surround
        [TestCase(7)]   //5.2ch Surround
        [TestCase(8)]   //7.1ch Surround
        [TestCase(9)]   //7.2ch Surround
        [TestCase(10)]   //9.1ch Surround
        public void UpSamplingLoadTest(int Channels)
        {
            const int SourceSampleRate = 44100;
            const int DestinationSampleRate = 192000;
            const double DestinationSampleRateD = DestinationSampleRate;
            double channelsInverse = 1.0 / Channels;
            var src = new SinusoidSource(new SampleFormat(Channels, SourceSampleRate)) { Frequency = 6000 };
            var resampler = new SplineResampler(src, DestinationSampleRate);
            var buffer = new float[2048];
            //Warm up
            var sw = new Stopwatch();
            ulong samples = 0;
            sw.Start();
            do
            {
                samples += (ulong)resampler.Read(buffer);
            } while (sw.ElapsedMilliseconds < 1000);
            sw.Stop();
            Console.WriteLine($"Samples read in warm up while {sw.Elapsed.TotalSeconds}[s]: {samples * channelsInverse} samples(about {samples * channelsInverse / DestinationSampleRateD}[s])");
            Console.WriteLine($"Sample process rate: {samples * channelsInverse / sw.Elapsed.TotalSeconds}[samples/s](about {samples * channelsInverse / sw.Elapsed.TotalSeconds / DestinationSampleRateD} times faster than real life)");
            samples = 0;
            sw.Reset();
            sw.Start();
            do
            {
                samples += (ulong)resampler.Read(buffer);
            } while (sw.ElapsedMilliseconds < 2000);
            sw.Stop();
            Console.WriteLine($"Samples read while {sw.Elapsed.TotalSeconds}[s]: {samples * channelsInverse} samples(about {samples * channelsInverse / DestinationSampleRateD}[s])");
            Console.WriteLine($"Sample process rate: {samples * channelsInverse / sw.Elapsed.TotalSeconds}[samples/s](about {samples * channelsInverse / sw.Elapsed.TotalSeconds / DestinationSampleRateD} times faster than real life)");
            Assert.Greater(samples, DestinationSampleRate);
            resampler.Dispose();
        }
    }
}
