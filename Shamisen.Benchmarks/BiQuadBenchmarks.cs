using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net50, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [DisassemblyDiagnoser(maxDepth: 16)]
    public class BiQuadBenchmarks
    {
        private IReadableAudioSource<float, SampleFormat> source;
        private BiQuadFilter filter;
        private float[] buffer;
        private const int SampleRate = 192000;
        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new PlaybackSpeedColumn(
                    a => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value,
                    a => SampleRate));
            }
        }
        [Params(2)]
        public int Channels { get; set; }
        [Params(1441)]
        public int Frames { get; set; }

        [Params(true)]
        public bool EnableIntrinsics { get; set; }

        [Params(/*(X86Intrinsics)X86IntrinsicsMask.None, */(X86Intrinsics)X86IntrinsicsMask.Sse42, (X86Intrinsics)X86IntrinsicsMask.Avx2)]
        public X86Intrinsics EnabledX86Intrinsics { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<float, SampleFormat>(new SampleFormat(Channels, SampleRate));
            filter = new BiQuadFilter(source, BiQuadParameter.CreateLPFParameter(SampleRate, 48000, 1), EnableIntrinsics, EnabledX86Intrinsics, IntrinsicsUtils.ArmIntrinsics);
            buffer = new float[Frames * Channels];
        }

        [Benchmark]
        public void BiQuadFilter()
        {
            var span = buffer.AsSpan();
            _ = filter.Read(span);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            filter?.Dispose();
            source?.Dispose();
            buffer = null;
        }
    }
}
