using System;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.WaveToSampleConverters;

namespace Shamisen.Benchmarks.Conversion
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class ALawToSampleConverterBenchmarks
    {
        private const int Frames = 1441;

        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new FrameThroughputColumn(a => Frames));
            }
        }
        private float[] buffer;
        [Params(1)]
        public int Channels { get; set; }

        [GlobalSetup]
        public void Setup() => buffer = new float[Frames * Channels];

        [Benchmark]
        public void Avx2M2()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            ALawToSampleConverter.ProcessAvx2M2(byteSpan, span);
        }
        [Benchmark]
        public void Avx2M3()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            ALawToSampleConverter.ProcessAvx2M3(byteSpan, span);
        }
        [Benchmark]
        public void Avx2FP()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            ALawToSampleConverter.ProcessAvx2FP(byteSpan, span);
        }
        [Benchmark]
        public void Avx2()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
#pragma warning disable CS0618 
            ALawToSampleConverter.ProcessAvx2(byteSpan, span);
#pragma warning restore CS0618 
        }

        [GlobalCleanup]
        public void Cleanup() => buffer = null;
    }
}
