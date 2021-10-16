
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Synthesis;

namespace Shamisen.Benchmarks
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net50)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class SquareWaveSourceBenchmarks
    {
        private SquareWaveSource source;
        private float[] buffer;
        private const int Frames = 1441;
        private const int SampleRate = 192000;

        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new FrameThroughputColumn(a => Frames));
            }
        }
        [Params(1, 2, 3, 4, 5, 6, 7, 8)]
        public int Channels { get; set; }

        public static IEnumerable<double> FrequencyParams()
        {
            double nyquist = SampleRate / 2.0;
            double freq = nyquist;
            for (int i = 0; i < 1; i++)
            {
                yield return freq;
                freq /= 2;
            }
        }

        //[ParamsSource(nameof(FrequencyParams))]
        [Params(440.0)]
        public double Frequency { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            buffer = new float[Frames * Channels];
            source = new SquareWaveSource(new SampleFormat(Channels, SampleRate))
            {
                Frequency = Frequency
            };

        }

        [Benchmark]
        public void SquareWaveSource() => _ = source.Read(buffer);

        [GlobalCleanup]
        public void Cleanup()
        {
            buffer = null;
            source.Dispose();
        }
    }
}
