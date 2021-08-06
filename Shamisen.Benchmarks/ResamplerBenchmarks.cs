using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net50)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: 256)]
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
                _ = AddColumn(new PlaybackSpeedColumn(
                    a => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value,
                    a => ((ConversionRatioProps)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "ConversionRatio")).Value).After));
            }
        }
        #endregion


        [ParamsSource(nameof(ValuesForConversionRatio), Priority =0)]
        
        public ConversionRatioProps ConversionRatio { get; set; }

        public IEnumerable<ConversionRatioProps> ValuesForConversionRatio => new ConversionRatioProps[] {
            new (24000, 154320, "CachedWrappedOdd"),    //Example of CachedWrappedOdd
            new (44100, 48000, "CachedDirect"),         //Often used
            new (44100, 154320, "Direct"),              //Example of Direct, Might be slowest
            new (44100, 192000, "CachedWrappedEven"),   //Often used
            new (48000, 192000, "CachedDirect"),        //Quadruple Rate
            new (64000, 192000, "CachedDirect"),        //Integer Rate
            new (96000, 192000, "CachedDirect"),        //Double Rate
        };

        [Params(1, Priority =-4)]
        public int Channels { get; set; }
        [Params(2881, Priority = -990)]
        public int Frames { get; set; }
        [Params(/*(X86Intrinsics)X86IntrinsicsMask.None, (X86Intrinsics)X86IntrinsicsMask.Ssse3, */(X86Intrinsics)X86IntrinsicsMask.Avx2, Priority =100)]
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

        [Benchmark]
        public void SplineResampler()
        {
            var span = buffer.AsSpan();
            _ = resampler.Read(span);
        }
    }

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct ConversionRatioProps
    {
        public int Before { get; set; }
        public int After { get; set; }
        public string Strategy { get; set; }

        public ConversionRatioProps(int before, int after, string strategy)
        {
            Before = before;
            After = after;
            Strategy = strategy;
        }

        public override bool Equals(object obj) => obj is ConversionRatioProps other && Before == other.Before && After == other.After && Strategy == other.Strategy;
        public override int GetHashCode() => HashCode.Combine(Before, After, Strategy);

        public void Deconstruct(out int before, out int after, out string strategy)
        {
            before = Before;
            after = After;
            strategy = Strategy;
        }

        public static implicit operator (int before, int after, string strategy)(ConversionRatioProps value) => (value.Before, value.After, value.Strategy);
        public static implicit operator ConversionRatioProps((int before, int after, string strategy) value) => new ConversionRatioProps(value.before, value.after, value.strategy);

        public static bool operator ==(ConversionRatioProps left, ConversionRatioProps right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConversionRatioProps left, ConversionRatioProps right)
        {
            return !(left == right);
        }

        private string GetDebuggerDisplay()
        {
            return $"{Before} -> {After} ({Strategy})";
        }

        public override string ToString() => GetDebuggerDisplay();
    }
}
