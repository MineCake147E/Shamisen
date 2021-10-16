using System.Numerics;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.Resampling.Sample;

namespace Shamisen.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net50)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class GenerateCoeffsBenchmarks
    {
        private Vector4[] buffer;
        [Params(2881, Priority = -990)]
        public int Frames { get; set; }
        [GlobalSetup]
        public void Setup() => buffer = new Vector4[Frames];
        [Benchmark]
        public void Standard() => SplineResampler.GenerateCoeffsStandard(buffer, 1.0f / Frames);
        [Benchmark]
        public void Sse2() => SplineResampler.GenerateCoeffsSse2(buffer, 1.0f / Frames);

        [Benchmark]
        public void Fma128() => SplineResampler.GenerateCoeffsFma128(buffer, 1.0f / Frames);
    }
}
