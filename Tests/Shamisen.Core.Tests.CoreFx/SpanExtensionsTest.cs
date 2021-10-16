using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;

using NUnit.Framework;

using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class SpanExtensionsTest
    {
        private const int Size = 8209;
        #region QuickFillFillsCorrectly

        [Test]
        public void QuickFillFillsCorrectlyDecimal()
        {
            Span<decimal> span = new decimal[131071];
            const decimal Value = -1m;
            span.QuickFill(Value);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastFill doesn't fill correctly!");
            }
            Assert.Pass();
        }

        [Test]
        public void QuickFillFillsCorrectlyInt24()
        {
            Span<Int24> span = new Int24[699049];
            var Value = new Int24(unchecked((int)0x8076_5432));
            span.QuickFill(Value);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastFill doesn't fill correctly!");
            }
            Assert.Pass();
        }

        #endregion

        #region FastFillFillsCorrectly
        [Test]
        public void FastFillFillsCorrectlyByte()
        {
            Span<byte> span = new byte[2097151];
            const byte Value = 0x55;
            span.FastFill(Value);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastFill doesn't fill correctly!");
            }
            Assert.Pass();
        }

        [Test]
        public void FastFillFillsCorrectlyInt16()
        {
            Span<short> span = new short[1048575];
            const short Value = 0x55aa;
            span.FastFill(Value);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastFill doesn't fill correctly!");
            }
            Assert.Pass();
        }

        [Test]
        public void FastFillFillsCorrectlySingle()
        {
            Span<float> span = new float[524287];
            const float Value = MathF.PI;
            span.FastFill(Value);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastFill doesn't fill correctly!");
            }
            Assert.Pass();
        }

        [Test]
        public void FastFillFillsCorrectlyDouble()
        {
            Span<double> span = new double[262143];
            const double Value = Math.PI;
            span.FastFill(Value);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastFill doesn't fill correctly!");
            }
            Assert.Pass();
        }

        #endregion

        [Test]
        public void FastAddAddsCorrectly()
        {
            Span<float> source = new float[32];
            Span<float> destination = new float[48];
            const int Value = 1;
            source.FastFill(Value);
            destination.FastFill(-1);

            AudioUtils.FastAdd(source, destination);
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

            AudioUtils.FastMix(source, destination, 2);
            for (int i = 0; i < source.Length; i++)
            {
                if (destination[i] != 1) Assert.Fail("The FastMix doesn't mix correctly!");
            }
            Assert.Pass();
        }


        #region ReverseEndiannessWorksCorrectly
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
        #endregion
    }
}
