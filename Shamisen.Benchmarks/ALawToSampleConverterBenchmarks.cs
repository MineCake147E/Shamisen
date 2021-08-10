using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: 256)]
    public class ALawToSampleConverterBenchmarks
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
        private ALawToSampleConverter converter;
        private float[] buffer;
        private const int SampleRate = 192000;
        [Params(1)]
        public int Channels { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<byte, IWaveFormat>(new WaveFormat(SampleRate, 8, Channels, AudioEncoding.Alaw));
            converter = new ALawToSampleConverter(source);
            buffer = new float[Frames * Channels];
        }

        [Benchmark]
        public void ALawToSampleConverter()
        {
            var span = buffer.AsSpan();
            _ = converter.Read(span);
        }
        /*[Benchmark]
        public void OldConversion()
        {
            var span = buffer.AsSpan();
            _ = converter.ReadOld(span);
        }*/

        [GlobalCleanup]
        public void Cleanup()
        {
            converter?.Dispose();
            source?.Dispose();
            buffer = null;
        }
    }
}
