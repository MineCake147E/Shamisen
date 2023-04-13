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

namespace Shamisen.Benchmarks.Conversion.SampleToWaveConverters
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    [MedianColumn]
    public class SampleToPcm24ConverterBenchmarks
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
        private Int24[] bufferDst;
        [Params(1)]
        public int Channels { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            bufferSrc = new float[Frames * Channels];
            bufferDst = new Int24[Frames * Channels];
            var g = MemoryMarshal.AsBytes(bufferSrc.AsSpan());
            RandomNumberGenerator.Fill(g);
        }

        [Benchmark]
        public void ProcessNormalAvx2() => SampleToPcm24Converter.ProcessNormalAvx2(bufferDst, bufferSrc);
        [Benchmark]
        public void ProcessNormalStandard() => SampleToPcm24Converter.ProcessNormalStandard(bufferDst, bufferSrc);
        [GlobalCleanup]
        public void Cleanup() => bufferSrc = null;
    }
}
