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
    public class FastScalarMultiplyBenchmarks
    {
        private const int SampleRate = 192000;
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                //_ = AddColumn(new PlaybackSpeedColumn(frameSelector, a => SampleRate));
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }
        [Params(4095)]
        public int Frames { get; set; }
        [Params(1)]
        public int Channels { get; set; }

        [Params(32.0f, MathF.PI, 6.64613997E+35f)]
        public float Scale { get; set; }
        private float[] bufferDst;
        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames * Channels;
            bufferDst = new float[samples];
        }
        /*
        [Benchmark]
#pragma warning disable CS0618
        public void FastScalarMultiplyStandardVariableOld() => AudioUtils.FastScalarMultiplyStandardVariableOld(bufferDst.AsSpan(), Scale);
#pragma warning restore CS0618

        [Benchmark]
        public void FastScalarMultiplyStandardFixed() => AudioUtils.FastScalarMultiplyStandardFixed(bufferDst.AsSpan(), Scale);
        */
        [Benchmark]
        public void FastScalarMultiplyStandardVariable() => AudioUtils.FastScalarMultiply(bufferDst.AsSpan(), bufferDst.AsSpan(), Scale);
    }
}
