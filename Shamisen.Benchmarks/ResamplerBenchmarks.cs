using System;
using System.Collections.Generic;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net50)]
    [DisassemblyDiagnoser(maxDepth: 16)]
    //[SimpleJob(RuntimeMoniker.Mono, baseline: true)]
    public class ResamplerBenchmarks
    {
        private SplineResampler resampler;
        private IReadableAudioSource<float, SampleFormat> source;
        private float[] buffer;

        [ParamsSource(nameof(ValuesForConversionRatio))]
        public (int before, int after) ConversionRatio { get; set; }

        public IEnumerable<(int, int)> ValuesForConversionRatio => new[] { (44100, 192000), (48000, 192000), (24000, 154320), (96000, 192000) };

        [Params(1, 2, 3, 4, 8)]
        public int Channels { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<float, SampleFormat>(new SampleFormat(Channels, ConversionRatio.before));
            resampler = new SplineResampler(source, ConversionRatio.after);
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
    }
}
