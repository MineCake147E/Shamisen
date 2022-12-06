using System;
using System.Linq;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.WaveToSampleConverters;

namespace Shamisen.Benchmarks.Conversion.WaveToSampleConverters
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class MuLawToSampleConverterBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int FrameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(FrameSelector));
            }
        }
        private float[] buffer;
        private const int SampleRate = 192000;
        [Params(1)]
        public int Channels { get; set; }

        [Params(/*2047, */4095, Priority = -990)]
        public int Frames { get; set; }

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
