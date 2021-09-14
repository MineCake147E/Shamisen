using System;
using System.Linq;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Utils.SpanExtensionsBenchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class FastMixBenchmarks
    {
        private const int SampleRate = 192000;
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
                _ = AddColumn(new PlaybackSpeedColumn(
                    frameSelector,
                    a => SampleRate));
            }

        }
        [Params(1024, 1441)]
        public int Frames { get; set; }
        [Params(1)]
        public int Channels { get; set; }

        [Params(MathF.PI)]
        public float ScaleA { get; set; }

        [Params(MathF.E)]
        public float ScaleB { get; set; }

        private float[] bufferDst, bufferA, bufferB;
        [GlobalSetup]
        public void Setup()
        {
            int samples = Frames * Channels;
            bufferDst = new float[samples];
            bufferA = new float[samples];
            bufferB = new float[samples];

        }

        [Benchmark]
        public void Avx()
        {
            SpanExtensions.FastMixAvx(bufferDst.AsSpan(), bufferA.AsSpan(), ScaleA, bufferB.AsSpan(), ScaleB);
        }
        [Benchmark]
        public void Sse()
        {
            SpanExtensions.FastMixSse(bufferDst.AsSpan(), bufferA.AsSpan(), ScaleA, bufferB.AsSpan(), ScaleB);
        }

        [Benchmark]
        public void Standard()
        {
            SpanExtensions.FastMixStandard(bufferDst.AsSpan(), bufferA.AsSpan(), ScaleA, bufferB.AsSpan(), ScaleB);
        }
    }
}
