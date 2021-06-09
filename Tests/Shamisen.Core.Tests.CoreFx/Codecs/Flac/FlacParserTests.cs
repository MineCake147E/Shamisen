using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Codecs.Flac.Parsing;
using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Codecs.Waveform.Parsing;
using Shamisen.Data;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Flac
{
    [TestFixture]
    public class FlacParserTests
    {
        public static IEnumerable<FileInfo> FlacParserParsesCorrectlyTestCaseGenerator()
            => Directory.EnumerateFiles("./Resources/", "*.zip", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive })
            .Select(a => new FileInfo(a)).Where(a => a.Exists);

        [TestCaseSource(nameof(FlacParserParsesCorrectlyTestCaseGenerator))]
        public void FlacParserParsesCorrectly(FileInfo path)
        {
            Assert.IsTrue(path.Exists);
            using var y = path.OpenRead();
            using var archive = new ZipArchive(y, ZipArchiveMode.Read);
            var flacFile = archive.Entries.First(a => a.Name.EndsWith(".flac"));
            var wavFile = archive.Entries.First(a => a.Name.EndsWith(".wav"));
            using var flacStream = flacFile.Open();
            using var wavStream = wavFile.Open();
            using var flacSource = new StreamDataSource(flacStream);
            using var wavSource = new StreamDataSource(wavStream);
            using var flac = new FlacParser(flacSource);
            using var wav = new SimpleWaveParser(new SimpleChunkParserFactory(), wavSource);
            int size = (int)flac.TotalLength * flac.Format.Channels;
            using var dataF = new PooledArray<int>(size);
            using var dataW = new PooledArray<Int24>(size);
            var rr = flac.Read(MemoryMarshal.Cast<int, byte>(dataF.Span));
            Assert.AreEqual(size * sizeof(int), rr.Length);
            var rw = wav.Read(MemoryMarshal.Cast<Int24, byte>(dataW.Span));
            Assert.AreEqual(rw.Length / 3, rr.Length / sizeof(int));
            Debug.WriteLine("Comparing!");
            Assert.Multiple(() =>
            {
                var err = 0ul;
                var sw = dataW.Span;
                var sf = dataF.Span;
                for (int i = 0; i < sw.Length; i++)
                {
                    if (sw[i] != (int)(Int24)sf[i])
                    {
                        Assert.AreEqual((int)sw[i], (int)(Int24)sf[i], $"Comparing {i}th element");
                        err++;
                        if (err > 128) break;
                    }
                }
            });
        }
    }
}
