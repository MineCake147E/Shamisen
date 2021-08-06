#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Misc
{
    [SimpleJob(RuntimeMoniker.Net50, baseline: true)]
    [DisassemblyDiagnoser(maxDepth: 16)]
    public class FloatingPointMaxBenchmarks
    {
        [Benchmark]
        [Arguments(0.0f)]
        [Arguments(-6.0f)]
        public Vector256<float> FloatingPointMaxAvx(float value)
        {
            var ymm0 = Vector256.Create(0f);
            var ymm1 = Vector256.Create(-5.0f);
            var ymm2 = Vector256.Create(value);
            for (int i = 0; i < 1024; i++)
            {
                ymm0 = Avx.Max(ymm1, ymm2); //Floating-point max in 256-bit vector resulted in frequency degradation
            }
            return ymm0;
        }
        [Benchmark]
        [Arguments(0.0f)]
        [Arguments(-6.0f)]
        public (Vector128<float>, Vector128<float>) FloatingPointMaxSse(float value)
        {
            var xmm0 = Vector128.Create(0f);
            var xmm3 = Vector128.Create(0f);
            var xmm1 = Vector128.Create(-5.0f);
            var xmm2 = Vector128.Create(value);
            for (int i = 0; i < 1024; i++)
            {
                xmm0 = Sse.Max(xmm1, xmm2);
                xmm3 = Sse.Max(xmm1, xmm2);
            }
            return (xmm0, xmm3);
        }

    }
}

#endif