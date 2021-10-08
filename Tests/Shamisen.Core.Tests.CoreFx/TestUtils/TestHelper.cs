using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Data;
using Shamisen.Filters;
using Shamisen.Filters.Buffering;

namespace Shamisen.Core.Tests.CoreFx
{
    public static class TestHelper
    {
        public const string ResourcesPath = "Shamisen.Core.Tests.CoreFx.Resources";

        public static void DoesNotTakeSoLong(Action action) => DoesNotTakeSoLong(action, TimeSpan.FromSeconds(1));

        public static void DoesNotTakeSoLong(Action action, TimeSpan timeout)
        {
            CancellationTokenSource tsFail = new();
            var t = Task.Run(action, tsFail.Token);
            t.ConfigureAwait(false);
            tsFail.CancelAfter(timeout);
            Assert.DoesNotThrow(() => t.Wait(), $"Timeout({timeout}) exceeded.");
        }

        public static DataCache<byte> GetDataCacheFromResource(string name)
        {
            var lib = Assembly.GetExecutingAssembly();
            var ms = new DataCache<byte>();
            using (var stream = lib.GetManifestResourceStream($"{ResourcesPath}.{name}"))
            {
                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);
                    _ = mem.Seek(0, SeekOrigin.Begin);
                    ms.Write(mem.GetBuffer().AsSpan().Slice(0, (int)mem.Length));
                }
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public static MemoryStream GetDataStreamFromResource(string name)
        {
            var lib = Assembly.GetExecutingAssembly();
            var ms = new MemoryStream();
            using (var stream = lib.GetManifestResourceStream($"{ResourcesPath}.{name}"))
            {
                stream.CopyTo(ms);
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
        /// <summary>
        /// Strip illegal chars and reserved words from a candidate filename (should not include the directory path)
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
        /// </remarks>
        public static string CoerceValidFileName(string filename)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"[{0}]+", invalidChars);

            string[] reservedWords = new[]
            {
                "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            string sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
            foreach (string reservedWord in reservedWords)
            {
                string reservedWordPattern = string.Format("^{0}\\.", reservedWord);
                sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase);
            }

            return sanitisedNamePart;
        }
        public static void DumpSamples(AudioCache<float, SampleFormat> cache, string fileName)
        {
            var path = new FileInfo($"./dumps/{CoerceValidFileName(fileName)}.wav");
            Console.WriteLine(path.FullName);
            if (!Directory.Exists("./dumps")) _ = Directory.CreateDirectory("./dumps");
            using var dc = new AudioCache<float, SampleFormat>(cache.Format);
            var trunc = new LengthTruncationSource<float, SampleFormat>(cache, cache.TotalLength ?? 0);
            using (var ssink = new StreamDataSink(path.OpenWrite(), true, true))
            {
                Assert.DoesNotThrow(() =>
                SimpleWaveEncoder.Instance.Encode(new SampleToFloat32Converter(trunc), ssink));
            }
        }

        public static void DumpSampleSource(int frameLen, int framesToWrite, ISampleSource source, string fileName)
        {
            var path = new FileInfo($"./dumps/{CoerceValidFileName(fileName)}.wav");
            Console.WriteLine(path.FullName);
            if (!Directory.Exists("./dumps")) _ = Directory.CreateDirectory("./dumps");
            using var dc = new AudioCache<float, SampleFormat>(source.Format);
            float[] buffer = new float[(ulong)frameLen * (ulong)dc.Format.Channels];
            for (int i = 0; i < framesToWrite; i++)
            {
                var q = source.Read(buffer);
                dc.Write(buffer.AsSpan(0, q.Length));
            }
            var trunc = new LengthTruncationSource<float, SampleFormat>(dc, (ulong)framesToWrite * (ulong)frameLen);
            using (var ssink = new StreamDataSink(path.OpenWrite(), true, true))
            {
                Assert.DoesNotThrow(() =>
                SimpleWaveEncoder.Instance.Encode(new SampleToFloat32Converter(trunc), ssink));
            }
        }
    }
}
