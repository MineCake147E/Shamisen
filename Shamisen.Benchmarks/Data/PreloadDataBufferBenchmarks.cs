using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Data;
using Shamisen.Filters;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks.Data
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class PreloadDataBufferBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new FrameThroughputColumn(a => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames", StringComparison.Ordinal)).Value));
            }
        }

        private IReadableAudioSource<float, SampleFormat> source;
        private SampleDataSource<float, SampleFormat> filter;
        private PreloadDataBuffer<float> preloadDataBuffer;
        private float[] buffer;
        private const int SampleRate = 192000;
        [Params(4095)]
        public int Frames { get; set; }
        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<float, SampleFormat>(new SampleFormat(1, SampleRate));
            filter = new SampleDataSource<float, SampleFormat>(source);
            preloadDataBuffer = new(filter, 8192, 16, true);
            buffer = new float[Frames * 1];
        }

        [Benchmark]
        public void PreloadDataBuffer() => _ = preloadDataBuffer.Read(buffer);

        [GlobalCleanup]
        public void Cleanup() => preloadDataBuffer.Dispose();
    }
}
