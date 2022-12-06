using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;

using NUnit.Framework;

using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class SpanUtilsTest
    {
        private const int Size = 8209;
        #region ReverseEndiannessWorksCorrectly
        #region UInt64
        [Test]
        public void ReverseEndiannessUInt64FallbackWorksCorrectly()
        {
            Span<ulong> spanS = new ulong[SpanUtilsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessFallback(spanD);
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
            Span<ulong> spanS = new ulong[SpanUtilsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessAvx2(spanD);
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
            Span<ulong> spanS = new ulong[SpanUtilsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessSsse3(spanD);
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
            Span<ulong> spanS = new ulong[SpanUtilsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessAdvSimd(spanD);
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
            Span<ulong> spanS = new ulong[SpanUtilsTest.Size];
            Span<ulong> spanD = new ulong[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessAdvSimdArm64(spanD);
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
            Span<int> spanS = new int[SpanUtilsTest.Size];
            Span<int> spanD = new int[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessFallback(spanD);
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
            Span<int> spanS = new int[SpanUtilsTest.Size];
            Span<int> spanD = new int[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessAvx2(spanD);
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
            Span<int> spanS = new int[SpanUtilsTest.Size];
            Span<int> spanD = new int[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessSsse3(spanD);
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
            Span<Int24> spanS = new Int24[SpanUtilsTest.Size];
            Span<Int24> spanD = new Int24[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessFallback(spanD);
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
            Span<Int24> spanS = new Int24[SpanUtilsTest.Size];
            Span<Int24> spanD = new Int24[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessSsse3(spanD);
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
            Span<short> spanS = new short[SpanUtilsTest.Size];
            Span<short> spanD = new short[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessFallback(spanD);
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
            Span<short> spanS = new short[SpanUtilsTest.Size];
            Span<short> spanD = new short[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessAvx2(spanD);
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
            Span<short> spanS = new short[SpanUtilsTest.Size];
            Span<short> spanD = new short[spanS.Length];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(spanS));
            spanS.CopyTo(spanD);
            SpanUtils.ReverseEndiannessSsse3(spanD);
            for (var i = 0; i < spanD.Length; i++)
            {
                if (spanD[i] != BinaryPrimitives.ReverseEndianness(spanS[i])) Assert.Fail("The ReverseEndianness doesn't reverse correctly!");
            }
            Assert.Pass();
        }

        #endregion
        #endregion

        #region QuickFillFillsCorrectly

        [Test]
        public void QuickFillFillsCorrectlyBool()
        {
            const int Offset = 352;
            Span<bool> act = new bool[Offset * 2 + 65536];
            Span<bool> exp = new bool[Offset * 2 + 65536];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(exp));
            _ = exp.TryCopyTo(act);
            var value = false;
            exp.Slice(Offset, 65536).Fill(value);
            act.Slice(Offset, 65536).QuickFill(value);
            TestHelper.AreEqual(exp, act);
        }

        [Test]
        public void QuickFillFillsCorrectlyHalf()
        {
            const int Offset = 176;
            Span<Half> act = new Half[Offset * 2 + 65536];
            Span<Half> exp = new Half[Offset * 2 + 65536];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(exp));
            _ = exp.TryCopyTo(act);
            var value = (Half)MathF.PI;
            exp.Slice(Offset, 65536).Fill(value);
            act.Slice(Offset, 65536).QuickFill(value);
            TestHelper.AreEqual(exp, act);
        }

        [Test]
        public void QuickFillFillsCorrectlyInt24()
        {
            const int Offset = 117;
            Span<Int24> act = new Int24[Offset * 2 + 65536];
            Span<Int24> exp = new Int24[Offset * 2 + 65536];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(exp));
            _ = exp.TryCopyTo(act);
            var value = new Int24(unchecked((int)0x8076_5432));
            exp.Slice(Offset, 65536).Fill(value);
            act.Slice(Offset, 65536).QuickFill(value);
            TestHelper.AreEqual(exp, act);
        }

        [Test]
        public void QuickFillFillsCorrectlyDateOnly()
        {
            const int Offset = 88;
            Span<DateOnly> act = new DateOnly[Offset * 2 + 65536];
            Span<DateOnly> exp = new DateOnly[Offset * 2 + 65536];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(exp));
            _ = exp.TryCopyTo(act);
            var value = DateOnly.FromDateTime(DateTime.Now);
            exp.Slice(Offset, 65536).Fill(value);
            act.Slice(Offset, 65536).QuickFill(value);
            TestHelper.AreEqual(exp, act);
        }

        [Test]
        public void QuickFillFillsCorrectly5BytesStruct()
        {
            const int Offset = 70;
            Span<Struct5Bytes> act = new Struct5Bytes[Offset * 2 + 65536];
            Span<Struct5Bytes> exp = new Struct5Bytes[Offset * 2 + 65536];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(exp));
            _ = exp.TryCopyTo(act);
            var value = new Struct5Bytes(0x12345678, 0x25);
            exp.Slice(Offset, 65536).Fill(value);
            act.Slice(Offset, 65536).QuickFill(value);
            TestHelper.AreEqual(exp, act);
            Assert.Pass();
        }

        [Test]
        public void QuickFillFillsCorrectly6BytesStruct()
        {
            const int Offset = 58;
            Span<Vector2Int24> act = new Vector2Int24[Offset * 2 + 65536];
            Span<Vector2Int24> exp = new Vector2Int24[Offset * 2 + 65536];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(exp));
            _ = exp.TryCopyTo(act);
            var value = new Vector2Int24(new Int24(unchecked((int)0x8076_5432)), new Int24(unchecked(-(int)0x8076_5432)));
            exp.Slice(Offset, 65536).Fill(value);
            act.Slice(Offset, 65536).QuickFill(value);
            TestHelper.AreEqual(exp, act);
            Assert.Pass();
        }

        [Test]
        public void QuickFillFillsCorrectly7BytesStruct()
        {
            const int Offset = 50;
            Span<Struct7Bytes> act = new Struct7Bytes[Offset * 2 + 65536];
            Span<Struct7Bytes> exp = new Struct7Bytes[Offset * 2 + 65536];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(exp));
            _ = exp.TryCopyTo(act);
            var value = new Struct7Bytes(0x12345678, new Int24(unchecked((int)0x8076_5432)));
            exp.Slice(Offset, 65536).Fill(value);
            act.Slice(Offset, 65536).QuickFill(value);
            TestHelper.AreEqual(exp, act);
            Assert.Pass();
        }

        [Test]
        public void QuickFillFillsCorrectlyDateTime()
        {
            const int Offset = 44;
            Span<DateTime> act = new DateTime[Offset * 2 + 65536];
            Span<DateTime> exp = new DateTime[Offset * 2 + 65536];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(exp));
            _ = exp.TryCopyTo(act);
            var value = DateTime.Now;
            exp.Slice(Offset, 65536).Fill(value);
            act.Slice(Offset, 65536).QuickFill(value);
            TestHelper.AreEqual(exp, act);
        }

        [Test]
        public void QuickFillFillsCorrectlyDecimal()
        {
            const int Offset = 22;
            Span<decimal> act = new decimal[Offset * 2 + 65536];
            Span<decimal> exp = new decimal[Offset * 2 + 65536];
            RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(exp));
            _ = exp.TryCopyTo(act);
            var value = -1m;
            exp.Slice(Offset, 65536).Fill(value);
            act.Slice(Offset, 65536).QuickFill(value);
            TestHelper.AreEqual(exp, act);
        }

        [StructLayout(LayoutKind.Sequential, Size = 5)]
        private readonly struct Struct5Bytes
        {
            private readonly int y;
            private readonly byte q;

            public Struct5Bytes(int y, byte q)
            {
                this.y = y;
                this.q = q;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 6)]
        private readonly struct Vector2Int24
        {
            private readonly Int24 x, y;

            public Vector2Int24(Int24 x, Int24 y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 5)]
        private readonly struct Struct7Bytes
        {
            private readonly int y;
            private readonly Int24 q;

            public Struct7Bytes(int y, Int24 q)
            {
                this.y = y;
                this.q = q;
            }
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
    }
}
