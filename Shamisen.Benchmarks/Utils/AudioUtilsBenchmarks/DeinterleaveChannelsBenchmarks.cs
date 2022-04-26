using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Utils;

namespace Shamisen.Benchmarks.Utils.AudioUtilsBenchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class DeinterleaveChannelsBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }
        [Params(65535)]
        public int Frames { get; set; }

        [Params(2, 3, 4, 9)]
        public int Channels { get; set; }
        private float[] bufferDst, bufferA, bufferB;

        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames * Channels;
            bufferDst = new float[samples];
            bufferA = new float[samples];
            bufferA.AsSpan().FastFill(1.5f);
        }

        [Benchmark]
        public void Avx2() => AudioUtils.X86.DeinterleaveChannelsSingleAvx2(bufferDst, bufferA, Channels, Frames);

        [Benchmark]
        public void Fallback() => AudioUtils.Fallback.DeinterleaveChannelsSingleFallback(bufferDst, bufferA, Channels, Frames);
    }
}
