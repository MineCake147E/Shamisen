using System.IO;
using System.Reflection;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Shamisen.Codecs.Flac;
using Shamisen.Data;

namespace Shamisen.Benchmarks
{
    [SimpleJob(RuntimeMoniker.HostProcess, baseline: true)]
    /*[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Mono)]*/
    [Config(typeof(Config))]
    [DisassemblyDiagnoser(maxDepth: int.MaxValue)]
    public class FlacDecodingBenchmarks
    {
        private MemoryStream ms;
        private StreamDataSource src;
        private byte[] buf;
        private class Config : ManualConfig
        {
            public Config()
            {
                _ = AddColumn(new FrameThroughputColumn(a => 192000 * 1 * 60));
            }
        }
        [GlobalSetup]
        public void Setup()
        {
            ms = GetDataStreamFromResource("PinkNoise_24bit_Lv8.flac");
            src = new StreamDataSource(ms);
            buf = new byte[192000 * sizeof(int) * 1 * 60];
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            ms.Position = 0;
        }

        [Benchmark]
        public void Decode()
        {
            using (var decoder = new FlacParser(src))
            {
                _ = decoder.Read(buf);
            }
        }

        private static MemoryStream GetDataStreamFromResource(string name)
        {
            var lib = Assembly.GetExecutingAssembly();
            var ms = new MemoryStream();
            using (var stream = lib.GetManifestResourceStream($"Shamisen.Benchmarks.TestSounds.{name}"))
            {
                stream.CopyTo(ms);
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
