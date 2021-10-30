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

using Shamisen.Conversion.SampleToWaveConverters;

namespace Shamisen.Benchmarks.Conversion
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    [MedianColumn]
    public class SampleToPcm8ConverterBenchmarks
    {
        private const int Frames = 4095;

        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new FrameThroughputColumn(a => Frames));
            }
        }

        private float[] bufferSrc;
        private byte[] bufferDst;
        [Params(1)]
        public int Channels { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            bufferSrc = new float[Frames * Channels];
            bufferDst = new byte[Frames * Channels];
            var g = MemoryMarshal.AsBytes(bufferSrc.AsSpan());
            RandomNumberGenerator.Fill(g);
        }

        /*[Benchmark]
        public void ProcessNormalAvx2M() => SampleToPcm8Converter.ProcessNormalAvx2M(bufferSrc, bufferDst);*/
        [Benchmark]
        public void ProcessNormalAvx2A() => SampleToPcm8Converter.ProcessNormalAvx2A(bufferSrc, bufferDst);
        [Benchmark]
        public void ProcessNormalStandard() => SampleToPcm8Converter.ProcessNormalStandard(bufferSrc, bufferDst);
        [GlobalCleanup]
        public void Cleanup() => bufferSrc = null;
    }
}
