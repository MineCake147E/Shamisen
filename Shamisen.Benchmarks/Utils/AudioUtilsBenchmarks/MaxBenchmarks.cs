using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Utils;

namespace Shamisen.Benchmarks.Utils.AudioUtilsBenchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class MaxBenchmarks
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
        private float[] bufferA;
        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames;
            bufferA = new float[samples];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(bufferA.AsSpan()));
            RangedPcm32ToSampleConverter.ProcessEMoreThan24Standard(bufferA, MemoryMarshal.Cast<float, int>(bufferA.AsSpan()), 24);
        }

        [Benchmark]
        public float VectorizedMax() => AudioUtils.Max(bufferA);

        [Benchmark]
        public float LinqMax() => bufferA.Max();

        [Benchmark]
        public float SimpleMax()
        {
            var span = bufferA.AsSpan();
            if (span.Length < 1) return float.NaN;
            var max = span[0];
            for (int i = 0; i < span.Length; i++)
            {
                max = Math.Max(max, span[i]);
            }
            return max;
        }

        [Benchmark]
        public float SimpleFastMax()
        {
            var span = bufferA.AsSpan();
            if (span.Length < 1) return float.NaN;
            var max = span[0];
            for (int i = 0; i < span.Length; i++)
            {
                max = FastMath.Max(max, span[i]);
            }
            return max;
        }
    }
}
