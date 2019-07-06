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
        [TestCase(4783)]    //Prime number
        [TestCase(2048)]    //2^11
        [TestCase(4096)]    //2^12
        [TestCase(8192)]    //2^13
        public void FastFillTest(int length)
        {
            var sw = new Stopwatch();
            Span<float> source = stackalloc float[length];
            long cntFast = 0;
            long cntStandard = 0;
            Thread.Sleep(50);
            sw.Start();
            do
            {
                source.FastFill(cntFast);
                cntFast++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine(cntFast);
            sw.Reset();
            Thread.Sleep(50);
            sw.Start();
            do
            {
                source.Fill(cntStandard);
                cntStandard++;
            } while (sw.ElapsedMilliseconds < length);
            sw.Stop();
            Console.WriteLine(cntStandard);
            Console.WriteLine($"{nameof(SpanExtensions.FastFill)} seems to be {(double)cntFast / cntStandard} times faster than {nameof(Span<float>.Fill)}");
            Assert.Greater(cntFast, cntStandard);
        }

        [TestCase(4783)]    //Prime number
        [TestCase(2048)]    //2^11
        [TestCase(4096)]    //2^12
        public void FastAddTest(int length)
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
            Console.WriteLine(cntFast);
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
            Console.WriteLine(cntStandard);
            Console.WriteLine($"{nameof(SpanExtensions.FastFill)} seems to be {(double)cntFast / cntStandard} times faster than unsafe loop");
            Assert.Greater(cntFast, cntStandard);
        }
    }
}
