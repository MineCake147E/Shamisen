using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Numerics;
using Shamisen.Utils;
using Shamisen.Utils.Numerics;

namespace Shamisen.Core.Tests.CoreFx.Numerics
{
    [TestFixture]
    public class ComplexUtilsTest
    {
        #region MultiplyAll

        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void MultiplyAllFallbackMultipliesCorrectly(int length)
        {
            var value = new ComplexF(0.3f, 0.7f);
            MultiplyAllTestUtils.PrepareArrays(length, value, out var src, out var exp, out var dst);
            ComplexUtils.Fallback.MultiplyAllFallback(dst, src, value);
            TestHelper.AssertArrays(exp, dst);
        }
        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void MultiplyAllAvxSingleMultipliesCorrectly(int length)
        {
            if (!Avx.IsSupported)
            {
                Assert.Warn($"{nameof(Avx)} is not supported!");
                return;
            }
            var value = new ComplexF(0.3f, 0.7f);
            MultiplyAllTestUtils.PrepareArrays(length, value, out var src, out var exp, out var dst);
            ComplexUtils.X86.MultiplyAllAvx(dst, src, value);
            TestHelper.AssertArrays(exp, dst);
        }
        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void MultiplyAllAvxDoubleMultipliesCorrectly(int length)
        {
            if (!Avx.IsSupported)
            {
                Assert.Warn($"{nameof(Avx)} is not supported!");
                return;
            }
            var value = new Complex(0.3f, 0.7f);
            MultiplyAllTestUtils.PrepareArrays(length, value, out var src, out var exp, out var dst);
            ComplexUtils.X86.MultiplyAllAvx(dst, src, value);
            TestHelper.AssertArrays(exp, dst);
        }
        private static class MultiplyAllTestUtils
        {
            internal static void PrepareArrays(int length, ComplexF value, out ComplexF[] src, out ComplexF[] exp, out ComplexF[] dst)
            {
                src = new ComplexF[length];
                exp = new ComplexF[length];
                dst = new ComplexF[length];
                TestHelper.GenerateRandomComplexNumbers(src);
                MultiplyAllSimple(exp, src, value);
            }
            internal static void PrepareArrays(int length, Complex value, out Complex[] src, out Complex[] exp, out Complex[] dst)
            {
                src = new Complex[length];
                exp = new Complex[length];
                dst = new Complex[length];
                TestHelper.GenerateRandomComplexNumbers(src);
                MultiplyAllSimple(exp, src, value);
            }

            internal static void MultiplyAllSimple(Span<ComplexF> destination, ReadOnlySpan<ComplexF> source, ComplexF value)
            {
                ref var x9 = ref MemoryMarshal.GetReference(source);
                ref var x10 = ref MemoryMarshal.GetReference(destination);
                nint i, length = MathI.Min(destination.Length, source.Length);
                for (i = 0; i < length; i++)
                {
                    Unsafe.Add(ref x10, i) = Unsafe.Add(ref x9, i) * value;
                }
            }

            internal static void MultiplyAllSimple(Span<Complex> destination, ReadOnlySpan<Complex> source, Complex value)
            {
                ref var x9 = ref MemoryMarshal.GetReference(source);
                ref var x10 = ref MemoryMarshal.GetReference(destination);
                nint i, length = MathI.Min(destination.Length, source.Length);
                for (i = 0; i < length; i++)
                {
                    Unsafe.Add(ref x10, i) = Unsafe.Add(ref x9, i) * value;
                }
            }
        }
        #endregion
        #region ConvertRealToComplex
        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ConvertRealToComplexFallbackSingleConvertsCorrectly(int length)
        {
            ConvertRealToComplexTestUtils.PrepareArrays(length, out var src, out var exp, out var dst);
            ComplexUtils.Fallback.ConvertRealToComplexFallback(dst, src);
            TestHelper.AssertArrays(exp, dst);
        }
        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ConvertRealToComplexAvx2SingleConvertsCorrectly(int length)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn($"{nameof(Avx2)} is not supported!");
                return;
            }
            ConvertRealToComplexTestUtils.PrepareArrays(length, out var src, out var exp, out var dst);
            ComplexUtils.X86.ConvertRealToComplexAvx2(dst, src);
            TestHelper.AssertArrays(exp, dst);
        }

        private static class ConvertRealToComplexTestUtils
        {
            internal static void PrepareArrays(int length, out float[] src, out ComplexF[] exp, out ComplexF[] dst)
            {
                src = new float[length];
                exp = new ComplexF[length];
                dst = new ComplexF[length];
                TestHelper.GenerateRandomRealNumbers(src);
                TestHelper.GenerateRandomComplexNumbers(dst);
                for (var i = 0; i < exp.Length; i++)
                {
                    exp[i] = src[i];
                }
            }
        }
        #endregion
        #region ExtractMagnitudeSquared
        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ExtractMagnitudeSquaredFallbackSingleExtractsCorrectly(int length)
        {
            ExtractMagnitudeSquaredTestUtils.PrepareArrays(length, out var src, out var exp, out var dst);
            ComplexUtils.Fallback.ExtractMagnitudeSquaredFallback(dst, src);
            TestHelper.AssertArrays(exp, dst);
        }
        [TestCase(4095)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void ExtractMagnitudeSquaredAvx2SingleExtractsCorrectly(int length)
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn($"{nameof(Avx2)} is not supported!");
                return;
            }
            ExtractMagnitudeSquaredTestUtils.PrepareArrays(length, out var src, out var exp, out var dst);
            ComplexUtils.X86.ExtractMagnitudeSquaredAvx2(dst, src);
            TestHelper.AssertArrays(exp, dst);
        }

        private static class ExtractMagnitudeSquaredTestUtils
        {
            internal static void PrepareArrays(int length, out ComplexF[] src, out float[] exp, out float[] dst)
            {
                src = new ComplexF[length];
                exp = new float[length];
                dst = new float[length];
                TestHelper.GenerateRandomComplexNumbers(src);
                TestHelper.GenerateRandomRealNumbers(dst);
                for (var i = 0; i < exp.Length; i++)
                {
                    exp[i] = src[i].Value.LengthSquared();
                }
            }
        }
        #endregion
    }
}
