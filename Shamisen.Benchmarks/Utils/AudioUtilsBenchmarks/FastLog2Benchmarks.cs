using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        public void FastLog2Order5FAvx2Fma() => AudioUtils.X86.FastLog2Order5FAvx2Fma(bufferDst, bufferA);

        [Benchmark]
        public void FastLog2Order5Avx2() => AudioUtils.X86.FastLog2Order5Avx2(bufferDst, bufferA);

        [Benchmark]
        public void FastLog2Order5Sse2() => AudioUtils.X86.FastLog2Order5Sse2(bufferDst, bufferA);

        [Benchmark]
        public void FastLog2Order5Fallback() => AudioUtils.Fallback.FastLog2Order5Fallback(bufferDst, bufferA);

        [Benchmark]
        public void MathFLog2()
        {
            var source = bufferA.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = MathF.Log2(Unsafe.Add(ref x9, i));
            }
        }
    }
}
