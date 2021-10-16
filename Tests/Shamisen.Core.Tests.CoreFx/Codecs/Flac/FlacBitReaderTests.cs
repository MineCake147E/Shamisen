using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Codecs.Flac.Parsing;
using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Data;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Flac
{
    [TestFixture]
    public class FlacBitReaderTests
    {
        public const string ResourcesPath = TestHelper.ResourcesPath;

        #region ReadBits

        private static IEnumerable<TestCaseData> B64TestCaseGenerator() => Enumerable.Range(1, 63).SelectMany(
            a => new[] {
                new TestCaseData((byte)a, ulong.MaxValue)
                , new TestCaseData((byte)a, 0xaaaa_aaaa_aaaa_aaaau)
                , new TestCaseData((byte)a, 0x5555_5555_5555_5555u)
                , new TestCaseData((byte)a, ~(~0ul << a))
                , new TestCaseData((byte)a, ~0ul << a)
                , new TestCaseData((byte)a, 1ul << a)
            });

        private static IEnumerable<TestCaseData> B32TestCaseGenerator() => Enumerable.Range(1, 31).SelectMany(
            a => new[] {
                new TestCaseData((byte)a, ulong.MaxValue)
                , new TestCaseData((byte)a, 0xaaaa_aaaa_aaaa_aaaau)
                , new TestCaseData((byte)a, 0x5555_5555_5555_5555u)
                , new TestCaseData((byte)a, ~(~0ul << a))
                , new TestCaseData((byte)a, ~0ul << a)
                , new TestCaseData((byte)a, 1ul << a)
            });

        private static IEnumerable<TestCaseData> B32SecondTestCaseGenerator() => Enumerable.Range(1, 31).SelectMany(g => Enumerable.Range(1, 31).Select(h => ((byte)g, (byte)h))).SelectMany(
            a => new[] {
                new TestCaseData(a.Item1, a.Item2, ulong.MaxValue)
                , new TestCaseData(a.Item1, a.Item2, 0xaaaa_aaaa_aaaa_aaaau)
                , new TestCaseData(a.Item1, a.Item2, 0x5555_5555_5555_5555u)
            });

        [TestCaseSource(nameof(B64TestCaseGenerator))]
        public void ReadBitsUInt64CorrectlyReadsOnFirstTouch(byte bits, ulong data)
        {
            Span<byte> w = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(w, data);
            ulong expected = data >> (64 - bits);
            using (var ms = new MemoryStream())
            {
                ms.Write(w);
                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var mds = new MemoryDataSource(ms))
                using (var reader = new FlacBitReader(mds))
                {
                    ulong? read = reader.ReadBitsUInt64(bits);
                    Assert.AreEqual(expected, read);
                }
            }
        }

        [TestCaseSource(nameof(B32SecondTestCaseGenerator))]
        public void ReadBitsUInt32CorrectlyReadsOnSecondTouch(byte bitsSecond, byte bitsFirst, ulong data)
        {
            Span<byte> w = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(w, data);
            uint expected = (uint)((data << bitsFirst) >> (64 - bitsSecond));
            using (var ms = new MemoryStream())
            {
                ms.Write(w);
                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var mds = new MemoryDataSource(ms))
                using (var reader = new FlacBitReader(mds))
                {
                    _ = reader.ReadBitsUInt32(bitsFirst);
                    uint? read = reader.ReadBitsUInt32(bitsSecond);
                    Assert.AreEqual(expected, read);
                }
            }
        }

        [TestCaseSource(nameof(B32TestCaseGenerator))]
        public void ReadBitsUInt32CorrectlyReadsAfterWordBoundary(byte bits, ulong data)
        {
            Span<byte> w = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(w, data);
            uint expected = (uint)(data >> (64 - bits));
            using (var ms = new MemoryStream())
            {
                ms.Write(w);
                ms.Write(w);
                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var mds = new MemoryDataSource(ms))
                using (var reader = new FlacBitReader(mds))
                {
                    _ = reader.ReadBitsUInt64(64);
                    uint? read = reader.ReadBitsUInt32(bits);
                    Assert.AreEqual(expected, read);
                }
            }
        }

        [TestCaseSource(nameof(B32TestCaseGenerator))]
        public void ReadBitsUInt32CorrectlyReadsAcrossWordBoundary(byte bits, ulong data)
        {
            Span<byte> w = stackalloc byte[sizeof(ulong)];
            byte bhalf = (byte)(bits >> 1);
            ulong rotatedData = (data << bhalf) | (data >> (64 - bhalf));
            BinaryPrimitives.WriteUInt64BigEndian(w, rotatedData);
            uint expected = (uint)(data >> (64 - bits));
            using (var ms = new MemoryStream())
            {
                ms.Write(w);
                ms.Write(w);
                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var mds = new MemoryDataSource(ms))
                using (var reader = new FlacBitReader(mds))
                {
                    _ = reader.ReadBitsUInt64((byte)(64 - bhalf));
                    uint? read = reader.ReadBitsUInt32(bits);
                    Assert.AreEqual(expected, read);
                }
            }
        }

        [TestCaseSource(nameof(B32TestCaseGenerator))]
        public void ReadBitsUInt32CorrectlyAscendWord(byte bits, ulong data)
        {
            Span<byte> w = stackalloc byte[sizeof(ulong)];
            byte bhalf = (byte)(bits >> 1);
            ulong rotatedData = (data << bhalf) | (data >> (64 - bhalf));
            BinaryPrimitives.WriteUInt64BigEndian(w, rotatedData);
            uint expected = (uint)(data >> (64 - bits));
            using (var ms = new MemoryStream())
            {
                ms.Write(w);
                ms.Write(w);
                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var mds = new MemoryDataSource(ms))
                using (var reader = new FlacBitReader(mds))
                {
                    _ = reader.ReadBitsUInt64((byte)(64 - bhalf));
                    Assert.AreEqual((64 - bhalf) % 64, reader.ConsumedBits, "ConsumedBits is different at first read!");
                    Assert.AreEqual((64 - bhalf) / 64, reader.ConsumedWords, "ConsumedWords is different at first read!");
                    uint? read = reader.ReadBitsUInt32(bits);
                    Assert.AreEqual(bits - bhalf, reader.ConsumedBits, "ConsumedBits is different at second read!");
                    Assert.AreEqual(1, reader.ConsumedWords, "ConsumedWords is different at second read!");
                    Assert.AreEqual(expected, read);
                }
            }
        }

        #endregion ReadBits

        #region ReadUnaryUnsigned

        private static IEnumerable<TestCaseData> ReadUnaryUnsignedCorrectlyReadsTestCaseGenerator() => Enumerable.Range(1, 63)
            .Select(a => new TestCaseData((byte)a));

        [TestCaseSource(nameof(ReadUnaryUnsignedCorrectlyReadsTestCaseGenerator))]
        public void ReadUnaryUnsignedReadsCorrectlyShort(byte bits)
        {
            using var dc = new DataCache<byte>();
            Span<byte> w = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(w, ~0ul >> bits);
            uint expected = bits;
            dc.Write(w);
            using var reader = new FlacBitReader(dc);
            bool flag = reader.ReadUnaryUnsigned(out uint read);
            Assert.IsTrue(flag);
            Assert.AreEqual(expected, read);
        }

        public static IEnumerable<uint> ReadUnaryUnsignedTestCaseGenerator() => Enumerable.Range(0, 32 - 5).Select(a => 64u << a).Concat(new[] { uint.MaxValue });

        /// <summary>
        /// Tests if the <see cref="FlacBitReader.ReadUnaryUnsigned(out uint)"/> reads 0-continuing unary code correctly, for 0-bit and longer unary codes.
        /// Contains <b>512MiB IN-MEMORY READ TEST</b> so you can avoid running them until you have high-end CPUs.
        /// </summary>
        /// <param name="bits">The bits.</param>
        [TestCaseSource(nameof(ReadUnaryUnsignedTestCaseGenerator))]
        public void ReadUnaryUnsignedReadsCorrectlyLong(uint bits)
        {
            using var reader = new FlacBitReader(new UnaryDataSource(bits));
            Assert.Multiple(() =>
            {
                Assert.IsTrue(reader.ReadUnaryUnsigned(out uint value));
                Assert.AreEqual(bits, value);
            });
        }

        #endregion ReadUnaryUnsigned

        #region ReadRiceCodes

        private static IEnumerable<TestCaseData> ReadRiceCodesReadsCorrectlyTestCaseGenerator()
        {
            const string Prefix = "ReadRiceCodesReadsCorrectly";
            for (int i = 1; i < 32; i++)
            {
                yield return new TestCaseData(i, 0x8000_0000_ffff_fffful, new int[] { 0 })
                    .SetName(Prefix + "SingleZero{a}");
                yield return new TestCaseData(i, 0x8000_0000_ffff_fffful | (0x8000_0000_0000_0000ul >> i), new int[] { -1 })
                    .SetName(Prefix + "SingleMinusOne{a}");
                yield return new TestCaseData(i, 0xffff_ffff_ffff_fffful, new int[] { -1 << (i - 1) })
                    .SetName(Prefix + "SingleMinDenormalized{a}");
                yield return new TestCaseData(i, 0xffff_ffff_ffff_fffful ^ (0x8000_0000_0000_0000ul >> i), new int[] { (1 << (i - 1)) - 1 })
                    .SetName(Prefix + "SingleMaxDenormalized{a}");
            }
            yield return new TestCaseData(3, 0x9999_9999_9999_9999ul, Enumerable.Repeat(-1, 64 / 4).ToArray());
            yield return new TestCaseData(3, 0x8888_8888_8888_8888ul, Enumerable.Repeat(0, 64 / 4).ToArray());
            yield return new TestCaseData(3, 0x89ab_cdef_89ab_cdeful, new[] { 0, -1, 1, -2, 2, -3, 3, -4, 0, -1, 1, -2, 2, -3, 3, -4 });
        }

        private static IEnumerable<TestCaseData> ReadRiceCodesReadsTwiceCorrectlyMultipleTestCaseGenerator()
        {
            yield return new TestCaseData(3, 0x9999_9999_9999_9999ul, Enumerable.Repeat(-1, 64 / 4).ToArray());
            yield return new TestCaseData(3, 0x8888_8888_8888_8888ul, Enumerable.Repeat(0, 64 / 4).ToArray());
            yield return new TestCaseData(3, 0x89ab_cdef_89ab_cdeful, new[] { 0, -1, 1, -2, 2, -3, 3, -4, 0, -1, 1, -2, 2, -3, 3, -4 });
        }

        [TestCaseSource(nameof(ReadRiceCodesReadsCorrectlyTestCaseGenerator))]
        public void ReadRiceCodesReadsCorrectlyMultiple(int parameter, ulong data, int[] values)
        {
            using var dc = new DataCache<byte>();
            Span<byte> w = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(w, data);
            dc.Write(w);
            using var reader = new FlacBitReader(dc);
            int[] m = new int[values.Length];
            bool rr = reader.ReadRiceCodes(m.AsSpan(), parameter);
            Assert.IsTrue(rr);
            Assert.Multiple(() =>
            {
                for (int i = 0; i < m.Length; i++)
                {
                    Assert.AreEqual(values[i], m[i], $"Decoding {i}th data from {data:X} with parameter {parameter}");
                }
            });
        }

        [TestCaseSource(nameof(ReadRiceCodesReadsTwiceCorrectlyMultipleTestCaseGenerator))]
        public void ReadRiceCodesReadsTwiceCorrectlyMultiple(int parameter, ulong data, int[] values)
        {
            using var dc = new DataCache<byte>();
            Span<byte> w = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(w, data);
            dc.Write(w);
            ulong rdata = BinaryPrimitives.ReverseEndianness(data);
            rdata = ((rdata << 4) & 0xf0f0_f0f0_f0f0_f0f0ul) | ((rdata >> 4) & 0x0f0f_0f0f_0f0f_0f0ful);
            BinaryPrimitives.WriteUInt64BigEndian(w, rdata);
            dc.Write(w);
            using var reader = new FlacBitReader(dc);
            int[] m = new int[values.Length];
            bool rr = reader.ReadRiceCodes(m.AsSpan(), parameter);
            Assert.IsTrue(rr);
            Assert.Multiple(() =>
            {
                for (int i = 0; i < m.Length; i++)
                {
                    Assert.AreEqual(values[i], m[i], $"Decoding {i}th data from {data:X} with parameter {parameter}");
                }
            });
            bool rr2 = reader.ReadRiceCodes(m.AsSpan(), parameter);
            Assert.IsTrue(rr2);
            Assert.Multiple(() =>
            {
                for (int i = 0; i < m.Length; i++)
                {
                    Assert.AreEqual(values[i], m[m.Length - i - 1], $"Decoding {i}th data from {(BinaryPrimitives.ReverseEndianness(data)):X} with parameter {parameter}");
                }
            });
        }

        #endregion ReadRiceCodes

        #region ReadUtf8

        private static IEnumerable<TestCaseData> Utf8TestCaseGenerator()
        {
            var ms = TestHelper.GetDataStreamFromResource("Utf8TestCases.txt");
            var sr = new StreamReader(ms, Encoding.UTF8);
            while (true)
            {
                string f = sr.ReadLine();
                if (f is not null)
                {
                    yield return new TestCaseData(f);
                    continue;
                }
                break;
            }
        }

        [TestCaseSource(nameof(Utf8TestCaseGenerator))]
        public void ReadUtf8UInt32ReadsCorrectly(string data)
        {
            byte[] t = Encoding.UTF8.GetBytes(data);
            using var dc = new DataCache<byte>();
            dc.Write(t);
            using var reader = new FlacBitReader(dc);
            Assert.Multiple(() =>
            {
                foreach (var item in data.EnumerateRunes())
                {
                    bool read = reader.ReadUtf8UInt32(out uint value, default, out int br);
                    Assert.IsTrue(read);
                    Assert.AreEqual(item.Value, (int)value);
                }
            });
        }

        [TestCaseSource(nameof(Utf8TestCaseGenerator))]
        public void ReadUtf8UInt64ReadsCorrectly(string data)
        {
            byte[] t = Encoding.UTF8.GetBytes(data);
            using var dc = new DataCache<byte>();
            dc.Write(t);
            using var reader = new FlacBitReader(dc);
            Assert.Multiple(() =>
            {
                foreach (var item in data.EnumerateRunes())
                {
                    bool read = reader.ReadUtf8UInt64(out ulong value, default, out int br);
                    Assert.IsTrue(read);
                    Assert.AreEqual(item.Value, (int)value);
                }
            });
        }

        #endregion ReadUtf8
    }
}
