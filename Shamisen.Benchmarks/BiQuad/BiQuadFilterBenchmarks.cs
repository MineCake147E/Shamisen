using System;
using System.Linq;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks.BiQuad
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: 16)]
    public class BiQuadFilterBenchmarks
    {
        private IReadableAudioSource<float, SampleFormat> source;
        private BiQuadFilter filter;
        private float[] buffer;
        private const int SampleRate = 192000;
        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new FrameThroughputColumn(a => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value));
            }
        }
        [Params(1, 2, 3, 4)]
        public int Channels { get; set; }
        [Params(8191)]
        public int Frames { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<float, SampleFormat>(new SampleFormat(Channels, SampleRate));
            filter = new BiQuadFilter(source, BiQuadParameter.CreateLPFParameter(SampleRate, 48000, 1));
            buffer = new float[Frames * Channels];
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
