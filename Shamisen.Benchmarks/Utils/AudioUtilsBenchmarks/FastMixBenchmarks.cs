using System;
using System.Linq;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Utils;

namespace Shamisen.Benchmarks.Utils.AudioUtilsBenchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
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
        [Params(8191)]
        public int Frames { get; set; }
        [Params(1)]
        public int Channels { get; set; }

        [Params(MathF.PI/*, 1.17549421069E-38f*/)]
        public float ScaleA { get; set; }

        [Params(MathF.E)]
        public float ScaleB { get; set; }

        private float[] bufferDst, bufferA, bufferB;
        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames * Channels;
            bufferDst = new float[samples];
            bufferA = new float[samples];
            bufferB = new float[samples];
            bufferA.AsSpan().FastFill(1.0f);
            bufferB.AsSpan().FastFill(-1.0f);
        }

        [Benchmark]
        public void StandardFixed() => AudioUtils.FastMixStandardFixed(bufferDst.AsSpan(), bufferA.AsSpan(), ScaleA, bufferB.AsSpan(), ScaleB);

        [Benchmark]
        public void StandardVariable() => AudioUtils.FastMixStandardVariable(bufferDst.AsSpan(), bufferA.AsSpan(), ScaleA, bufferB.AsSpan(), ScaleB);
    }
}
