using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Analysis;

namespace Shamisen.Core.Tests.CoreFx.Analysis
{
    [TestFixture]
    public class CooleyTukeyFftTest
    {
        private const double MachineEpsilon = 1.0 / (1ul << 52);
        #region FFTTest

        [TestCase]
        public void FFTTestDouble()
        {
            var array = new Complex[2048];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = Math.Sin(2.0 * Math.PI * i / array.Length);
            }
            var copy = new Complex[array.Length];
            Array.Copy(array, copy, array.Length);
            var span = new Span<Complex>(array);

            CooleyTukeyFft.FFT(span);
            Span<Complex> transformed = stackalloc Complex[array.Length];
            span.CopyTo(transformed);
            CooleyTukeyFft.FFT(span, FftMode.Backward);

            try
            {
                for (var i = 0; i < array.Length; i++)
                {
                    Assert.AreEqual(copy[i].Real, array[i].Real, -1.0 / int.MinValue);
                    Assert.AreEqual(copy[i].Imaginary, array[i].Imaginary, -1.0 / int.MinValue);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("IsOK,Expected,Actual,Difference");
                for (var i = 0; i < array.Length; i++)
                {
                    var diff = copy[i] - array[i];
                    var maxDiff = Math.Max(Math.Abs(diff.Real), Math.Abs(diff.Imaginary));
                    Console.WriteLine($"{(maxDiff < -1.0 / int.MinValue ? "○" : "✕")}, {copy[i]}, {array[i]}, {diff}");
                }
                throw;
            }
        }

        [TestCase(8)]
        [TestCase(16)]
        [TestCase(32)]
        [TestCase(64)]
        [TestCase(2048)]
        public void FFTTestFloat(int size)
        {
            size = (int)MathI.ExtractHighestSetBit((uint)size);
            var array = new ComplexF[size];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = (float)Math.Sin(2.0 * Math.PI * i / array.Length);
            }
            var copy = new ComplexF[array.Length];
            Array.Copy(array, copy, array.Length);
            var span = new Span<ComplexF>(array);

            CooleyTukeyFft.FFT(span);
            Span<ComplexF> transformed = stackalloc ComplexF[array.Length];
            span.CopyTo(transformed);
            CooleyTukeyFft.FFT(span, FftMode.Backward);

            try
            {
                for (var i = 0; i < array.Length; i++)
                {
                    Assert.AreEqual(copy[i].Real, array[i].Real, -1.0 / short.MinValue);
                    Assert.AreEqual(copy[i].Imaginary, array[i].Imaginary, -1.0 / short.MinValue);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Expected,Actual,Difference");
                for (var i = 0; i < array.Length; i++)
                {
                    Console.WriteLine($"{copy[i]}, {array[i]}, {copy[i] - array[i]}");
                }
                throw;
            }
        }
        #endregion

        #region IntrinsicsConsistency
        #region Perform2

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        public void Perform2Avx2PerformsCorrectly(int order)
        {
            if (!Avx2.IsSupported)
                Assert.Warn($"{nameof(Avx2)} is not supported!");
            PrepareArrays(order, out var fallback, out var optimized);
            CooleyTukeyFft.X86.Perform2Avx2(optimized);
            CooleyTukeyFft.Fallback.Perform2Fallback(fallback);
            TestHelper.AssertArrays(fallback, optimized);
        }
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        public void Perform2SsePerformsCorrectly(int order)
        {
            if (!Sse.IsSupported)
                Assert.Warn($"{nameof(Sse)} is not supported!");
            PrepareArrays(order, out var fallback, out var optimized);
            CooleyTukeyFft.X86.Perform2Sse(optimized);
            CooleyTukeyFft.Fallback.Perform2Fallback(fallback);
            TestHelper.AssertArrays(fallback, optimized);
        }

        [TestCase(1, FftMode.Forward)]
        [TestCase(2, FftMode.Forward)]
        [TestCase(4, FftMode.Forward)]
        [TestCase(8, FftMode.Forward)]
        [TestCase(1, FftMode.Backward)]
        [TestCase(2, FftMode.Backward)]
        [TestCase(4, FftMode.Backward)]
        [TestCase(8, FftMode.Backward)]
        public void Perform4Avx2PerformsCorrectly(int order, FftMode mode)
        {
            if (!Avx2.IsSupported)
                Assert.Warn($"{nameof(Avx2)} is not supported!");
            PrepareArrays(order, out var fallback, out var optimized);
            CooleyTukeyFft.X86.Perform4Avx2(optimized, mode);
            CooleyTukeyFft.Fallback.Perform4Fallback(fallback, mode);
            TestHelper.AssertArrays(fallback, optimized);
        }

        [TestCase(1, FftMode.Forward)]
        [TestCase(2, FftMode.Forward)]
        [TestCase(4, FftMode.Forward)]
        [TestCase(8, FftMode.Forward)]
        [TestCase(1, FftMode.Backward)]
        [TestCase(2, FftMode.Backward)]
        [TestCase(4, FftMode.Backward)]
        [TestCase(8, FftMode.Backward)]
        public void Perform4SsePerformsCorrectly(int order, FftMode mode)
        {
            if (!Sse.IsSupported)
                Assert.Warn($"{nameof(Sse)} is not supported!");
            PrepareArrays(order, out var fallback, out var optimized);
            CooleyTukeyFft.X86.Perform4Sse(optimized, mode);
            CooleyTukeyFft.Fallback.Perform4Fallback(fallback, mode);
            TestHelper.AssertArrays(fallback, optimized);
        }

        private static void PrepareArrays(int order, out ComplexF[] fallback, out ComplexF[] optimized)
        {
            var src = new ComplexF[1 << order];
            fallback = new ComplexF[src.Length];
            optimized = new ComplexF[src.Length];
            TestHelper.GenerateRandomComplexNumbers(src);
            var span = src.AsSpan();
            span.CopyTo(fallback);
            src.AsSpan().CopyTo(optimized);
        }

        #endregion
        #region Perform4

        #endregion
        #endregion

        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(11)]
        public void FFTCacheDumpDouble(int index = 5)
        {
            Span<Complex> span = stackalloc Complex[1 << index - 1];
            CooleyTukeyFft.CalculateCache(FftMode.Forward, index, span);
            Span<ComplexF> spanF = stackalloc ComplexF[span.Length];
            CooleyTukeyFft.CalculateCache(FftMode.Forward, index, spanF);
            try
            {
                for (var i = 0; i < span.Length; i++)
                {
                    Assert.AreEqual(spanF[i].Real, span[i].Real, -1.0 / short.MinValue);
                    Assert.AreEqual(spanF[i].Imaginary, span[i].Imaginary, -1.0 / short.MinValue);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Expected,Actual");
                for (var i = 0; i < span.Length; i++)
                {
                    Console.WriteLine($"{spanF[i]}, {span[i]}");
                }
                throw;
            }

            Assert.Pass();
        }

        [TestCase(3, FftMode.Forward)]
        [TestCase(5, FftMode.Forward)]
        [TestCase(3, FftMode.Backward)]
        [TestCase(5, FftMode.Backward)]
        public void FFTCacheDumpFloat(int index, FftMode mode)
        {
            Span<ComplexF> span = stackalloc ComplexF[1 << index - 1];
            CooleyTukeyFft.CalculateCache(mode, index, span);
            for (var i = 0; i < span.Length; i++)
            {
                Console.WriteLine($"{span[i]}");
            }
            Assert.Pass();
        }
    }
}
