using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Utils;
using Shamisen.Utils.Intrinsics;

namespace Shamisen.Benchmarks.Utils.AudioUtilsBenchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class FastLog2Benchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }
        [Params(131071)]
        public int Frames { get; set; }
        private float[] bufferDst, bufferA, bufferB;

        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames;
            bufferDst = new float[samples];
            bufferA = new float[samples];
            bufferA.AsSpan().FastFill(1.5f);
        }

        [Benchmark]
        public void Avx2Fma() => AudioUtils.X86.FastLog2Order5FAvx2Fma(bufferDst, bufferA);

        [Benchmark]
        public void Avx2() => AudioUtils.X86.FastLog2Order5Avx2(bufferDst, bufferA);

        [Benchmark]
        public void Sse2() => AudioUtils.X86.FastLog2Order5Sse2(bufferDst, bufferA);

        [Benchmark]
        public void Fallback() => AudioUtils.Fallback.FastLog2Order5Fallback(bufferDst, bufferA);
    }
}
