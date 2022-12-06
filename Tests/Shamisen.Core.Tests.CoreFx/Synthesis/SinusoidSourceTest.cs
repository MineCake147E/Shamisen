using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
//using CSCodec.Filters.Transformation;
using NUnit.Framework;

using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Data;
using Shamisen.Filters;
using Shamisen.Filters.Buffering;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx.Synthesis
{
    [TestFixture]
    public class SinusoidSourceTest
    {
        private const double Freq = 523.2511306011972693556999870466094027289077206840796617283;

        [TestCase(1, 24000, 1024 + 7, 64)]
        [TestCase(2, 24000, 1024 + 7, 64)]
        [TestCase(1, 192000, 1024 + 7, 187)]
        public void SinusoidManyFrameDump(int channels, int sourceSampleRate, int frameLen = 1024, int framesToWrite = 64)
        {
            var src = new SinusoidSource(new SampleFormat(channels, sourceSampleRate)) { Frequency = Freq };
            var path = new System.IO.FileInfo($"./dumps/SinusoidDump_{channels}ch_{sourceSampleRate}_{frameLen}fpb_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}.wav");
            using var dc = new AudioCache<float, SampleFormat>(src.Format);
            Console.WriteLine(path.FullName);
            var buffer = new float[(ulong)frameLen * (ulong)channels];
            for (var i = 0; i < framesToWrite; i++)
            {
                var rr = src.Read(buffer);
                if (rr.HasData)
                {
                    dc.Write(buffer.AsSpan(0, rr.Length));
                }
                else
                {
                    break;
                }
            }
            var trunc = new LengthTruncationSource<float, SampleFormat>(dc, (ulong)framesToWrite * (ulong)frameLen);
            using (var ssink = new StreamDataSink(path.OpenWrite(), true, true))
            {
                Assert.DoesNotThrow(() =>
                SimpleWaveEncoder.Instance.Encode(new SampleToFloat32Converter(trunc), ssink));
            }

            Assert.Pass();
            src.Dispose();
        }

        [TestCase(8192)]
        [TestCase(8191)]
        public void GenerateMonauralBlockAvx2FmaMM256AccuratelyGenerates(int sampleRate, double frequency = 1.0)
        {
            if (!Avx2.IsSupported || !Fma.IsSupported)
            {
                Assert.Warn($"Either {nameof(Avx2)} or {nameof(Fma)} is not supported!");
                return;
            }
            PrepareArraysAndAngularVelocity(sampleRate, frequency, out var dst, out var exp, out var omega);
            var t0 = SinusoidSource.GenerateMonauralBlockAvx2FmaMM256(dst, omega, Fixed64.Zero);
            var t1 = GenerateMonauralBlockIeee754(exp, omega, Fixed64.Zero);
            TestHelper.AssertArrays(exp, dst, 1.0f / 32768);
            Assert.AreEqual(t1, t0);
        }

        [TestCase(8192)]
        [TestCase(8191)]
        public void GenerateMonauralBlockAvx2MM256AccuratelyGenerates(int sampleRate, double frequency = 1.0)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn($"{nameof(Avx2)} is not supported!");
                return;
            }
            PrepareArraysAndAngularVelocity(sampleRate, frequency, out var dst, out var exp, out var omega);
            var t0 = SinusoidSource.GenerateMonauralBlockAvx2MM256(dst, omega, Fixed64.Zero);
            var t1 = SinusoidSource.GenerateMonauralBlockStandard(exp, omega, Fixed64.Zero);
            TestHelper.AssertArrays(exp, dst, 1.0f / 32768);
            Assert.AreEqual(t1, t0);
        }

        [TestCase(8192)]
        [TestCase(8191)]
        public void GenerateMonauralBlockSse41AccuratelyGenerates(int sampleRate, double frequency = 1.0)
        {
            if (!Sse41.IsSupported)
            {
                Assert.Warn($"{nameof(Sse41)} is not supported!");
                return;
            }
            PrepareArraysAndAngularVelocity(sampleRate, frequency, out var dst, out var exp, out var omega);
            var t0 = SinusoidSource.GenerateMonauralBlockSse41(dst, omega, Fixed64.Zero);
            var t1 = SinusoidSource.GenerateMonauralBlockStandard(exp, omega, Fixed64.Zero);
            TestHelper.AssertArrays(exp, dst, 1.0f / 32768);
            Assert.AreEqual(t1, t0);
        }

        [TestCase(8192)]
        [TestCase(8191)]
        public void SinCalculatesAccurately(int sampleRate, double frequency = 1.0)
        {
            PrepareArraysAndAngularVelocity(sampleRate, frequency, out var dst, out var exp, out var omega);
            var t0 = SinusoidSource.GenerateMonauralBlockStandard(dst, omega, Fixed64.Zero);
            var t1 = GenerateMonauralBlockIeee754(exp, omega, Fixed64.Zero);
            TestHelper.AssertArrays(exp, dst, 1.0f / 32768);
            Assert.AreEqual(t1, t0);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static Fixed64 GenerateMonauralBlockIeee754(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (float)Math.Sin(Math.PI * SinusoidSource.ConvertFixed64ToSingle(theta));
                theta += omega;
            }
            return theta;
        }
        private static void PrepareArraysAndAngularVelocity(int sampleRate, double frequency, out float[] dst, out float[] exp, out Fixed64 omega)
        {
            dst = new float[sampleRate];
            exp = new float[sampleRate];
            TestHelper.GenerateRandomRealNumbers(dst);
            TestHelper.GenerateRandomRealNumbers(exp);
            omega = SinusoidSource.CalculateAngularVelocity(frequency, sampleRate);
        }
    }
}
