using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class SpanExtensionsTest
    {
        [Test]
        public void QuickFillFillsCorrectly()
        {
            Span<decimal> span = new decimal[127];
            const decimal Value = -1m;
            span.QuickFill(Value);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastFill doesn't fill correctly!");
            }
            Assert.Pass();
        }

        [TestCase(2459)]    //Prime number
        [TestCase(2048)]    //2^11
        public void QuickFillFasterThanNormalFill(int length)
        {
            var sw = new Stopwatch();
            Span<float> span = new float[length];
            long cntFast = 0;
            long cntStandard = 0;
            Thread.Sleep(50);
            sw.Start();
            do
            {
                span.QuickFill(cntFast);
                cntFast += span.Length;
            } while (sw.ElapsedMilliseconds < 2000);
            sw.Stop();
            Console.WriteLine($"QuickFill: {cntFast / sw.Elapsed.TotalSeconds} sample/s({cntFast / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            sw.Reset();
            Thread.Sleep(50);
            sw.Start();
            do
            {
                span.Fill(cntStandard);
                cntStandard += span.Length;
            } while (sw.ElapsedMilliseconds < 2000);
            sw.Stop();
            Console.WriteLine($"Fill: {cntStandard / sw.Elapsed.TotalSeconds} sample/s({cntStandard / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            Console.WriteLine($"{nameof(SpanExtensions.QuickFill)} seems to be {(double)cntFast / cntStandard} times faster than {nameof(Span<float>.Fill)}.");
            Assert.Greater(cntFast, cntStandard);
        }

        [Test]
        public void FastFillFillsCorrectly()
        {
            Span<float> span = new float[32];
            const int Value = 1;
            span.FastFill(Value);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastFill doesn't fill correctly!");
            }
            Assert.Pass();
        }

        [TestCase(2459)]    //Prime number
        [TestCase(2048)]    //2^11
        public void FastFillFasterThanNormalFill(int length)
        {
            var sw = new Stopwatch();
            Span<float> span = new float[length];
            long cntFast = 0;
            long cntStandard = 0;
            Thread.Sleep(50);
            sw.Start();
            do
            {
                span.FastFill(cntFast);
                cntFast += span.Length;
            } while (sw.ElapsedMilliseconds < 2000);
            sw.Stop();
            Console.WriteLine($"FastFill: {cntFast / sw.Elapsed.TotalSeconds} sample/s({cntFast / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            sw.Reset();
            Thread.Sleep(50);
            sw.Start();
            do
            {
                span.Fill(cntStandard);
                cntStandard += span.Length;
            } while (sw.ElapsedMilliseconds < 2000);
            sw.Stop();
            Console.WriteLine($"Fill: {cntStandard / sw.Elapsed.TotalSeconds} sample/s({cntStandard / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            Console.WriteLine($"{nameof(SpanExtensions.FastFill)} seems to be {(double)cntFast / cntStandard} times faster than {nameof(Span<float>.Fill)}.");
            Assert.Greater(cntFast, cntStandard);
        }

        [Test]
        public void FastAddAddsCorrectly()
        {
            Span<float> source = new float[32];
            Span<float> destination = new float[48];
            const int Value = 1;
            source.FastFill(Value);
            destination.FastFill(-1);

            SpanExtensions.FastAdd(source, destination);
            for (int i = 0; i < source.Length; i++)
            {
                if (destination[i] != 0) Assert.Fail("The FastAdd doesn't add correctly!");
            }
            Assert.Pass();
        }

        [TestCase(2459)]    //Prime number
        [TestCase(2048)]    //2^11
        public void FastAddFasterThanUnsafeAdd(int length)
        {
            var sw = new Stopwatch();
            Span<float> source = new float[length];
            Span<float> destination = new float[length + 1024];
            source.FastFill(1);
            destination.FastFill(0);
            long cntFast = 0;
            long cntStandard = 0;
            Thread.Sleep(50);
            sw.Start();
            do
            {
                SpanExtensions.FastAdd(source, destination);
                cntFast += source.Length;
            } while (sw.ElapsedMilliseconds < 2000);
            sw.Stop();
            Console.WriteLine($"FastAdd: {cntFast / sw.Elapsed.TotalSeconds} sample/s({cntFast / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            sw.Reset();
            Thread.Sleep(50);
            sw.Start();
            do
            {
                unsafe
                {
                    fixed (float* srcFx = source)
                    fixed (float* dstFx = destination)
                    {
                        var src = srcFx;
                        var dst = dstFx;
                        do
                        {
                            *dst++ += *src++;
                        } while (src < srcFx + source.Length);
                    }
                }
                cntStandard += source.Length;
            } while (sw.ElapsedMilliseconds < 2000);
            sw.Stop();
            Console.WriteLine($"UnsafeAdd: {cntStandard / sw.Elapsed.TotalSeconds} sample/s({cntStandard / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            Console.WriteLine($"{nameof(SpanExtensions.FastAdd)} seems to be {(double)cntFast / cntStandard} times faster than unsafe loop.");
            Assert.Greater(cntFast, cntStandard);
        }

        [Test]
        public void FastScalarMultiplyScalesCorrectly()
        {
            Span<float> span = new float[32];
            span.FastFill(1);
            const float Value = MathF.PI;
            span.FastScalarMultiply(Value);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastScalarMultiply doesn't scale correctly!");
            }
            Assert.Pass();
        }

        [TestCase(2459)]    //Prime number
        [TestCase(2048)]    //2^11
        public void FastScalarMultiplyFasterThanUnsafe(int length)
        {
            var sw = new Stopwatch();
            Span<float> span = new float[length];
            span.FastFill(1);
            long cntFast = 0;
            long cntStandard = 0;
            Thread.Sleep(50);
            sw.Start();
            do
            {
                span.FastScalarMultiply(cntFast);
                cntFast += span.Length;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"FastScalarMultiply: {cntFast / sw.Elapsed.TotalSeconds} sample/s({cntFast / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            sw.Reset();
            Thread.Sleep(50);
            sw.Start();
            do
            {
                unsafe
                {
                    fixed (float* srcFx = span)
                    {
                        var src = srcFx;
                        do
                        {
                            *src *= cntStandard;
                        } while (++src < srcFx + span.Length);
                    }
                }
                cntStandard += span.Length;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"UnsafeScalarMultiply: {cntStandard / sw.Elapsed.TotalSeconds} sample/s({cntStandard / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            Console.WriteLine($"{nameof(SpanExtensions.FastScalarMultiply)} seems to be {(double)cntFast / cntStandard} times faster than unsafe loop.");
            Assert.Greater(cntFast, cntStandard);
        }

        [Test]
        public void FastMixMixesCorrectly()
        {
            Span<float> source = stackalloc float[32];
            Span<float> destination = stackalloc float[48];
            const int Value = 1;
            source.FastFill(Value);
            destination.FastFill(-1);

            SpanExtensions.FastMix(source, destination, 2);
            for (int i = 0; i < source.Length; i++)
            {
                if (destination[i] != 1) Assert.Fail("The FastMix doesn't mix correctly!");
            }
            Assert.Pass();
        }

        [TestCase(2459)]    //Prime number
        [TestCase(2048)]    //2^11
        public void FastMixFasterThanUnsafeMix(int length)
        {
            var sw = new Stopwatch();
            Span<float> source = stackalloc float[length];
            Span<float> destination = stackalloc float[length + 1024];
            source.FastFill(1);
            destination.FastFill(0);
            long cntFast = 0;
            long cntStandard = 0;
            Thread.Sleep(50);
            sw.Start();
            do
            {
                SpanExtensions.FastMix(source, destination, 0.5f);
                cntFast += source.Length;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"FastMix: {cntFast / sw.Elapsed.TotalSeconds} sample/s({cntFast / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            sw.Reset();
            Thread.Sleep(50);
            sw.Start();
            do
            {
                var u = 0.5f;
                unsafe
                {
                    fixed (float* srcFx = source)
                    fixed (float* dstFx = destination)
                    {
                        var src = srcFx;
                        var dst = dstFx;
                        do
                        {
                            *dst++ += (*src++) * u;
                        } while (src < srcFx + source.Length);
                    }
                }
                cntStandard += source.Length;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"UnsafeMix: {cntStandard / sw.Elapsed.TotalSeconds} sample/s({cntStandard / sw.Elapsed.TotalSeconds / 384000.0} times faster than real time in 192kHz Stereo)");
            Console.WriteLine($"{nameof(SpanExtensions.FastMix)} seems to be {(double)cntFast / cntStandard} times faster than unsafe loop.");
            Assert.Greater(cntFast, cntStandard);
        }
    }
}
