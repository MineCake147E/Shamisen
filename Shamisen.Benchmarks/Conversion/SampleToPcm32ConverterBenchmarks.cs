using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks.Conversion
{
    [SimpleJob(RuntimeMoniker.Net50, baseline: true)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class SampleToPcm32ConverterBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int FrameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(FrameSelector));
            }
        }

        private IReadableAudioSource<float, SampleFormat> source;
        private SampleToPcm32Converter converter;
        private byte[] buffer;
        private const int SampleRate = 192000;

        [Params(1)]
        public int Channels { get; set; }

        [Params(true, false)]
        public bool DoDeltaSigmaModulation { get; set; }

        [ParamsAllValues]
        public Endianness TargetEndianness { get; set; }

        [Params((X86Intrinsics)X86IntrinsicsMask.None)]
        public X86Intrinsics EnabledX86Intrinsics { get; set; }

        [Params(/*2047, */4095, Priority = -990)]
        public int Frames { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<float, SampleFormat>(new SampleFormat(Channels, SampleRate));
            converter = new SampleToPcm32Converter(source, DoDeltaSigmaModulation, TargetEndianness);
            buffer = new byte[Frames * Channels * sizeof(short)];
        }

        [Benchmark]
        public void SampleToPcm32()
        {
            var span = buffer.AsSpan();
            _ = converter.Read(span);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            converter?.Dispose();
            source?.Dispose();
            buffer = null;
        }
    }
}
