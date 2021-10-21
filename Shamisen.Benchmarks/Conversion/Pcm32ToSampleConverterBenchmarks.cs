using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.WaveToSampleConverters;

namespace Shamisen.Benchmarks.Conversion
{
    [SimpleJob(RuntimeMoniker.Net50)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class Pcm32ToSampleConverterBenchmarks
    {
        private const int SampleRate = 192000;
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }
        [Params(4096, 4229)]
        public int Frames { get; set; }
        private float[] bufferDst;
        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames * 1;
            bufferDst = new float[samples];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(bufferDst.AsSpan()));
        }

        [Benchmark]
        public void NormalStandard() => Pcm32ToSampleConverter.ProcessNormalStandard(bufferDst);
        [Benchmark]
        public void NormalSse2() => Pcm32ToSampleConverter.ProcessNormalSse2(bufferDst);

        [Benchmark]
        public void NormalAvx2() => Pcm32ToSampleConverter.ProcessNormalAvx2(bufferDst);

        [Benchmark]
        public void NormalAvx2ExtremeUnroll() => Pcm32ToSampleConverter.ProcessNormalAvx2ExtremeUnroll(bufferDst);

        [Benchmark]
        public void ReversedStandard() => Pcm32ToSampleConverter.ProcessReversedStandard(bufferDst);

        [Benchmark]
        public void ReversedSsse3() => Pcm32ToSampleConverter.ProcessReversedSsse3(bufferDst);

        [Benchmark]
        public void ReversedAvx2() => Pcm32ToSampleConverter.ProcessReversedAvx2(bufferDst);

    }
}
