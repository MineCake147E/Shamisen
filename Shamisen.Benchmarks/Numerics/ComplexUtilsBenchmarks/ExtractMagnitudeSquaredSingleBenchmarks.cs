using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Numerics;

namespace Shamisen.Benchmarks.Numerics.ComplexUtilsBenchmarks
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]

    public class ExtractMagnitudeSquaredSingleBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                static int frameSelector(BenchmarkDotNet.Running.BenchmarkCase a) => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                //_ = AddColumn(new PlaybackSpeedColumn(frameSelector, a => SampleRate));
                _ = AddColumn(new FrameThroughputColumn(frameSelector));
            }
        }

        [Params(4095)]
        public int Frames { get; set; }

        private ComplexF[] src;
        private float[] dst;
        [GlobalSetup]
        public void Setup()
        {
            src = new ComplexF[Frames];
            dst = new float[Frames];
        }
        [Benchmark]
        public void ExtractMagnitudeSquaredFallback() => ComplexUtils.Fallback.ExtractMagnitudeSquaredFallback(dst, src);
        [Benchmark]
        public void ExtractMagnitudeSquaredAvx2() => ComplexUtils.X86.ExtractMagnitudeSquaredAvx2(dst, src);
    }
}
