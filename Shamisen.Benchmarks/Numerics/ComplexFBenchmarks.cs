using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Numerics
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class ComplexFBenchmarks
    {
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

        private ComplexF[] x, y, z;
        private Vector2[] v;

        [GlobalSetup]
        public void Setup()
        {
            x = new ComplexF[Frames];
            y = new ComplexF[Frames];
            z = new ComplexF[Frames];
            v = new Vector2[Frames];
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        [Benchmark]
        public void Ctor()
        {
            var sV = v.AsSpan();
            var sZ = z.AsSpan();
            ref var rsi = ref MemoryMarshal.GetReference(sV);
            ref var rdi = ref MemoryMarshal.GetReference(sZ);
            nint i, length = MathI.Min(sZ.Length, sV.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = new(Unsafe.Add(ref rsi, i));
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        [Benchmark]
        public void Multiply()
        {
            var sX = x.AsSpan();
            var sY = y.AsSpan();
            var sZ = z.AsSpan();
            ref var rsi = ref MemoryMarshal.GetReference(sX);
            ref var r8 = ref MemoryMarshal.GetReference(sY);
            ref var rdi = ref MemoryMarshal.GetReference(sZ);
            nint i, length = MathI.Min(MathI.Min(sX.Length, sY.Length), sZ.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) * Unsafe.Add(ref r8, i);
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        [Benchmark]
        public void Add()
        {
            var sX = x.AsSpan();
            var sY = y.AsSpan();
            var sZ = z.AsSpan();
            ref var rsi = ref MemoryMarshal.GetReference(sX);
            ref var r8 = ref MemoryMarshal.GetReference(sY);
            ref var rdi = ref MemoryMarshal.GetReference(sZ);
            nint i, length = MathI.Min(MathI.Min(sX.Length, sY.Length), sZ.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) + Unsafe.Add(ref r8, i);
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        [Benchmark]
        public void Subtract()
        {
            var sX = x.AsSpan();
            var sY = y.AsSpan();
            var sZ = z.AsSpan();
            ref var rsi = ref MemoryMarshal.GetReference(sX);
            ref var r8 = ref MemoryMarshal.GetReference(sY);
            ref var rdi = ref MemoryMarshal.GetReference(sZ);
            nint i, length = MathI.Min(MathI.Min(sX.Length, sY.Length), sZ.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) - Unsafe.Add(ref r8, i);
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        [Benchmark]
        public void MultiplyConstant()
        {
            var sX = x.AsSpan();
            var sZ = z.AsSpan();
            var vy = new ComplexF(0.0f, 1.0f);
            ref var rsi = ref MemoryMarshal.GetReference(sX);
            ref var rdi = ref MemoryMarshal.GetReference(sZ);
            nint i, length = MathI.Min(sX.Length, sZ.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) * vy;
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        [Benchmark]
        public void AddConstant()
        {
            var sX = x.AsSpan();
            var sZ = z.AsSpan();
            var vy = new ComplexF(0.0f, 1.0f);
            ref var rsi = ref MemoryMarshal.GetReference(sX);
            ref var rdi = ref MemoryMarshal.GetReference(sZ);
            nint i, length = MathI.Min(sX.Length, sZ.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) + vy;
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        [Benchmark]
        public void SubtractConstant()
        {
            var sX = x.AsSpan();
            var sZ = z.AsSpan();
            var vy = new ComplexF(0.0f, 1.0f);
            ref var rsi = ref MemoryMarshal.GetReference(sX);
            ref var rdi = ref MemoryMarshal.GetReference(sZ);
            nint i, length = MathI.Min(sX.Length, sZ.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) - vy;
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        [Benchmark]
        public void Conjugate()
        {
            var sX = x.AsSpan();
            var sZ = z.AsSpan();
            ref var rsi = ref MemoryMarshal.GetReference(sX);
            ref var rdi = ref MemoryMarshal.GetReference(sZ);
            nint i, length = MathI.Min(sZ.Length, sX.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = ComplexF.Conjugate(Unsafe.Add(ref rsi, i));
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        [Benchmark]
        public void Negate()
        {
            var sX = x.AsSpan();
            var sZ = z.AsSpan();
            ref var rsi = ref MemoryMarshal.GetReference(sX);
            ref var rdi = ref MemoryMarshal.GetReference(sZ);
            nint i, length = MathI.Min(sZ.Length, sX.Length);
            for (i = 0; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = -Unsafe.Add(ref rsi, i);
            }
        }
    }
}
