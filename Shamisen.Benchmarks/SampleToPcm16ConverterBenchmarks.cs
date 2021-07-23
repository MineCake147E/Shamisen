using System;
using System.Collections.Generic;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net50, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [DisassemblyDiagnoser(maxDepth: 16)]
    public class SampleToPcm16ConverterBenchmarks
    {
        private IReadableAudioSource<float, SampleFormat> source;
        private SampleToPcm16Converter converter;
        private byte[] buffer;
        private const int SampleRate = 192000;

        [Params(1)]
        public int Channels { get; set; }

        [Params(true, false)]
        public bool EnableIntrinsics { get; set; }

        [Params(/*(X86Intrinsics)X86IntrinsicsMask.None, */(X86Intrinsics)X86IntrinsicsMask.Sse42)]
        public X86Intrinsics EnabledX86Intrinsics { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<float, SampleFormat>(new SampleFormat(Channels, SampleRate));
            converter = new SampleToPcm16Converter(source, EnableIntrinsics, EnabledX86Intrinsics, IntrinsicsUtils.ArmIntrinsics, true, Endianness.Little);
            buffer = new byte[1024 * Channels * sizeof(short)];
        }

        [Benchmark]
        public void SampleToPcm16()
        {
            var span = buffer.AsSpan();
            _ = converter.Read(span);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            converter?.Dispose();
            source?.Dispose();
            buffer = null;
        }

#pragma warning disable S125 // Sections of code should not be commented out
        /*
        // * Summary *

        BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
        Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
          [Host]        : .NET Framework 4.8 (4.8.4300.0), X64 RyuJIT
          .NET Core 5.0 : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT

        Job=.NET Core 5.0  Runtime=.NET Core 5.0

        |        Method | Channels | EnableIntrinsics | EnabledX86Intrinsics |       Mean |    Error |   StdDev | Ratio | Code Size |
        |-------------- |--------- |----------------- |--------------------- |-----------:|---------:|---------:|------:|----------:|
        | SampleToPcm16 |        1 |            False | Sse, (...)Sse42 [36] | 2,562.5 ns | 34.94 ns | 29.17 ns |  1.00 |    1974 B |
        |               |          |                  |                      |            |          |          |       |           |
        | SampleToPcm16 |        1 |             True | Sse, (...)Sse42 [36] |   263.2 ns |  5.19 ns |  4.86 ns |  1.00 |    2348 B |
        */
    }

#pragma warning restore S125 // Sections of code should not be commented out
}
