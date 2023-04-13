using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.WaveToSampleConverters;

namespace Shamisen.Benchmarks.Conversion.WaveToSampleConverters
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    [MedianColumn]
    public class ALawToSampleConverterBenchmarks
    {
        private const int Frames = 4095;

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
        public void Setup()
        {
            buffer = new float[Frames * Channels];
            var g = MemoryMarshal.AsBytes(buffer.AsSpan());
            RandomNumberGenerator.Fill(g);
        }

        [Benchmark]
        public void Avx2()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            ALawToSampleConverter.ProcessAvx2(span, byteSpan);
        }

        [Benchmark]
        public void Sse41()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            ALawToSampleConverter.ProcessSse41(span, byteSpan);
        }

        [GlobalCleanup]
        public void Cleanup() => buffer = null;
    }
}
