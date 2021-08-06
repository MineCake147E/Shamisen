using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Misc
{
    [SimpleJob(RuntimeMoniker.Net50, baseline: true)]
    [DisassemblyDiagnoser(maxDepth: 16)]
    public class FloatingPointMultiplyByPowerOfTwoBenchmarks
    {
        [Benchmark]
        [Arguments(0.0f)]
        [Arguments(-6.0f)]
        public Vector256<float> FloatingPointMultiplyAvx(float value)
        {
            var ymm0 = Vector256.Create(0f);
            var ymm1 = Vector256.Create(32768.0f);
            var ymm2 = Vector256.Create(value);
            for (int i = 0; i < 1024; i++)
            {
                ymm0 = Avx.Multiply(ymm1, ymm2); //Floating-point mul in 256-bit vector results in frequency degradation
            }
            return ymm0;
        }
        [Benchmark]
        [Arguments(0.0f)]
        [Arguments(-6.0f)]
        public (Vector128<float>, Vector128<float>) FloatingPointMultiplySse(float value)
        {
            var xmm0 = Vector128.Create(0f);
            var xmm3 = Vector128.Create(0f);
            var xmm1 = Vector128.Create(32768.0f);
            var xmm2 = Vector128.Create(value);
            for (int i = 0; i < 1024; i++)
            {
                xmm0 = Sse.Multiply(xmm1, xmm2);
                xmm3 = Sse.Multiply(xmm2, xmm1);
            }
            return (xmm0, xmm3);
        }
        [Benchmark]
        [Arguments(0.0f)]
        [Arguments(-6.0f)]
        public Vector256<float> IntegerAddAvx2(float value)
        {
            var ymm0 = Vector256.Create(0f);
            var ymm1 = Vector256.Create(0x0780_0000);
            var ymm2 = Vector256.Create(value);
            for (int i = 0; i < 1024; i++)
            {
                //For 256-bits vectors, the integer-addition method is significantly faster, but invalid for denormalized numbers
                ymm0 = Avx2.Add(ymm1, ymm2.AsInt32()).AsSingle();
            }
            return ymm0;
        }
        [Benchmark]
        [Arguments(0.0f)]
        [Arguments(-6.0f)]
        public (Vector128<float>, Vector128<float>) IntegerAddSse2(float value)
        {
            var xmm0 = Vector128.Create(0f);
            var xmm3 = Vector128.Create(0f);
            var xmm1 = Vector128.Create(0x0780_0000);
            var xmm2 = Vector128.Create(value);
            for (int i = 0; i < 1024; i++)
            {
                xmm0 = Sse2.Add(xmm1, xmm2.AsInt32()).AsSingle();
                xmm3 = Sse2.Add(xmm2.AsInt32(), xmm1).AsSingle();
            }
            return (xmm0, xmm3);
        }
    }
}
