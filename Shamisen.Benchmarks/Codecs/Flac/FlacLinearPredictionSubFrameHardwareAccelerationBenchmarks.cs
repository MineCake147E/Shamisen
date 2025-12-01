using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using Shamisen.Codecs.Flac.SubFrames;

namespace Shamisen.Benchmarks.Codecs.Flac
{
    //[AnyCategoriesFilter("Avx512FVL")]
    [Config(typeof(Config))]
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public partial class FlacLinearPredictionSubFrameHardwareAccelerationBenchmarks
    {
        public sealed partial class Config : ManualConfig
        {
            public Config()
            {
                var ordersToMeasure = FrozenSet.Create([.. Enumerable.Range(4, 33 - 4)]);
                _ = AddFilter(new UnionFilter([
                    ..InitIntrinsicsConfig()/*,
                    new NameFilter(a => a.ContainsAny(SearchValues.Create(["Avx2", "Standard"], StringComparison.Ordinal))),
                    new SimpleFilter(a => a.Parameters[nameof(Order)] is int value && ordersToMeasure.Contains(value))*/
                ]));
                _ = ordersToMeasure;
                _ = WithOrderer(new CustomOrderer());
            }

            private sealed class CustomOrderer : DefaultOrderer
            {
                public override IEnumerable<BenchmarkCase> GetExecutionOrder(ImmutableArray<BenchmarkCase> benchmarkCases, IEnumerable<BenchmarkLogicalGroupRule>? order = null)
                    => benchmarkCases.OrderBy(c => c.Parameters[nameof(Order)] is int value ? value : -1);

                public override IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups, IEnumerable<BenchmarkLogicalGroupRule>? order = null)
                    => logicalGroups.OrderBy(g => g.First().Parameters[nameof(Order)] is int value ? value : -1);
            }
        }

        private int[]? outputBuffer;
        private int[] coefficients = new int[32];

        [ParamsSource(nameof(ValuesForOrder))]
        public int Order { get; set; }

        [ParamsSource(nameof(ValuesForAccumulatorDepth))]
        public int AccumulatorDepth { get; set; }

        [ParamsSource(nameof(ValuesForShiftAmount))]
        public int ShiftAmount { get; set; }

        [Params(16383)]
        public int Size { get; set; }

        public static IEnumerable<int> ValuesForOrder() => Enumerable.Range(1, 32);
        public static IEnumerable<int> ValuesForAccumulatorDepth() => [64];
        public static IEnumerable<int> ValuesForShiftAmount() => [7];

        [GlobalSetup]
        public void Setup()
        {
            outputBuffer = new int[Size + Order];
            coefficients = new int[32];
            Random.Shared.NextBytes(MemoryMarshal.AsBytes(coefficients.AsSpan()));
            Random.Shared.NextBytes(MemoryMarshal.AsBytes(outputBuffer.AsSpan()));
        }

        private FlacLinearPredictionSubFrame.RestoreParameters GenerateParameters() => new(new((byte)Order, (byte)AccumulatorDepth, (byte)AccumulatorDepth), ShiftAmount, coefficients, outputBuffer);

        [BenchmarkCategory("Standard")]
        [Benchmark(Baseline = true)]
        public void Standard() => FlacLinearPredictionSubFrame.RestoreSignalStandard(GenerateParameters());
    }
}
