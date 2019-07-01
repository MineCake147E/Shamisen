using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Formats;
using MonoAudio.Synthesis;
using NUnit.Framework;
using CSCodec.Filters.Transformation;
using System.Numerics;

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
    }
}
