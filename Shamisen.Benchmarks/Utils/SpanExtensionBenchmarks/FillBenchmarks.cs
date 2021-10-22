using System;
using System.Linq;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Utils.SpanExtensionBenchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class FillBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }

        [Params(65535, 65536)]
        public int Frames { get; set; }

        [Params(1)]
        public int Channels { get; set; }

        [Params(MathF.PI)]
        public float Value { get; set; }
        private float[] bufferDst;
        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames * Channels;
            bufferDst = new float[samples];
        }
        [Benchmark]
        public void FastFill() => SpanExtensions.FastFill(bufferDst, Value);

        [Benchmark]
        public void QuickFill() => SpanExtensions.QuickFill(bufferDst, Value);
    }
}
