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
            for (var i = 0; i < span.Length; i++)
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
            for (var i = 0; i < span.Length; i++)
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
            for (var i = 0; i < span.Length; i++)
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
            for (var i = 0; i < span.Length; i++)
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
            for (var i = 0; i < span.Length; i++)
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
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastFill doesn't fill correctly!");
            }
            Assert.Pass();
        }

        #endregion

        #region ReverseEndiannessWorksCorrectly
        #region UInt64
        [Test]
        public void ReverseEndiannessUInt64FallbackWorksCorrectly()
        {
            Span<ulong> spanS = new ulong[SpanExtensionsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessFallback(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessUInt64Avx2WorksCorrectly()
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
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessUInt64Ssse3WorksCorrectly()
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
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessUInt64AdvSimdWorksCorrectly()
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
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessUInt64AdvSimdArm64WorksCorrectly()
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
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        #endregion
        #region Int32
        [Test]
        public void ReverseEndiannessInt32FallbackWorksCorrectly()
        {
            Span<int> spanS = new int[SpanExtensionsTest.Size];
            Span<int> spanD = new int[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessFallback(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessInt32Avx2WorksCorrectly()
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("AVX2 is not supported!");
                return;
            }
            Span<int> spanS = new int[SpanExtensionsTest.Size];
            Span<int> spanD = new int[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessAvx2(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessInt32Ssse3WorksCorrectly()
        {
            if (!Ssse3.IsSupported)
            {
                Assert.Warn("Ssse3 is not supported!");
                return;
            }
            Span<int> spanS = new int[SpanExtensionsTest.Size];
            Span<int> spanD = new int[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessSsse3(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }

        #endregion
        #region Int24
        [Test]
        public void ReverseEndiannessInt24FallbackWorksCorrectly()
        {
            Span<Int24> spanS = new Int24[SpanExtensionsTest.Size];
            Span<Int24> spanD = new Int24[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessFallback(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != Int24.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }

        [Test]
        public void ReverseEndiannessInt24Ssse3WorksCorrectly()
        {
            if (!Ssse3.IsSupported)
            {
                Assert.Warn("Ssse3 is not supported!");
                return;
            }
            Span<Int24> spanS = new Int24[SpanExtensionsTest.Size];
            Span<Int24> spanD = new Int24[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessSsse3(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != Int24.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }

        #endregion
        #region Int16
        [Test]
        public void ReverseEndiannessInt16FallbackWorksCorrectly()
        {
            Span<short> spanS = new short[SpanExtensionsTest.Size];
            Span<short> spanD = new short[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessFallback(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessInt16Avx2WorksCorrectly()
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("AVX2 is not supported!");
                return;
            }
            Span<short> spanS = new short[SpanExtensionsTest.Size];
            Span<short> spanD = new short[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessAvx2(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }
        [Test]
        public void ReverseEndiannessInt16Ssse3WorksCorrectly()
        {
            if (!Ssse3.IsSupported)
            {
                Assert.Warn("Ssse3 is not supported!");
                return;
            }
            Span<short> spanS = new short[SpanExtensionsTest.Size];
            Span<short> spanD = new short[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanExtensions.ReverseEndiannessSsse3(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }

        #endregion
        #endregion
    }
}
