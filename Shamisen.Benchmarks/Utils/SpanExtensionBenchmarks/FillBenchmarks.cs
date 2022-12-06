using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Utils.SpanExtensionBenchmarks
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.Method, BenchmarkDotNet.Order.MethodOrderPolicy.Alphabetical)]
    [SimpleJob(runtimeMoniker: RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class FillBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }
        private DateOnly[] bufferDstDateOnly;

        private DateTime[] bufferDstDateTime;

        private Guid[] bufferDstGuid;

        private Int24[] bufferDstInt24;

        private float[] bufferDstSingle;
        private ulong[] bufferDstUInt64;
        private static readonly DateOnly DateOnlyParameter = DateOnly.FromDateTime(DateTime.UtcNow);
        private static readonly DateTime DateTimeParameter = DateTime.UtcNow;
        private static readonly Guid GuidParameter = Guid.NewGuid();

        public IEnumerable<DateOnly> DateOnlyValue { get { yield return DateOnlyParameter; } }

        public IEnumerable<DateTime> DateTimeValue { get { yield return DateTimeParameter; } }

        [Params(4095)]
        public int Frames { get; set; }

        public IEnumerable<Guid> GuidValue { get { yield return GuidParameter; } }

        public IEnumerable<Int24> Int24Value { get { yield return Int24.MaxValue; } }
        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames;
            bufferDstSingle = new float[samples];
            bufferDstInt24 = new Int24[samples];
            bufferDstDateOnly = new DateOnly[samples];
            bufferDstDateTime = new DateTime[samples];
            bufferDstGuid = new Guid[samples];
            bufferDstUInt64 = new ulong[samples];
        }

        [ArgumentsSource(nameof(DateOnlyValue))]
        [BenchmarkCategory("DateOnly"), Benchmark(Baseline = true)]
        public void DateOnlyQuickFill(DateOnly value) => SpanUtils.QuickFill(bufferDstDateOnly, value);

        [ArgumentsSource(nameof(DateTimeValue))]
        [BenchmarkCategory("DateTime"), Benchmark(Baseline = true)]
        public void DateTimeQuickFill(DateTime value) => SpanUtils.QuickFill(bufferDstDateTime, value);

        [ArgumentsSource(nameof(GuidValue))]
        [BenchmarkCategory("Guid"), Benchmark(Baseline = true)]
        public void GuidQuickFill(Guid value) => SpanUtils.QuickFill(bufferDstGuid, value);

        [ArgumentsSource(nameof(Int24Value))]
        [BenchmarkCategory("Int24"), Benchmark(Baseline = true)]
        public void Int24QuickFill(Int24 value) => SpanUtils.QuickFill(bufferDstInt24, value);

        [Arguments(MathF.PI)]
        [BenchmarkCategory("Single"), Benchmark(Baseline = true)]
        public void SingleQuickFill(float value) => SpanUtils.QuickFill(bufferDstSingle, value);

        [Arguments(0x4009_21FB_5444_2D18ul)]
        [BenchmarkCategory("UInt64"), Benchmark(Baseline = true)]
        public void UInt64QuickFill(ulong value) => SpanUtils.QuickFill(bufferDstUInt64, value);

        [ArgumentsSource(nameof(DateOnlyValue))]
        [BenchmarkCategory("DateOnly"), Benchmark(Baseline = false)]
        public void DateOnlyStandardFill(DateOnly value) => bufferDstDateOnly.AsSpan().Fill(value);

        [ArgumentsSource(nameof(DateTimeValue))]
        [BenchmarkCategory("DateTime"), Benchmark(Baseline = false)]
        public void DateTimeStandardFill(DateTime value) => bufferDstDateTime.AsSpan().Fill(value);

        [ArgumentsSource(nameof(GuidValue))]
        [BenchmarkCategory("Guid"), Benchmark(Baseline = false)]
        public void GuidStandardFill(Guid value) => bufferDstGuid.AsSpan().Fill(value);

        [ArgumentsSource(nameof(Int24Value))]
        [BenchmarkCategory("Int24"), Benchmark(Baseline = false)]
        public void Int24StandardFill(Int24 value) => bufferDstInt24.AsSpan().Fill(value);

        [Arguments(MathF.PI)]
        [BenchmarkCategory("Single"), Benchmark(Baseline = false)]
        public void SingleStandardFill(float value) => bufferDstSingle.AsSpan().Fill(value);

        [Arguments(0x4009_21FB_5444_2D18ul)]
        [BenchmarkCategory("UInt64"), Benchmark(Baseline = false)]
        public void UInt64StandardFill(ulong value) => bufferDstUInt64.AsSpan().Fill(value);
    }
}
