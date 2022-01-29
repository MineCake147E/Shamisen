using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class ReplaceNaNsWithBenchmarks
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
            MemoryMarshal.Cast<float, ulong>(bufferA.AsSpan()).FastFill(0x7fff_ffff_0000_0000ul);
        }
        [Benchmark]
        public void Avx() => AudioUtils.X86.ReplaceNaNsWithAvx(bufferDst, bufferA, 1.0f);

        [Benchmark]
        public void Fallback() => AudioUtils.Fallback.ReplaceNaNsWithFallback(bufferDst, bufferA, 1.0f);

        [Benchmark]
        public void Avx2() => AudioUtils.X86.ReplaceNaNsWithAvx2(bufferDst, bufferA, 1.0f);
    }
}
