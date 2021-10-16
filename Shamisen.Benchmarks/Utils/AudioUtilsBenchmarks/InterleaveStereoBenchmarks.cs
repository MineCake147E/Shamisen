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
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class InterleaveStereoBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }
        [Params(8191)]
        public int Frames { get; set; }
        private int[] bufferDst, bufferA, bufferB;
        [GlobalSetup]
        public void Setup()
        {
            int samples = Frames;
            bufferDst = new int[samples * 2];
            bufferA = new int[samples];
            bufferB = new int[samples];
        }

        [Benchmark]
        public void Avx() => AudioUtils.X86.InterleaveStereoInt32Avx(bufferDst.AsSpan(), bufferA.AsSpan(), bufferB.AsSpan());
        [Benchmark]
        public void Sse() => AudioUtils.X86.InterleaveStereoInt32Sse(bufferDst.AsSpan(), bufferA.AsSpan(), bufferB.AsSpan());
        [Benchmark]
        public void Standard() => AudioUtils.Fallback.InterleaveStereoInt32(bufferDst.AsSpan(), bufferA.AsSpan(), bufferB.AsSpan());
    }
}
