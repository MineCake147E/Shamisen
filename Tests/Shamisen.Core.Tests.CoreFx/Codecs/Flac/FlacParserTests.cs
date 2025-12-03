using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
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
        private static readonly string[] SourceArray = ["./Samples", "./Songs"];

        public static IEnumerable<TestCaseData> FlacParserParsesCorrectlyTestCaseGenerator()
            => SourceArray.Where(Directory.Exists).SelectMany(a => new DirectoryInfo(a).EnumerateFiles("*.zip", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true }))
            .Where(a => a.Exists).Select(a => new TestCaseData(a).SetArgDisplayNames($"\"{Path.GetRelativePath(Environment.CurrentDirectory, a.FullName)}\""));

        [TestCaseSource(nameof(FlacParserParsesCorrectlyTestCaseGenerator))]
        public void FlacParserParsesCorrectly(FileInfo path)
        {
            Assert.That(path.Exists);
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
            Console.WriteLine($"WAVE Format: {wav.Format.SampleRate}Hz, {wav.Format.BitDepth}bit {wav.Format.Encoding}, {wav.Format.Channels}ch");
            Console.WriteLine($"WAVE TotalLength : {wav.TotalLength}");
            using var flac = new FlacParser(flacSource, new FlacParserOptions(true, true, true, true, true, true));
            DumpFlacMetadata(flac);
            switch (wav.Format.BitDepth)
            {
                case 32:
                    Compare<int>(flac, wav);
                    break;
                case 24:
                    Compare<Int24>(flac, wav);
                    break;
                case 16:
                    Compare<short>(flac, wav);
                    break;
                case 8:
                    Compare<OffsetSByte>(flac, wav);
                    break;
                default:
                    Assert.Fail($"Bit depth {wav.Format.BitDepth} is not supported!");
                    break;
            }
        }

        private static void Compare<T>(FlacParser flac, SimpleWaveParser wav) where T : unmanaged, IBinaryNumber<T>, ISignedNumber<T>
        {
            var t = new Stopwatch();
            t.Start();
            Debug.WriteLine("Comparing!");
            ulong err = 0ul;
            ulong frameIndex = 0ul;
            ulong wavLength = 0ul;
            ulong flacLength = 0ul;
            while (true)
            {
                var frame = flac.PrepareNextFrame();
                if (frame is null) break;
                using var dataF = new PooledArray<int>((int)frame.TotalLength.GetValueOrDefault(int.MaxValue) * frame.Format.Channels);
                using var dataW = new PooledArray<T>(dataF.Length);
                var shiftsNeeded = wav.Format.BitDepth - frame.Format.EffectiveBitDepth;
                var sw = dataW.Span;
                var sf = dataF.Span;
                var rrf = flac.ReadFrame(sf);
                var rrw = wav.Read(MemoryMarshal.Cast<T, byte>(sw));
                Assert.That(rrf, Is.EqualTo(rrw / Unsafe.SizeOf<T>()), $"FLAC ReadResult length mismatch in frame #{frameIndex} ({frame})");
                wavLength += (ulong)(rrw.Length / Unsafe.SizeOf<T>());
                flacLength += (ulong)rrf.Length;
                using (Assert.EnterMultipleScope())
                {
                    for (int i = 0; i < sf.Length; i++)
                    {
                        var vw = long.CreateChecked(sw[i]);
                        var vf = sf[i] << shiftsNeeded;
                        if (vw != vf)
                        {
                            Assert.That(vf, Is.EqualTo(vw), $"Comparing element #{i} in frame #{frameIndex} ({frame})");
                            err++;
                            if (err > 128) break;
                        }
                    }
                }
                frameIndex++;
            }
            Assert.That(flacLength, Is.EqualTo(wavLength), "Total length mismatch!");
        }

        private static void DumpFlacMetadata(FlacParser flac)
        {
            DumpStreamInfo(flac.StreamInfoBlock);
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

        private static void DumpStreamInfo(FlacStreamInfoBlock streamInfoBlock)
        {
            Console.WriteLine($"FLAC Stream Info:");
            Console.WriteLine(streamInfoBlock);
        }

        private static void DumpPictures(ReadOnlyMemory<FlacPicture> pictures)
        {
            Console.WriteLine($"FLAC Pictures:");
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
            Console.WriteLine($"FLAC Application Metadata:");
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
            Console.WriteLine($"FLAC VorbisComment: ");
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
