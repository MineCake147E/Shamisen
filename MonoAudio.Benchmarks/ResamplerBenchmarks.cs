using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Synthesis;

namespace MonoAudio.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp22)]
    [SimpleJob(RuntimeMoniker.Mono, baseline: true)]
    public class ResamplerBenchmarks
    {
        private SplineResampler resampler;
        private SquareWaveSource source;
        private float[] buffer;

        [GlobalSetup]
        public void Setup()
        {
            source = new SquareWaveSource(new SampleFormat(2, 44100))
            {
                Frequency = 6000
            };
            resampler = new SplineResampler(source, 192000);
            buffer = new float[2560];
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            resampler?.Dispose();
            source?.Dispose();
            buffer = null;
        }

        [Benchmark]
        public void SplineResampler()
        {
            var span = buffer.AsSpan();
            _ = resampler.Read(span);
        }

        [Benchmark(Baseline = true)]
        public void InternalGenerate()
        {
            var span = buffer.AsSpan().Slice(588);
            _ = source.Read(span);
        }
    }
}