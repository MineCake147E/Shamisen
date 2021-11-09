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
    [SimpleJob(RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class RangedPcm32ToSampleConverterBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }

        public static int[] GetBitDepthValues() => new[] { 8, 12, 16, 24, 32 };

        [Params(4095)]
        public int Frames { get; set; }

        [ParamsSource(nameof(GetBitDepthValues))]
        public int EffectiveBitDepth { get; set; }
        private float[] bufferDst;
        private int[] bufferSrc;
        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames * 1;
            bufferDst = new float[samples];
            bufferSrc = new int[samples];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(bufferSrc.AsSpan()));
        }

        [Benchmark]
        public void ProcessStandard() => RangedPcm32ToSampleConverter.ProcessEMoreThan24Standard(bufferDst, bufferSrc, EffectiveBitDepth);
    }
}
