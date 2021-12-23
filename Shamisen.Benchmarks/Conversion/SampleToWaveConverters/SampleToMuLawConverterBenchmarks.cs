﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.SampleToWaveConverters;

namespace Shamisen.Benchmarks.Conversion.SampleToWaveConverters
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: short.MaxValue)]
    public class SampleToMuLawConverterBenchmarks
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
        private float[] srcBuffer;
        private byte[] dstBuffer;
        private const int SampleRate = 192000;

        [Params(/*2047, */4095, Priority = -990)]
        public int Frames { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            dstBuffer = new byte[Frames];
            srcBuffer = new float[Frames];
            var g = MemoryMarshal.AsBytes(srcBuffer.AsSpan());
            RandomNumberGenerator.Fill(g);
        }

        [Benchmark]
        public void ProcessStandardVectorized() => SampleToMuLawConverter.ProcessStandardVectorized(srcBuffer, dstBuffer);

        [Benchmark(Baseline = true)]
        public void ProcessAvx2() => SampleToMuLawConverter.ProcessAvx2(srcBuffer, dstBuffer);

    }
}