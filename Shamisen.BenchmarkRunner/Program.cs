using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Shamisen.Benchmarks.Running
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkSwitcher
            .FromAssembly(typeof(ResamplerBenchmarks).Assembly)
            .Run(args, DefaultConfig.Instance.WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(256)));
            Console.Write("Press any key to exit:");
            Console.ReadKey();
        }
    }
}
