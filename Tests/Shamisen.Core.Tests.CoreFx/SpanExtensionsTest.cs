using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;

using NUnit.Framework;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class SpanExtensionsTest
    {
        private const int Size = 8209;

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
        [Test]
        public void ReverseEndiannessFallbackWorksCorrectly()
        {
            Span<ulong> spanS = new ulong[SpanExtensionsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessFallback(spanD);
            for (int i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessAvx2WorksCorrectly()
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("AVX2 is not supported!");
                return;
            }
            Span<ulong> spanS = new ulong[SpanExtensionsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessAvx2(spanD);
            for (int i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessSsse3WorksCorrectly()
        {
            if (!Ssse3.IsSupported)
            {
                Assert.Warn("Ssse3 is not supported!");
                return;
            }
            Span<ulong> spanS = new ulong[SpanExtensionsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessSsse3(spanD);
            for (int i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessAdvSimdWorksCorrectly()
        {
            if (!AdvSimd.IsSupported)
            {
                Assert.Warn("AdvSimd is not supported!");
                return;
            }
            Span<ulong> spanS = new ulong[SpanExtensionsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessAdvSimd(spanD);
            for (int i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessAdvSimdArm64WorksCorrectly()
        {
            if (!AdvSimd.Arm64.IsSupported)
            {
                Assert.Warn("AdvSimd.Arm64 is not supported!");
                return;
            }
            Span<ulong> spanS = new ulong[SpanExtensionsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessAdvSimdArm64(spanD);
            for (int i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
    }
}
