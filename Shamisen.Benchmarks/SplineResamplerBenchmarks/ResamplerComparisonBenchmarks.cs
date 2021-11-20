using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using CSCore.DSP;

using Shamisen;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks.SplineResamplerBenchmarks
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    [Config(typeof(Config))]
    public class ResamplerComparisonBenchmarks
    {
        #region Configs and custom columns

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
        #endregion

        private DummySource<float, SampleFormat> sourceMA;
        private DummySourceCSCore sourceCC;
        private DmoResampler dmoResampler;
        private SplineResampler splineResampler;

        [ParamsSource(nameof(ValuesForConversionRatio))]
        public (int before, int after) ConversionRatio { get; set; }

        public IEnumerable<(int, int)> ValuesForConversionRatio => new[] { (44100, 192000), (48000, 192000) };

        private float[] bufferS;
        private byte[] bufferCC;

        [Params(1, 2)]
        public int Channels { get; set; }

        [Params(4095)]
        public int Frames { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            sourceMA = new DummySource<float, SampleFormat>(new SampleFormat(Channels, ConversionRatio.before));
            sourceCC = new DummySourceCSCore(new CSCore.WaveFormat(ConversionRatio.before, 32, Channels));
            splineResampler = new SplineResampler(sourceMA, ConversionRatio.after);
            dmoResampler = new DmoResampler(sourceCC, new CSCore.WaveFormat(ConversionRatio.after, 32, Channels, CSCore.AudioEncoding.IeeeFloat));

            bufferS = new float[Frames * Channels];
            bufferCC = new byte[sizeof(float) * bufferS.Length];
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            splineResampler?.Dispose();
            sourceMA?.Dispose();
            dmoResampler?.Dispose();
            sourceCC?.Dispose();
            bufferS = null;
        }

        [Benchmark]
        public void ShamisenSplineResampler()
        {
            var span = bufferS.AsSpan();
            _ = splineResampler.Read(span);
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(2)]
        [Arguments(3)]
        [Arguments(4)]
        public void CSCoreDmoResampler(int quality)
        {
            dmoResampler.Quality = quality;
            _ = dmoResampler.Read(bufferCC, 0, bufferS.Length);
        }

        /*
``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1320 (21H1/May2021Update)
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT


```
|                  Method | Channels | Frames | ConversionRatio | quality |      Mean |     Error |    StdDev | Frame Throughput [Frames/s] | Code Size |
|------------------------ |--------- |------- |---------------- |-------- |----------:|----------:|----------:|----------------------------:|----------:|
|      **CSCoreDmoResampler** |        **1** |   **4095** | **(44100, 192000)** |       **1** | **18.177 μs** | **0.1513 μs** | **0.1416 μs** |        **225,284,993.95490300** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **1** |   **4095** | **(44100, 192000)** |       **2** | **18.106 μs** | **0.1230 μs** | **0.1150 μs** |        **226,166,321.02770300** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **1** |   **4095** | **(44100, 192000)** |       **3** | **19.622 μs** | **0.1080 μs** | **0.1010 μs** |        **208,692,223.57398900** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **1** |   **4095** | **(44100, 192000)** |       **4** | **19.236 μs** | **0.0978 μs** | **0.0867 μs** |        **212,880,008.70290500** |   **1,111 B** |
| **ShamisenSplineResampler** |        **1** |   **4095** | **(44100, 192000)** |       **?** |  **8.979 μs** | **0.0934 μs** | **0.0873 μs** |        **456,056,322.53928700** |   **9,346 B** |
|      **CSCoreDmoResampler** |        **1** |   **4095** | **(48000, 192000)** |       **1** | **16.973 μs** | **0.1083 μs** | **0.0846 μs** |        **241,258,825.40439900** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **1** |   **4095** | **(48000, 192000)** |       **2** | **17.050 μs** | **0.1526 μs** | **0.1353 μs** |        **240,169,569.41378700** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **1** |   **4095** | **(48000, 192000)** |       **3** | **18.762 μs** | **0.1506 μs** | **0.1409 μs** |        **218,256,522.86044900** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **1** |   **4095** | **(48000, 192000)** |       **4** | **17.693 μs** | **0.0988 μs** | **0.0925 μs** |        **231,452,228.03430900** |   **1,111 B** |
| **ShamisenSplineResampler** |        **1** |   **4095** | **(48000, 192000)** |       **?** |  **1.808 μs** | **0.0122 μs** | **0.0109 μs** |      **2,265,509,029.78610000** |   **9,441 B** |
|      **CSCoreDmoResampler** |        **2** |   **4095** | **(44100, 192000)** |       **1** | **26.051 μs** | **0.1861 μs** | **0.1453 μs** |        **157,190,303.17641400** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **2** |   **4095** | **(44100, 192000)** |       **2** | **26.212 μs** | **0.3789 μs** | **0.3358 μs** |        **156,228,385.72790700** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **2** |   **4095** | **(44100, 192000)** |       **3** | **28.135 μs** | **0.1765 μs** | **0.1651 μs** |        **145,545,924.59322200** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **2** |   **4095** | **(44100, 192000)** |       **4** | **28.075 μs** | **0.1982 μs** | **0.1757 μs** |        **145,857,803.75858100** |   **1,111 B** |
| **ShamisenSplineResampler** |        **2** |   **4095** | **(44100, 192000)** |       **?** | **11.075 μs** | **0.1201 μs** | **0.1064 μs** |        **369,763,759.66472400** |   **9,763 B** |
|      **CSCoreDmoResampler** |        **2** |   **4095** | **(48000, 192000)** |       **1** | **24.413 μs** | **0.1006 μs** | **0.0941 μs** |        **167,736,130.04442200** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **2** |   **4095** | **(48000, 192000)** |       **2** | **24.915 μs** | **0.1503 μs** | **0.1406 μs** |        **164,356,285.43587100** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **2** |   **4095** | **(48000, 192000)** |       **3** | **26.761 μs** | **0.4430 μs** | **0.3927 μs** |        **153,020,894.28728400** |   **1,111 B** |
|      **CSCoreDmoResampler** |        **2** |   **4095** | **(48000, 192000)** |       **4** | **26.241 μs** | **0.0920 μs** | **0.0816 μs** |        **156,055,329.72568200** |   **1,111 B** |
| **ShamisenSplineResampler** |        **2** |   **4095** | **(48000, 192000)** |       **?** |  **2.618 μs** | **0.0235 μs** | **0.0208 μs** |      **1,564,005,443.70365000** |   **7,913 B** |

         * */
    }
}
