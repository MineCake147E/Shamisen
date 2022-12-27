using System.Threading;
using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.Resampling.Sample;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Shamisen.Benchmarks.SplineResamplerBenchmarks
{
    [DryJob()]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class ResampleMethodDisassemblyBenchmarks
    {
        private UnifiedResampleArgs args;
        private float[] buffer, srcBuffer;
        private Vector4[] cbuf;
        [GlobalSetup]
        public void Setup()
        {
            args = new(0, 0, 1ul << 32, 0, 0);
            buffer = Array.Empty<float>();
            srcBuffer = new float[1024];
            cbuf = new Vector4[1024];
        }
        #region Benchmarks
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralUpAnyRateUnrolledSse2() =>
            SplineResampler.ResampleCachedDirectMonauralUpAnyRateUnrolledSse2(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralUpAnyRateGenericSse() =>
            SplineResampler.ResampleCachedDirectMonauralUpAnyRateGenericSse(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralIntegerMultipleRateX86() =>
            SplineResampler.ResampleCachedDirectMonauralIntegerMultipleRateX86(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralAnyRateSse3() =>
            SplineResampler.ResampleCachedDirectMonauralAnyRateSse3(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralDoubleRateSse41() =>
            SplineResampler.ResampleCachedDirectMonauralDoubleRateSse41(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralQuadrupleRateSse41() =>
            SplineResampler.ResampleCachedDirectMonauralQuadrupleRateSse41(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectStereoUpAnyRateSse() =>
            SplineResampler.ResampleCachedDirectStereoUpAnyRateSse(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectStereoIntegerRateSse() =>
            SplineResampler.ResampleCachedDirectStereoIntegerRateSse(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectStereoDoubleRateSse() =>
            SplineResampler.ResampleCachedDirectStereoDoubleRateSse(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectStereoQuadrupleRateX64() =>
            SplineResampler.ResampleCachedDirectStereoQuadrupleRateX64(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectStereoAnyRateSse() =>
            SplineResampler.ResampleCachedDirectStereoAnyRateSse(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectGenericAvx2() =>
            SplineResampler.ResampleCachedDirectGenericAvx2(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedEvenGenericAvx2() =>
            SplineResampler.ResampleCachedWrappedEvenGenericAvx2(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedOddGenericAvx2() =>
            SplineResampler.ResampleCachedWrappedOddGenericAvx2(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleDirectGenericAvx2() =>
            SplineResampler.ResampleDirectGenericAvx2(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedOddMonauralStandard() =>
            SplineResampler.ResampleCachedWrappedOddMonauralStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedOddVectorFitChannelsStandard() =>
            SplineResampler.ResampleCachedWrappedOddVectorFitChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedOddGenericStandard() =>
            SplineResampler.ResampleCachedWrappedOddGenericStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedEvenMonauralStandard() =>
            SplineResampler.ResampleCachedWrappedEvenMonauralStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedEvenVectorFitChannelsStandard() =>
            SplineResampler.ResampleCachedWrappedEvenVectorFitChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedEvenGenericStandard() =>
            SplineResampler.ResampleCachedWrappedEvenGenericStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirect2ChannelsStandard() =>
            SplineResampler.ResampleCachedDirect2ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirect3ChannelsStandard() =>
            SplineResampler.ResampleCachedDirect3ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirect4ChannelsStandard() =>
            SplineResampler.ResampleCachedDirect4ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedOdd2ChannelsStandard() =>
            SplineResampler.ResampleCachedWrappedOdd2ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedOdd3ChannelsStandard() =>
            SplineResampler.ResampleCachedWrappedOdd3ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedOdd4ChannelsStandard() =>
            SplineResampler.ResampleCachedWrappedOdd4ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedEven2ChannelsStandard() =>
            SplineResampler.ResampleCachedWrappedEven2ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedEven3ChannelsStandard() =>
            SplineResampler.ResampleCachedWrappedEven3ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedWrappedEven4ChannelsStandard() =>
            SplineResampler.ResampleCachedWrappedEven4ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleDirect2ChannelsStandard() =>
            SplineResampler.ResampleDirect2ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleDirect3ChannelsStandard() =>
            SplineResampler.ResampleDirect3ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleDirect4ChannelsStandard() =>
            SplineResampler.ResampleDirect4ChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectVectorFitChannelsStandard() =>
            SplineResampler.ResampleCachedDirectVectorFitChannelsStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralAnyRateStandard() =>
            SplineResampler.ResampleCachedDirectMonauralAnyRateStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralUpAnyRateStandard() =>
            SplineResampler.ResampleCachedDirectMonauralUpAnyRateStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralIntegerMultipleRateStandard() =>
            SplineResampler.ResampleCachedDirectMonauralIntegerMultipleRateStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralDoubleRateStandard() =>
            SplineResampler.ResampleCachedDirectMonauralDoubleRateStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectMonauralQuadrupleRateStandard() =>
            SplineResampler.ResampleCachedDirectMonauralQuadrupleRateStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleCachedDirectGenericStandard() =>
            SplineResampler.ResampleCachedDirectGenericStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleDirectMonauralStandard() =>
            SplineResampler.ResampleDirectMonauralStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleDirectVectorFitStandard() =>
            SplineResampler.ResampleDirectVectorFitStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void ResampleDirectGenericStandard() =>
            SplineResampler.ResampleDirectGenericStandard(args, buffer.AsSpan(), srcBuffer.AsSpan(), cbuf.AsSpan());
        #endregion
    }
}
