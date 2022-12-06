using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Analysis;

namespace Shamisen.Benchmarks.Analysis
{
    [SimpleJob(RuntimeMoniker.HostProcess)]
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class SingleFftBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                //static int FrameSelector(BenchmarkDotNet.Running.BenchmarkCase a)
                //{
                //    var n = (int)a.Parameters.Items.FirstOrDefault(a => string.Equals(a.Name, "Frames")).Value;
                //    return 5 * n * MathI.LogBase2((uint)n);
                //}
                //_ = AddColumn(new FrameThroughputColumn(FrameSelector) { ColumnName = "FFT Throughput [MFLOPS]", Legend = "FLOPs processed per second" });
                _ = AddColumn(new FrameThroughputColumn(a => 1) { ColumnName = "FFT Throughput [FFTs/s]", Legend = "FFTs processed per second" });
            }
        }
        [Params(131072, 65536, 32768, 16384, 8192, 4096, 2048, 1024)]
        public int Frames { get; set; }
        private ComplexF[] x;

        [GlobalSetup]
        public void Setup() => x = new ComplexF[Frames];//RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(x.AsSpan()));//Pcm32ToSampleConverter.ProcessNormal(MemoryMarshal.Cast<ComplexF, float>(x.AsSpan()));

        //[Benchmark]
        //public void PerformForward() => FastFourierTransformation.FFT(x, FftMode.Forward);
        [Benchmark]
        public void PerformBackward() => CooleyTukeyFft.FFT(x, FftMode.Backward);
    }
}
