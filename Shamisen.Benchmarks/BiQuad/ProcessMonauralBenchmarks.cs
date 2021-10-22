using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Filters;

namespace Shamisen.Benchmarks.BiQuad
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class ProcessMonauralBenchmarks
    {
        private const int SampleRate = 192000;
        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new FrameThroughputColumn(a => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames", StringComparison.Ordinal)).Value));
            }
        }
        [Params(8191)]
        public int Frames { get; set; }
        private float[] buffer;
        private BiQuadParameter parameter;
        private Vector2[] states;
        [GlobalSetup]
        public void Setup()
        {
            buffer = new float[Frames];
            states = new Vector2[1];
            parameter = BiQuadParameter.CreateLPFParameter(SampleRate, 22050, 0.70710678118654752440084436210485);
        }
        [Benchmark]
        public void ProcessMonauralStandard() => BiQuadFilter.ProcessMonauralStandard(buffer, parameter, states);

        [GlobalCleanup]
        public void Cleanup() => buffer = null;
    }
}
