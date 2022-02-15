using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Data;
using Shamisen.Filters;
using Shamisen.Filters.Buffering;
using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx.TestUtils
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
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);

            var reservedWords = new[]
            {
                "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
            foreach (var reservedWord in reservedWords)
            {
                var reservedWordPattern = string.Format("^{0}\\.", reservedWord);
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
            var buffer = new float[(ulong)frameLen * (ulong)dc.Format.Channels];
            for (var i = 0; i < framesToWrite; i++)
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
        public static void GenerateRandomComplexNumbers(Span<ComplexF> src)
        {
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(src));
            RangedPcm32ToSampleConverter.ProcessEMoreThan24Standard(MemoryMarshal.Cast<ComplexF, float>(src), MemoryMarshal.Cast<ComplexF, int>(src), 30);
        }

        public static void GenerateRandomComplexNumbers(Span<Complex> src)
        {
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(src));
            var a = new Vector<double>(0xBFFF_FFFF_FFFF_FFFFul).AsDouble();
            ref var rdi = ref Unsafe.As<Complex, double>(ref MemoryMarshal.GetReference(src));
            nint i, length = src.Length * 2;
            var olen = length - 4 * Vector<double>.Count + 1;
            for (i = 0; i < olen; i += 4 * Vector<double>.Count)
            {
                var ymm0 = Vector.BitwiseAnd(Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rdi, i + 0 * Vector<double>.Count)), a);
                var ymm1 = Vector.BitwiseAnd(Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rdi, i + 1 * Vector<double>.Count)), a);
                var ymm2 = Vector.BitwiseAnd(Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rdi, i + 2 * Vector<double>.Count)), a);
                var ymm3 = Vector.BitwiseAnd(Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rdi, i + 3 * Vector<double>.Count)), a);
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rdi, i + 0 * Vector<double>.Count)) = ymm0;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rdi, i + 1 * Vector<double>.Count)) = ymm1;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rdi, i + 2 * Vector<double>.Count)) = ymm2;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rdi, i + 3 * Vector<double>.Count)) = ymm3;
            }
            for (; i < length; i++)
            {
                var u = BitConverter.DoubleToUInt64Bits(Unsafe.Add(ref rdi, i));
                u &= 0xBFFF_FFFF_FFFF_FFFFul;
                Unsafe.Add(ref rdi, i) = u;
            }
        }
        public static void GenerateRandomRealNumbers(Span<float> src)
        {
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(src));
            RangedPcm32ToSampleConverter.ProcessEMoreThan24Standard(src, MemoryMarshal.Cast<float, int>(src), 30);
        }

        public static void GenerateRandomNumbers(Span<float> src) => RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(src));

        public static void AssertArrays(ReadOnlySpan<ComplexF> exp, ReadOnlySpan<ComplexF> dst)
        {
            NeumaierAccumulator sumdiff = default;
            Assert.AreEqual(exp.Length, dst.Length);
            for (var i = 0; i < dst.Length; i++)
            {
                Complex opt = dst[i];
                Complex sim = exp[i];
                var diff = opt - sim;
                sumdiff += Math.Abs(diff.Real);
                sumdiff += Math.Abs(diff.Imaginary);
                if (diff != 0)
                {
                    Console.WriteLine($"{i:X08}: {sim}, {opt}, {diff}");
                }
            }
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            var avgDiff = sumdiff.Sum / dst.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.AreEqual(0.0, sumdiff.Sum);
        }
        public static void AssertArrays(ReadOnlySpan<Complex> exp, ReadOnlySpan<Complex> dst)
        {
            NeumaierAccumulator sumdiff = default;
            Assert.AreEqual(exp.Length, dst.Length);
            for (var i = 0; i < dst.Length; i++)
            {
                var opt = dst[i];
                var sim = exp[i];
                var diff = opt - sim;
                sumdiff += Math.Abs(diff.Real);
                sumdiff += Math.Abs(diff.Imaginary);
                if (diff != 0)
                {
                    Console.WriteLine($"{i:X08}: {sim}, {opt}, {diff}");
                }
            }
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            var avgDiff = sumdiff.Sum / dst.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.AreEqual(0.0, sumdiff.Sum);
        }
        public static void AssertArrays(ReadOnlySpan<float> exp, ReadOnlySpan<float> dst, double delta = 0.0)
        {
            delta = Math.Abs(delta);
            NeumaierAccumulator sumdiff = default;
            Assert.AreEqual(exp.Length, dst.Length);
            var maxdiff = 0.0;
            for (var i = 0; i < dst.Length; i++)
            {
                double opt = dst[i];
                double sim = exp[i];
                var diff = opt - sim;
                var adiff = Math.Abs(diff);
                sumdiff += adiff;
                if (adiff >= delta)
                {
                    Console.WriteLine($"{i:X08}: {sim}, {opt}, {diff}");
                }
                maxdiff = Math.Max(maxdiff, adiff);
            }
            var avgDiff = sumdiff.Sum / dst.Length;
            Console.WriteLine($"Total difference: {sumdiff.Sum}");
            Console.WriteLine($"Average difference: {avgDiff}");
            Console.WriteLine($"Maximum difference: {maxdiff}");
            Assert.AreEqual(0.0, maxdiff, delta);
        }

        public static void AssertArraysRelative(ReadOnlySpan<float> exp, ReadOnlySpan<float> dst, double delta = 0.0)
        {
            Assert.AreEqual(exp.Length, dst.Length);
            double maxdiff = 0.0;
            for (var i = 0; i < dst.Length; i++)
            {
                double opt = dst[i];
                double sim = exp[i];
                var diff = (opt - sim) / sim;
                if (double.IsNaN(diff)) diff = 0.0;
                var adiff = Math.Abs(diff);
                if (diff != 0)
                {
                    Console.WriteLine($"{i:X08}: {sim}, {opt}, {diff}");
                }
                maxdiff = Math.Max(maxdiff, adiff);
            }
            Console.WriteLine($"Maximum Relative Difference: {maxdiff}");
            Assert.AreEqual(0, maxdiff, delta);
        }
    }
}
