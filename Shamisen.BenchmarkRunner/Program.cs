using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using Shamisen.Benchmarks.SplineResamplerBenchmarks;

namespace Shamisen.Benchmarks.Running
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkSwitcher
            .FromAssembly(typeof(ResamplerBenchmarks).Assembly)
            .Run(args, DefaultConfig.Instance.WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(256)).AddDiagnoser(new DisassemblyDiagnoser(new(int.MaxValue)))
            );
            Console.Write("Press any key to exit:");
            Console.ReadKey();
        }
    }
}
