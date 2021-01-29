using System;
using System.Collections.Generic;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Filters;
using MonoAudio.Synthesis;

namespace MonoAudio.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [DisassemblyDiagnoser(maxDepth: 16)]
    public class BiQuadBenchmarks
    {
        private IReadableAudioSource<float, SampleFormat> source;
        private BiQuadFilter filter;
        private float[] buffer;
        private const int SampleRate = 192000;

        [Params(1, 2, 4, 5)]
        public int Channels { get; set; }

        [Params(true)]
        public bool EnableIntrinsics { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<float, SampleFormat>(new SampleFormat(Channels, SampleRate));
            filter = new BiQuadFilter(source, BiQuadParameter.CreateLPFParameter(SampleRate, 48000, 1), EnableIntrinsics);
            buffer = new float[1024 * Channels];
        }

        [Benchmark]
        public void BiQuadFilter()
        {
            var span = buffer.AsSpan();
            _ = filter.Read(span);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            filter?.Dispose();
            source?.Dispose();
            buffer = null;
        }
    }
}
