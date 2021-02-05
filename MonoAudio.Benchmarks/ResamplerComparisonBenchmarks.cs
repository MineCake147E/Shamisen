using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Synthesis;
using CSCore.DSP;

namespace MonoAudio.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    public class ResamplerComparisonBenchmarks
    {
        private DummySource<float, SampleFormat> sourceMA;
        private DummySourceCSCore sourceCC;
        private DmoResampler dmoResampler;
        private SplineResampler splineResampler;

        [ParamsSource(nameof(ValuesForConversionRatio))]
        public (int before, int after) ConversionRatio { get; set; }

        public IEnumerable<(int, int)> ValuesForConversionRatio => new[] { (44100, 192000) };

        private float[] bufferMA;
        private byte[] bufferCC;

        [Params(1, 2)]
        public int Channels { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            sourceMA = new DummySource<float, SampleFormat>(new SampleFormat(Channels, ConversionRatio.before));
            sourceCC = new DummySourceCSCore(new CSCore.WaveFormat(ConversionRatio.before, 32, Channels));
            splineResampler = new SplineResampler(sourceMA, ConversionRatio.after);
            dmoResampler = new DmoResampler(sourceCC, new CSCore.WaveFormat(ConversionRatio.after, 32, Channels, CSCore.AudioEncoding.IeeeFloat));

            bufferMA = new float[2560 * Channels];
            bufferCC = new byte[sizeof(float) * bufferMA.Length];
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            splineResampler?.Dispose();
            sourceMA?.Dispose();
            dmoResampler?.Dispose();
            sourceCC?.Dispose();
            bufferMA = null;
        }

        [Benchmark]
        public void MonoAudioSplineResampler()
        {
            var span = bufferMA.AsSpan();
            _ = splineResampler.Read(span);
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(3)]
        [Arguments(4)]
        [Arguments(30)]
        [Arguments(60)]
        public void CSCoreDmoResampler(int quality)
        {
            dmoResampler.Quality = quality;
            _ = dmoResampler.Read(bufferCC, 0, bufferMA.Length);
        }

        /*
// * Summary *

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
  [Host]        : .NET Framework 4.8 (4.8.4300.0), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT

Job=.NET Core 5.0  Runtime=.NET Core 5.0

|                   Method | Channels | ConversionRatio | quality |      Mean |     Error |    StdDev |
|------------------------- |--------- |---------------- |-------- |----------:|----------:|----------:|
|       CSCoreDmoResampler |        1 | (44100, 192000) |       1 | 12.035 us | 0.2263 us | 0.2117 us |
|       CSCoreDmoResampler |        1 | (44100, 192000) |       3 | 12.646 us | 0.2218 us | 0.2075 us |
|       CSCoreDmoResampler |        1 | (44100, 192000) |       4 | 12.683 us | 0.2061 us | 0.1927 us |
|       CSCoreDmoResampler |        1 | (44100, 192000) |      30 | 22.155 us | 0.3271 us | 0.3060 us |
|       CSCoreDmoResampler |        1 | (44100, 192000) |      60 | 29.361 us | 0.3782 us | 0.3538 us |
| MonoAudioSplineResampler |        1 | (44100, 192000) |       ? |  8.461 us | 0.1104 us | 0.0979 us |
|       CSCoreDmoResampler |        2 | (44100, 192000) |       1 | 16.995 us | 0.3155 us | 0.2951 us |
|       CSCoreDmoResampler |        2 | (44100, 192000) |       3 | 18.293 us | 0.3137 us | 0.2781 us |
|       CSCoreDmoResampler |        2 | (44100, 192000) |       4 | 18.293 us | 0.2433 us | 0.2276 us |
|       CSCoreDmoResampler |        2 | (44100, 192000) |      30 | 33.671 us | 0.2396 us | 0.2124 us |
|       CSCoreDmoResampler |        2 | (44100, 192000) |      60 | 45.398 us | 0.4979 us | 0.4658 us |
| MonoAudioSplineResampler |        2 | (44100, 192000) |       ? | 15.956 us | 0.2922 us | 0.2590 us |
         * */
    }
}
