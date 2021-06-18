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

using Shamisen.Codecs.Flac;
using Shamisen.Codecs.Flac.Metadata;
using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Codecs.Waveform.Parsing;
using Shamisen.Data;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Flac
{
    [TestFixture]
    public class FlacParserTests
    {
        public static IEnumerable<FileInfo> FlacParserParsesCorrectlyTestCaseGenerator()
            => new[] { "./Samples", "./Songs" }.Where(a => Directory.Exists(a)).SelectMany(a => Directory.EnumerateFiles(a, "*.zip", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true }))
            .Select(a => new FileInfo(a)).Where(a => a.Exists);

        [TestCaseSource(nameof(FlacParserParsesCorrectlyTestCaseGenerator))]
        [NonParallelizable]
        public void FlacParserParsesCorrectly(FileInfo path)
        {
            Assert.IsTrue(path.Exists);
            using var y = path.OpenRead();
            using var archive = new ZipArchive(y, ZipArchiveMode.Read);
            var flacFile = archive.Entries.First(a => a.Name.EndsWith(".flac"));
            var wavFile = archive.Entries.First(a => a.Name.EndsWith(".wav"));
            using var flacStream = flacFile.Open();
            using var wavStream = wavFile.Open();
            var t = new Stopwatch();
            t.Start();
            using var flacMemory = new MemoryStream();
            flacStream.CopyTo(flacMemory);
            flacMemory.Position = 0;
            using var wavMemory = new MemoryStream();
            wavStream.CopyTo(wavMemory);
            wavMemory.Position = 0;
            t.Stop();
            Console.WriteLine($"File preparation took {t.Elapsed.TotalSeconds}[s]");
            using var flacSource = new StreamDataSource(flacMemory);
            using var wavSource = new StreamDataSource(wavMemory);
            using var wav = new SimpleWaveParser(new SimpleChunkParserFactory(), wavSource);
            Console.WriteLine($"Format: {wav.Format.SampleRate}Hz, {wav.Format.BitDepth}bit LinearPCM, {wav.Format.Channels}ch");
            Console.WriteLine($"TotalLength : {wav.TotalLength}");
            int size = (int)wav.TotalLength * wav.Format.Channels;
            switch (wav.Format.BitDepth)
            {
                case 24:
                    Compare24(flacSource, wav, size);
                    break;
                case 16:
                    Compare16(flacSource, wav, size);
                    break;
                default:
                    break;
            }
        }

        private static void Compare24(StreamDataSource flacSource, SimpleWaveParser wav, int size)
        {
            var t = new Stopwatch();
            t.Start();
            var dataF = new PooledArray<int>(size);
            var dataW = new PooledArray<Int24>(size);
            t.Stop();
            Console.WriteLine($"Memory preparation took {t.Elapsed.TotalSeconds}[s]");
            t.Restart();
            var rw = wav.Read(MemoryMarshal.Cast<Int24, byte>(dataW.Span));
            t.Stop();
            Console.WriteLine($"WAVE Decoding took {t.Elapsed.TotalSeconds}[s]");
            t.Restart();
            using var flac = new FlacParser(flacSource, new FlacParserOptions(true, true, true, true, true, true));
            var rr = flac.Read(MemoryMarshal.Cast<int, byte>(dataF.Span));
            t.Stop();
            var duration = (double)wav.TotalLength / wav.Format.SampleRate;
            Console.WriteLine($"FLAC Decoding took {t.Elapsed.TotalSeconds}[s]\n(around {duration / t.Elapsed.TotalSeconds} times faster than real time)");
            Assert.AreEqual(size * sizeof(int), rr.Length);
            Assert.AreEqual(rw.Length / 3, rr.Length / sizeof(int));
            Debug.WriteLine("Comparing!");
            Assert.Multiple(() =>
            {
                t.Restart();
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
                t.Stop();
                Console.WriteLine($"Comparison took {t.Elapsed.TotalSeconds}[s]");
            });
            if (flac.CueSheet.HasValue)
            {
                DumpCueSheet(flac.CueSheet.Value);
            }
            if (flac.Comment.HasValue)
            {
                DumpComment(flac.Comment.Value);
            }
            if (!flac.ApplicationMetadata.IsEmpty)
            {
                DumpApplicationMetadata(flac.ApplicationMetadata);
            }
            if (!flac.Pictures.IsEmpty)
            {
                DumpPictures(flac.Pictures);
            }
        }

        private static void Compare16(StreamDataSource flacSource, SimpleWaveParser wav, int size)
        {
            var t = new Stopwatch();
            t.Start();
            var dataF = new PooledArray<int>(size);
            var dataW = new PooledArray<short>(size);
            t.Stop();
            Console.WriteLine($"Memory preparation took {t.Elapsed.TotalSeconds}[s]");
            t.Restart();
            using var flac = new FlacParser(flacSource, new FlacParserOptions(true, true, true, true, true, true));
            var rr = flac.Read(MemoryMarshal.Cast<int, byte>(dataF.Span));
            t.Stop();
            var duration = (double)wav.TotalLength / wav.Format.SampleRate;
            Console.WriteLine($"FLAC Decoding took {t.Elapsed.TotalSeconds}[s]\n(around {duration / t.Elapsed.TotalSeconds} times faster than real time)");
            Assert.AreEqual(size * sizeof(int), rr.Length);
            t.Restart();
            var rw = wav.Read(MemoryMarshal.Cast<short, byte>(dataW.Span));
            t.Stop();
            Console.WriteLine($"WAVE Decoding took {t.Elapsed.TotalSeconds}[s]");
            Assert.AreEqual(rw.Length / sizeof(short), rr.Length / sizeof(int));
            Debug.WriteLine("Comparing!");
            Assert.Multiple(() =>
            {
                t.Restart();
                var err = 0ul;
                var sw = dataW.Span;
                var sf = dataF.Span;
                for (int i = 0; i < sw.Length; i++)
                {
                    if (sw[i] != (short)sf[i])
                    {
                        Assert.AreEqual(sw[i], (short)sf[i], $"Comparing {i}th element");
                        err++;
                        if (err > 128) break;
                    }
                }
                t.Stop();
                Console.WriteLine($"Comparison took {t.Elapsed.TotalSeconds}[s]");
            });
            if (flac.CueSheet.HasValue)
            {
                DumpCueSheet(flac.CueSheet.Value);
            }
            if (flac.Comment.HasValue)
            {
                DumpComment(flac.Comment.Value);
            }
            if (!flac.ApplicationMetadata.IsEmpty)
            {
                DumpApplicationMetadata(flac.ApplicationMetadata);
            }
            if (!flac.Pictures.IsEmpty)
            {
                DumpPictures(flac.Pictures);
            }
        }

        private static void DumpPictures(ReadOnlyMemory<FlacPicture> pictures)
        {
            Console.WriteLine($"Pictures:");
            var sb = new StringBuilder();
            for (int i = 0; i < pictures.Span.Length; i++)
            {
                var g = pictures.Span[i];
                _ = sb.AppendLine($"\tPictureType: {g.PictureType}");
                _ = sb.AppendLine($"\tMimeType: {Encoding.UTF8.GetString(g.MimeType.Span)}");
                _ = sb.AppendLine($"\tDescription: {Encoding.UTF8.GetString(g.Description.Span)}");
                _ = sb.AppendLine($"\tWidth x Height: {g.Width} x {g.Height}");
                _ = sb.AppendLine($"\tColorDepth: {g.ColorDepth}");
                _ = sb.AppendLine($"\tPallets: {g.IndexedColors}");
                _ = sb.AppendLine($"\tLength: {g.Data.Length}");
            }
            Console.WriteLine(sb.ToString());
        }

        private static void DumpApplicationMetadata(ReadOnlyMemory<FlacApplicationMetadata> applicationMetadata)
        {
            Console.WriteLine($"Application Metadata:");
            var sb = new StringBuilder();
            for (int i = 0; i < applicationMetadata.Span.Length; i++)
            {
                var g = applicationMetadata.Span[i];
                _ = sb.AppendLine($"\tID: {g.Id}");
                _ = g.Data.Length < 128
                    ? sb.AppendLine($"\tData: \n{DebugUtils.DumpBinary(g.Data.Span)}")
                    : sb.AppendLine($"\tData.Length: {g.Data.Length}");
            }
            Console.WriteLine(sb.ToString());
        }

        private static void DumpComment(VorbisComment value)
        {
            Console.WriteLine($"VorbisComment: ");
            var sb = new StringBuilder();
            _ = sb.AppendLine($"\tVendor: {value.Vendor}");
            _ = sb.AppendLine($"\tUserComments:\n\t\t{string.Join("\n\t\t", value.UserComments)}");
            Console.WriteLine(sb.ToString());
        }

        private static void DumpCueSheet(FlacCueSheet value)
        {
            //
        }
    }
}
