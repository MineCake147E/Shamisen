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

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Benchmarks.Codecs.Flac
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class FlacCrc16Benchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int FrameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Words")).Value;
                _ = AddColumn(new FrameThroughputColumn(FrameSelector)
                {
                    ColumnName = "CRC Throughput [words/s]",
                    Legend = "# of words of CRC calculated per second"
                });
            }
        }

        private ulong[] srcBuffer;

        [Params(/*2047, */4095, Priority = -990)]
        public int Words { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            srcBuffer = new ulong[Words];
            var g = MemoryMarshal.AsBytes(srcBuffer.AsSpan());
            RandomNumberGenerator.Fill(g);
        }

        [Benchmark]
        public FlacCrc16 Standard() => new FlacCrc16(0) * srcBuffer.AsSpan();

        [Benchmark]
        public FlacCrc16 Pclmulqdq() => FlacCrc16.CalculateCrc16Pclmulqdq(new FlacCrc16(0), srcBuffer.AsSpan());

    }
}
