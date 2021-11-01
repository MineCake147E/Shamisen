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

namespace Shamisen.Benchmarks.Conversion.WaveToSampleConverters
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    [MedianColumn]
    public class Pcm8ToSampleConverterBenchmarks
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
        public void Avx2M()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            Pcm8ToSampleConverter.ProcessAvx2M(byteSpan, span);
        }
        [Benchmark]
        public void Avx2A()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            Pcm8ToSampleConverter.ProcessAvx2A(byteSpan, span);
        }
        [Benchmark]
        public void Sse41()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            Pcm8ToSampleConverter.ProcessSse41(byteSpan, span);
        }
        [Benchmark]
        public void Standard()
        {
            var span = buffer.AsSpan();
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan = byteSpan.Slice(byteSpan.Length - span.Length);
            Pcm8ToSampleConverter.ProcessStandard(byteSpan, span);
        }
        [GlobalCleanup]
        public void Cleanup() => buffer = null;
    }
}
