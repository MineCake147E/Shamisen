using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Shamisen.Benchmarks.Utils.SpanExtensionBenchmarks
{
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

        [Params(4095)]
        public int Frames { get; set; }

        [Params(1)]
        public int Channels { get; set; }

        public IEnumerable<DateOnly> DateOnlyValue { get { yield return DateOnly.FromDateTime(DateTime.Now); } }
        public IEnumerable<DateTime> DateTimeValue { get { yield return DateTime.Now; } }
        public IEnumerable<Guid> GuidValue { get { yield return Guid.NewGuid(); } }

        public IEnumerable<Int24> Int24Value { get { yield return Int24.MaxValue; } }

        private float[] bufferDstSingle;
        private Int24[] bufferDstInt24;
        private DateOnly[] bufferDstDateOnly;
        private DateTime[] bufferDstDateTime;
        private Guid[] bufferDstGuid;
        [GlobalSetup]
        public void Setup()
        {
            var samples = Frames * Channels;
            bufferDstSingle = new float[samples];
            bufferDstDateOnly = new DateOnly[samples];
            bufferDstDateTime = new DateTime[samples];
            bufferDstGuid = new Guid[samples];
        }

        [Arguments(MathF.PI)]
        [Benchmark]
        public void StandardFillSingle(float value) => bufferDstSingle.AsSpan().Fill(value);

        [Arguments(MathF.PI)]
        [Benchmark]
        public void FastFillSingle(float value) => SpanExtensions.FastFill(bufferDstSingle, value);

        //[ArgumentsSource(nameof(Int24Value))]
        //[Benchmark]
        //public void StandardFillInt24(Int24 value) => bufferDstInt24.AsSpan().Fill(value);

        //[ArgumentsSource(nameof(Int24Value))]
        //[Benchmark]
        //public void QuickFillInt24(Int24 value) => SpanExtensions.QuickFill(bufferDstInt24, value);

        [ArgumentsSource(nameof(DateOnlyValue))]
        [Benchmark]
        public void StandardFillDateOnly(DateOnly value) => bufferDstDateOnly.AsSpan().Fill(value);

        [ArgumentsSource(nameof(DateOnlyValue))]
        [Benchmark]
        public void QuickFillDateOnly(DateOnly value) => bufferDstDateOnly.AsSpan().QuickFill(value);

        [ArgumentsSource(nameof(DateTimeValue))]
        [Benchmark]
        public void StandardFillDateTime(DateTime value) => bufferDstDateTime.AsSpan().Fill(value);

        [ArgumentsSource(nameof(DateTimeValue))]
        [Benchmark]
        public void QuickFillDateTime(DateTime value) => bufferDstDateTime.AsSpan().QuickFill(value);

        //[ArgumentsSource(nameof(GuidValue))]
        //[Benchmark]
        //public void StandardFillGuid(Guid value) => bufferDstGuid.AsSpan().Fill(value);

        //[ArgumentsSource(nameof(GuidValue))]
        //[Benchmark]
        //public void QuickFillGuid(Guid value) => bufferDstGuid.AsSpan().QuickFill(value);
    }
}
