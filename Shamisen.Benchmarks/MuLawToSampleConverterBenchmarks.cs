using System;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.WaveToSampleConverters;

namespace Shamisen.Benchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: 256)]
    public class MuLawToSampleConverterBenchmarks
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
        private const int SampleRate = 192000;
        [Params(1)]
        public int Channels { get; set; }

        [GlobalSetup]
        public void Setup() => buffer = new float[Frames * Channels];

        [Benchmark]
        public void Sse41()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            MuLawToSampleConverter.ProcessSse41(byteSpan, span);
        }
        [Benchmark]
        public void Avx2MM128()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            MuLawToSampleConverter.ProcessAvx2MM128(byteSpan, span);
        }
        [Benchmark]
        public void Avx2MM256()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            MuLawToSampleConverter.ProcessAvx2MM256(byteSpan, span);
        }
        [GlobalCleanup]
        public void Cleanup() => buffer = null;
    }
}
