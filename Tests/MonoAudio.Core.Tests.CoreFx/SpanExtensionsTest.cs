using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MonoAudio.Core.Tests.CoreFx
{
    [TestFixture]
    public class SpanExtensionsTest
    {
        [Test]
        public void FastFillFillsCorrectly()
        {
            Span<float> span = stackalloc float[32];
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
            Span<float> span = stackalloc float[length];
            long cntFast = 0;
            long cntStandard = 0;
            Thread.Sleep(50);
            sw.Start();
            do
            {
                span.FastFill(cntFast);
                cntFast++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"FastFill: {(double)cntFast / length} operations/s({cntFast / 384000.0} times faster than real time in 192kHz Stereo)");
            sw.Reset();
            Thread.Sleep(50);
            sw.Start();
            do
            {
                span.Fill(cntStandard);
                cntStandard++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"Fill: {(double)cntStandard / length} operations/s({cntStandard / 384000.0} times faster than real time in 192kHz Stereo)");
            Console.WriteLine($"{nameof(SpanExtensions.FastFill)} seems to be {(double)cntFast / cntStandard} times faster than {nameof(Span<float>.Fill)}.");
            Assert.Greater(cntFast, cntStandard);
        }

        [Test]
        public void FastAddAddsCorrectly()
        {
            Span<float> source = stackalloc float[32];
            Span<float> destination = stackalloc float[48];
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
                SpanExtensions.FastAdd(source, destination);
                cntFast++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"FastAdd: {(double)cntFast / length} operations/s({cntFast / 384000.0} times faster than real time in 192kHz Stereo)");
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
                cntStandard++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"UnsafeAdd: {(double)cntStandard / length} operations/s({cntStandard / 384000.0} times faster than real time in 192kHz Stereo)");
            Console.WriteLine($"{nameof(SpanExtensions.FastAdd)} seems to be {(double)cntFast / cntStandard} times faster than unsafe loop.");
            Assert.Greater(cntFast, cntStandard);
        }

        [Test]
        public void FastScalarMultiplyScalesCorrectly()
        {
            Span<float> span = stackalloc float[32];
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
            Span<float> span = stackalloc float[length];
            span.FastFill(1);
            long cntFast = 0;
            long cntStandard = 0;
            Thread.Sleep(50);
            sw.Start();
            do
            {
                span.FastScalarMultiply(cntFast);
                cntFast++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"FastScalarMultiply: {(double)cntFast / length} operations/s({cntFast / 384000.0} times faster than real time in 192kHz Stereo)");
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
                cntStandard++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"UnsafeScalarMultiply: {(double)cntStandard / length} operations/s({cntStandard / 384000.0} times faster than real time in 192kHz Stereo)");
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
                SpanExtensions.FastMix(source, destination, 1.0f / cntFast);
                cntFast++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"FastMix: {(double)cntFast / length} operations/s({cntFast / 384000.0} times faster than real time in 192kHz Stereo)");
            sw.Reset();
            Thread.Sleep(50);
            sw.Start();
            do
            {
                var u = 1.0f / cntStandard;
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
                cntStandard++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine($"UnsafeMix: {(double)cntStandard / length} operations/s({cntStandard / 384000.0} times faster than real time in 192kHz Stereo)");
            Console.WriteLine($"{nameof(SpanExtensions.FastAdd)} seems to be {(double)cntFast / cntStandard} times faster than unsafe loop.");
            Assert.Greater(cntFast, cntStandard);
        }
    }
}
