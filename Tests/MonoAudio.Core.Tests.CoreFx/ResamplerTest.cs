using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Formats;
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
        }

        [Test]
        public void UpSamplingTwoFrameDump()
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
            resampler.Read(buffer);
            foreach (var item in buffer)
            {
                Console.WriteLine(item);
            }
            Assert.Pass();
        }

        [Test]
        public void UpSamplingLoadTest()
        {
            const int SourceSampleRate = 44100;
            const int DestinationSampleRate = 192000;
            const double destinationSampleRateD = (double)DestinationSampleRate;
            var src = new SinusoidSource(new SampleFormat(1, SourceSampleRate)) { Frequency = 6000 };
            var resampler = new SplineResampler(src, DestinationSampleRate);
            var buffer = new float[2048];
            //Warm up
            var sw = new Stopwatch();
            ulong samples = 0;
            sw.Start();
            do
            {
                samples += (ulong)resampler.Read(buffer);
            } while (sw.ElapsedMilliseconds < 200);
            sw.Stop();
            Console.WriteLine($"Samples read in warm up while {sw.Elapsed.TotalSeconds}[s]: {samples} samples(about {samples / destinationSampleRateD}[s])");
            Console.WriteLine($"Sample process rate: {samples / sw.Elapsed.TotalSeconds}[samples/s](about {samples / sw.Elapsed.TotalSeconds / destinationSampleRateD} times faster than real life)");
            samples = 0;
            sw.Reset();
            sw.Start();
            do
            {
                samples += (ulong)resampler.Read(buffer);
            } while (sw.ElapsedMilliseconds < 1000);
            sw.Stop();
            Console.WriteLine($"Samples read while {sw.Elapsed.TotalSeconds}[s]: {samples} samples(about {samples / destinationSampleRateD}[s])");
            Console.WriteLine($"Sample process rate: {samples / sw.Elapsed.TotalSeconds}[samples/s](about {samples / sw.Elapsed.TotalSeconds / destinationSampleRateD} times faster than real life)");
            Assert.Greater(samples, DestinationSampleRate);
        }
    }
}
