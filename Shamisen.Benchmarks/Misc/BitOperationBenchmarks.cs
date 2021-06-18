using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Misc
{
    [SimpleJob(RuntimeMoniker.Net50, baseline: true)]
    [DisassemblyDiagnoser(maxDepth: 16)]
    public class BitOperationBenchmarks
    {
        [Benchmark]
        [Arguments(0xffffffff, 10, 10)]
        [Arguments(0xffffffff, 20, 10)]
        [Arguments(0, 10, 10)]
        [Arguments(0, 20, 10)]
        public uint MathIExtract(uint value, byte start, byte length)
        {
            return MathI.ExtractBitField(value, start, length);
        }

        [Benchmark]
        [Arguments(0xffffffff, 10, 10)]
        [Arguments(0xffffffff, 20, 10)]
        [Arguments(0, 10, 10)]
        [Arguments(0, 20, 10)]
        public uint FallbackExtract(uint value, byte start, byte length)
        {
            return MathIFallbacks.ExtractBitField(value, start, length);
        }
    }
}
