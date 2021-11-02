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
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class Pcm24ToSampleConverterBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int FrameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(FrameSelector));
                //_ = AddColumn(new PlaybackSpeedColumn(
                //    FrameSelector,
                //    a => ((ConversionRatioProps)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "ConversionRatio")).Value).After));

            }
        }

        private IReadableAudioSource<float, SampleFormat> source;
        private float[] dstBuffer;
        private Int24[] srcBuffer;
        private const int SampleRate = 192000;

        [Params(/*2047, */4095, Priority = -990)]
        public int Frames { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            srcBuffer = new Int24[Frames];
            dstBuffer = new float[Frames];
            var g = MemoryMarshal.AsBytes(srcBuffer.AsSpan());
            RandomNumberGenerator.Fill(g);
        }

        [Benchmark]
        public void ProcessNormalStandard() => Pcm24ToSampleConverter.ProcessNormalStandard(srcBuffer, dstBuffer);

        [Benchmark]
        public void ProcessNormalAvx2P() => Pcm24ToSampleConverter.ProcessNormalAvx2(srcBuffer, dstBuffer);
    }
}
