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

namespace Shamisen.Benchmarks.Utils.FastMathBenchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [AllCategoriesFilter("Fast", "AsNormal")]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class Log2Benchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }

        [Params(65536)]
        public int Frames { get; set; }
        private float[] bufferDst, bufferSrc;
        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames;
            var b = bufferDst = new float[samples];
            var s = bufferSrc = new float[b.Length];
            var f = 1.0f / s.Length;
            for (var i = 0; i < s.Length; i++)
            {
                s[i] = i * f;
            }
        }

        [BenchmarkCategory("Standard")]
        [Benchmark(Baseline = true)]
        public void MathFLog2()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = MathF.Log2(Unsafe.Add(ref x9, i));
            }
        }

        [BenchmarkCategory("Standard", "Double")]
        [Benchmark]
        public void MathLog2()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = (float)Math.Log2(Unsafe.Add(ref x9, i));
            }
        }

        [BenchmarkCategory("Fast", "AsNormal")]
        [Benchmark]
        public void FastMathLog2AsNormal()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = FastMath.Log2AsNormal(Unsafe.Add(ref x9, i));
            }
        }

        [BenchmarkCategory("Fast")]
        [Benchmark]
        public void FastMathLog2()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = FastMath.Log2(Unsafe.Add(ref x9, i));
            }
        }

        [BenchmarkCategory("Fast", "FMA", "AsNormal")]
        [Benchmark]
        public void FastMathFastLog2AsNormal()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = FastMath.FastLog2AsNormal(Unsafe.Add(ref x9, i));
            }
        }

        [BenchmarkCategory("Fast", "FMA")]
        [Benchmark]
        public void FastMathFastLog2()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = FastMath.FastLog2(Unsafe.Add(ref x9, i));
            }
        }

        [BenchmarkCategory("Fast", "AsNormal", "Experimental")]
        [Benchmark]
        public void FastMathLog2AsNormalEstrin()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = FastMath.Log2AsNormalEstrin(Unsafe.Add(ref x9, i));
            }
        }

        [BenchmarkCategory("Fast", "AsNormal", "Experimental")]
        [Benchmark]
        public void FastMathLog2AsNormalCake0()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = FastMath.Log2AsNormalCake0(Unsafe.Add(ref x9, i));
            }
        }
    }
}
