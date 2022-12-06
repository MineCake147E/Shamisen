
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Synthesis;

namespace Shamisen.Benchmarks.Synthesis
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class SinusoidSourceBenchmarks
    {
        private SinusoidSource source;
        private float[] buffer;
        private const int Frames = 1441;

        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new FrameThroughputColumn(a => Frames));
            }
        }
        [Params(1)]
        public int Channels { get; set; }
        [GlobalSetup]
        public void Setup()
        {
            buffer = new float[Frames * Channels];
            source = new SinusoidSource(new SampleFormat(1, 192000))
            {
                Frequency = 1000
            };
        }

        [Benchmark]
        public void SinusoidSource() => _ = source.Read(buffer);

        [GlobalCleanup]
        public void Cleanup()
        {
            buffer = null;
            source.Dispose();
        }
    }
}
