using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Shamisen.Codecs.Flac.SubFrames;

namespace Shamisen.Benchmarks.Codecs.Flac
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public unsafe class FlacLinearPredictionSubFrameHardwareAccelerationBenchmarks
    {
        private int[] outputBuffer;
        private int[] residualBuffer;
        private int[] coefficients;

        [ParamsSource(nameof(ValuesForOrder))]
        public int Order { get; set; }

        [Params(16383)]
        public int Size { get; set; }

        public static IEnumerable<int> ValuesForOrder() => [32];

        [GlobalSetup]
        public void Setup()
        {
            outputBuffer = new int[Size + Order];
            residualBuffer = new int[Size];
            coefficients = new int[Order];
            Random.Shared.NextBytes(MemoryMarshal.AsBytes(coefficients.AsSpan()));
            Random.Shared.NextBytes(MemoryMarshal.AsBytes(residualBuffer.AsSpan()));
            Random.Shared.NextBytes(MemoryMarshal.AsBytes(outputBuffer.AsSpan()));
        }

        [Benchmark]
        public void Default() => FlacLinearPredictionSubFrame.RestoreSignalDefault(0, 32, residualBuffer, coefficients, outputBuffer);

        [Benchmark]
        public void Standard() => FlacLinearPredictionSubFrame.RestoreSignalStandard(0, 32, residualBuffer, coefficients, outputBuffer);

        [Benchmark]
        public void Default64() => FlacLinearPredictionSubFrame.RestoreSignalDefault(16, 48, residualBuffer, coefficients, outputBuffer);

        [Benchmark]
        public void Standard64() => FlacLinearPredictionSubFrame.RestoreSignalStandard(16, 48, residualBuffer, coefficients, outputBuffer);
    }
}
