﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks.SplineResamplerBenchmarks
{
    //[DryJob()]
    [SimpleJob(RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    //[SimpleJob(RuntimeMoniker.Mono, baseline: true)]
    public partial class ResamplerBenchmarks
    {
        private SplineResampler resampler;
        private IReadableAudioSource<float, SampleFormat> source;
        private float[] buffer;
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

        [ParamsSource(nameof(ValuesForConversionRatio), Priority = 0)]

        public ConversionRatioProps ConversionRatio { get; set; }

        public IEnumerable<ConversionRatioProps> ValuesForConversionRatio => new ConversionRatioProps[] {
            //new (24000, 154320, "CachedWrappedOdd"),    //Example of CachedWrappedOdd
            new (176400, 192000, "CachedDirect"),       //UpAnyRateUnrolled, Often used(equivalent to 44100Hz → 48000Hz)
            //new (44100, 154320, "Direct"),              //Example of Direct, Might be slowest
            //new (44100, 192000, "CachedWrappedEven"),   //Example of CachedWrappedEven, Often used
            //new (48000, 192000, "CachedDirect"),        //Quadruple Rate
            //new (64000, 192000, "CachedDirect"),        //Integer Rate
            //new (96000, 192000, "CachedDirect"),        //Double Rate
        };

        [Params(1, /*2, 3, 4, 5, 6, 7, 8, 9, 10,*/ Priority = -4)]
        public int Channels { get; set; }
        [Params(/*2047, */4095, Priority = -990)]
        public int Frames { get; set; }

        public IEnumerable<X86Intrinsics> X86IntrinsicsMasks => new[] {
            IntrinsicsUtils.X86Intrinsics,
            //X86Intrinsics.None
        };

        [ParamsSource(nameof(X86IntrinsicsMasks), Priority = 100)]
        public X86Intrinsics EnabledX86Intrinsics { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new DummySource<float, SampleFormat>(new SampleFormat(Channels, ConversionRatio.Before));
            resampler = new SplineResampler(source, ConversionRatio.After, EnabledX86Intrinsics);
            buffer = new float[Frames * Channels];
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            resampler?.Dispose();
            source?.Dispose();
            buffer = null;
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible), Benchmark]
        public void SplineResampler()
        {
            var span = buffer.AsSpan();
            _ = resampler.Read(span);
        }
    }
}
