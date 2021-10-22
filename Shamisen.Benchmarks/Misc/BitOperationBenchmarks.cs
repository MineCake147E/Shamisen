using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Misc
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    [DisassemblyDiagnoser(maxDepth: 16)]
    public class BitOperationBenchmarks
    {
        [Benchmark]
        [Arguments(0xffffffff, 10, 10)]
        public uint MathIExtract(uint value, byte start, byte length) => MathI.ExtractBitField(value, start, length);

        [Benchmark]
        [Arguments(0xffffffff, 10, 10)]
        public uint ZeroHighBits(uint value, byte start, byte length)
        {
            value >>= start;
            return Bmi2.ZeroHighBits(value, length);
        }

        [Benchmark]
        [Arguments(0xffffffff, 10, 10)]
        public uint BitFieldExtract(uint value, byte start, byte length) => Bmi1.BitFieldExtract(value, start, length);

        [Benchmark]
        [Arguments(0xffffffff, 10, 10)]
        public uint FallbackExtract(uint value, byte start, byte length) => MathIFallbacks.ExtractBitField(value, start, length);
    }
}
