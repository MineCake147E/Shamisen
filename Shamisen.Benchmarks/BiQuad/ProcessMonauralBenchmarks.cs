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
    [SimpleJob(RuntimeMoniker.Net50, baseline: true)]
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
                _ = AddColumn(new PlaybackSpeedColumn(
                    a => (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value,
                    a => SampleRate));
            }
        }
        [Params(2053)]
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
        public void ProcessMonauralSse() => BiQuadFilter.ProcessMonauralSse(buffer, parameter, states);
        [Benchmark]
        public void ProcessMonauralSseM1() => BiQuadFilter.ProcessMonauralSseM1(buffer, parameter, states);
        [Benchmark]
        public void ProcessMonauralSseM2() => BiQuadFilter.ProcessMonauralSseM2(buffer, parameter, states);
        [GlobalCleanup]
        public void Cleanup()
        {
            buffer = null;
        }
    }
}
