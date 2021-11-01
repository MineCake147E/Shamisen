using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks.Conversion.SampleToWaveConverters
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class SampleToPcm16ConverterBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int FrameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                _ = AddColumn(new FrameThroughputColumn(FrameSelector));
                //_ = AddColumn(new PlaybackSpeedColumn(
                //    FrameSelector,
                //    a => ((ConversionRatioProps)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "ConversionRatio")).Value).After));

            }
        }
        private IReadableAudioSource<float, SampleFormat> source;
        private SampleToPcm16Converter converter;
        private float[] srcBuffer;
        private short[] dstBuffer;
        private const int SampleRate = 192000;

        [Params(/*2047, */4095, Priority = -990)]
        public int Frames { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            dstBuffer = new short[Frames];
            srcBuffer = new float[Frames];
            var g = MemoryMarshal.AsBytes(srcBuffer.AsSpan());
            RandomNumberGenerator.Fill(g);
        }

        [Benchmark]
        public void ProcessNormalAvx2() => SampleToPcm16Converter.ProcessNormalAvx2(srcBuffer, dstBuffer);
        [Benchmark]
        public void ProcessNormalSse2() => SampleToPcm16Converter.ProcessNormalSse2(srcBuffer, dstBuffer);
        [Benchmark]
        public void ProcessNormalStandard() => SampleToPcm16Converter.ProcessNormalStandard(srcBuffer, dstBuffer);
        [Benchmark]
        public void ProcessReversedAvx2() => SampleToPcm16Converter.ProcessReversedAvx2(srcBuffer, dstBuffer);
        [Benchmark]
        public void ProcessReversedSsse3() => SampleToPcm16Converter.ProcessReversedSsse3(srcBuffer, dstBuffer);
        [Benchmark]
        public void ProcessReversedStandard() => SampleToPcm16Converter.ProcessReversedStandard(srcBuffer, dstBuffer);
        [GlobalCleanup]
        public void Cleanup()
        {
            converter?.Dispose();
            source?.Dispose();
            dstBuffer = null;
        }

#pragma warning disable S125 // Sections of code should not be commented out
        /*
        ``` ini

        BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1288 (21H1/May2021Update)
        Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
        .NET SDK=6.0.100-rc.2.21505.57
          [Host]   : .NET 5.0.11 (5.0.1121.47308), X64 RyuJIT
          .NET 5.0 : .NET 5.0.11 (5.0.1121.47308), X64 RyuJIT

        Job=.NET 5.0  Runtime=.NET 5.0

        ```
        |        Method | Frames | Channels | DoDeltaSigmaModulation |                            EnabledX86Intrinsics | TargetEndianness |        Mean |     Error |    StdDev | Ratio | Frame Throughput [Frames/s] | Code Size |
        |-------------- |------- |--------- |----------------------- |------------------------------------------------ |----------------- |------------:|----------:|----------:|------:|----------------------------:|----------:|
        | **SampleToPcm16** |   **4095** |        **1** |                  **False** |                                            **None** |           **Little** |    **647.5 ns** |   **3.01 ns** |   **2.67 ns** |  **1.00** |      **6,324,256,553.08469000** |   **3,007 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                  **False** |                                            **None** |              **Big** |  **3,774.6 ns** |  **34.03 ns** |  **31.83 ns** |  **1.00** |      **1,084,895,397.79907000** |   **3,007 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                  **False** |            **Sse, Sse2, Sse3, Ssse3, Sse41, Sse42** |           **Little** |    **883.6 ns** |   **5.33 ns** |   **4.72 ns** |  **1.00** |      **4,634,367,428.32514000** |   **3,007 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                  **False** |            **Sse, Sse2, Sse3, Ssse3, Sse41, Sse42** |              **Big** |    **900.6 ns** |   **4.07 ns** |   **3.81 ns** |  **1.00** |      **4,546,980,816.06529000** |   **3,007 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                  **False** | **Sse, Sse2, Sse3, Ssse3, Sse41, Sse42, Avx, Avx2** |           **Little** |    **583.6 ns** |   **4.50 ns** |   **3.99 ns** |  **1.00** |      **7,017,367,119.37300000** |   **3,007 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                  **False** | **Sse, Sse2, Sse3, Ssse3, Sse41, Sse42, Avx, Avx2** |              **Big** |    **658.1 ns** |   **4.29 ns** |   **4.01 ns** |  **1.00** |      **6,222,361,259.51207000** |   **3,007 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                   **True** |                                            **None** |           **Little** | **33,118.4 ns** | **120.31 ns** | **112.54 ns** |  **1.00** |        **123,647,215.11759800** |   **1,936 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                   **True** |                                            **None** |              **Big** | **33,145.0 ns** | **128.54 ns** | **120.24 ns** |  **1.00** |        **123,548,111.68169500** |   **1,936 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                   **True** |            **Sse, Sse2, Sse3, Ssse3, Sse41, Sse42** |           **Little** | **33,153.7 ns** |  **91.74 ns** |  **85.81 ns** |  **1.00** |        **123,515,468.23248900** |   **1,936 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                   **True** |            **Sse, Sse2, Sse3, Ssse3, Sse41, Sse42** |              **Big** | **33,123.2 ns** |  **97.27 ns** |  **90.99 ns** |  **1.00** |        **123,629,303.77549700** |   **1,936 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                   **True** | **Sse, Sse2, Sse3, Ssse3, Sse41, Sse42, Avx, Avx2** |           **Little** | **33,132.7 ns** | **115.65 ns** | **108.18 ns** |  **1.00** |        **123,593,903.43703100** |   **1,936 B** |
        |               |        |          |                        |                                                 |                  |             |           |           |       |                             |           |
        | **SampleToPcm16** |   **4095** |        **1** |                   **True** | **Sse, Sse2, Sse3, Ssse3, Sse41, Sse42, Avx, Avx2** |              **Big** | **33,148.7 ns** | **121.50 ns** |  **94.86 ns** |  **1.00** |        **123,534,389.11120900** |   **1,936 B** |

        */
    }

#pragma warning restore S125 // Sections of code should not be commented out
}
