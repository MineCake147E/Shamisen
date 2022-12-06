using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
    public class MultiplyAllSingleBenchmarks
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

        private ComplexF[] x, y;
        [GlobalSetup]
        public void Setup()
        {
            x = new ComplexF[Frames];
            y = new ComplexF[Frames];
        }

        [Benchmark]
        public void MultiplyAllFallback() => ComplexUtils.Fallback.MultiplyAllFallback(y, x, new ComplexF(0.28f, 1.45f));
        [Benchmark]
        public void MultiplyAllAvx() => ComplexUtils.X86.MultiplyAllAvx(y, x, new ComplexF(0.28f, 1.45f));
    }
}
