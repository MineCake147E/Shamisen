using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace MonoAudio.Benchmarks.Running
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkSwitcher
            .FromAssembly(typeof(ResamplerBenchmarks).Assembly)
            .Run(args);
            Console.Write("Press any key to exit:");
            Console.ReadKey();
        }
    }
}
