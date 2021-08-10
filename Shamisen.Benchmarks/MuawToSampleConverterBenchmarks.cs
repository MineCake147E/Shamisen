using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: 256)]
    public class MuawToSampleConverterBenchmarks
    {
        private const int Frames = 4096;

        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new PlaybackSpeedColumn(
                    a => Frames,
                    a => SampleRate));
            }
        }
        private IReadableAudioSource<byte, IWaveFormat> source;
        private MuLawToSampleConverter converter;
        private float[] buffer;
        private const int SampleRate = 192000;
        [Params(1)]
        public int Channels { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<byte, IWaveFormat>(new WaveFormat(SampleRate, 8, Channels, AudioEncoding.Mulaw));
            converter = new MuLawToSampleConverter(source);
            buffer = new float[Frames * Channels];
        }

        [Benchmark]
        public void MuLawToSampleConverter()
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
