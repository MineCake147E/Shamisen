using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Utils.FastMathBenchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class Exp2Benchmarks
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

        [Benchmark]
        public void MathFExp2()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = MathF.Pow(2.0f, Unsafe.Add(ref x9, i));
            }
        }

        [Benchmark]
        public void FastMathExp2()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = FastMath.Exp2(Unsafe.Add(ref x9, i));
            }
        }

        [Benchmark]
        public void FastMathFastExp2()
        {
            var source = bufferSrc.AsSpan();
            var destination = bufferDst.AsSpan();
            ref var x9 = ref MemoryMarshal.GetReference(source);
            ref var x10 = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(destination.Length, source.Length);
            for (; i < length; i++)
            {
                Unsafe.Add(ref x10, i) = FastMath.FastExp2(Unsafe.Add(ref x9, i));
            }
        }

    }
}
