using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Codecs.Flac.Parsing;
using Shamisen.Data;

namespace Shamisen.Benchmarks.Codecs.Flac
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class FlacBitReaderBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new FrameThroughputColumn(a => 1) { ColumnName = "Operation Throughput [op/s]", Legend = "Operations done per second" });
            }
        }

        private DummyDataSource<byte> source;
        private FlacBitReader bitReader;
        [GlobalSetup]
        public void Setup()
        {
            source = new();
            bitReader = new(source);
        }

        [Arguments((byte)31)]
        [Benchmark]
        public bool ReadBitsUInt32(byte bits) => bitReader.ReadBitsUInt32(bits, out _);
    }
}
