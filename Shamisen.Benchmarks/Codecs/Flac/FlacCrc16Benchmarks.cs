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
        internal sealed class Config : ManualConfig
        {
            public Config()
            {
                static int FrameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.First(a => string.Equals(a.Name, "Words")).Value;
                _ = AddColumn(new FrameThroughputColumn(FrameSelector)
                {
                    ColumnName = "CRC Throughput [words/s]",
                    Legend = "# of words of CRC calculated per second"
                });
            }
        }

        private ulong[]? srcBuffer;

        public static IEnumerable<int> WordsSource() => Enumerable.Range(0, 6).Select(a => 1 << a);

        [ParamsSource(nameof(WordsSource))]
        public int Words { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            srcBuffer = new ulong[Words];
            var g = MemoryMarshal.AsBytes(srcBuffer.AsSpan());
            RandomNumberGenerator.Fill(g);
        }

        [Benchmark]
        public FlacCrc16 Standard() => FlacCrc16.CalculateCrc16UInt64BigEndianStandard(new FlacCrc16(0), srcBuffer.AsSpan());

        [Benchmark]
        public FlacCrc16 Pclmulqdq() => FlacCrc16.CalculateCrc16UInt64BigEndianPclmulqdq(new FlacCrc16(0), srcBuffer.AsSpan());
    }
}
