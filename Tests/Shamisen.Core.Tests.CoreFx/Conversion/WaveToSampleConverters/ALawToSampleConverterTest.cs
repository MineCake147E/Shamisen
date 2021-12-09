using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

using NUnit.Framework;

using Shamisen.Conversion.WaveToSampleConverters;

namespace Shamisen.Core.Tests.CoreFx.Conversion.WaveToSampleConverters
{
    [TestFixture]
    public class ALawToSampleConverterTest
    {
        private const int Mask = 0b01010101;

        [TestCase((byte)(0b1_001_1010 ^ Mask), unchecked((short)0b0_000000_1_1010_1_000))]
        [TestCase((byte)(0b0_111_1111 ^ Mask), unchecked((short)-0b1_1111_1_000000_000))]
        public void ConvertsCorrectly(byte value, short expected)
            => Assert.AreEqual(expected, ALawToSampleConverter.ConvertALawToInt16(value));

        [TestCase((byte)(0b1_001_1010 ^ Mask), unchecked((short)0b0_000000_1_1010_1_000))]
        [TestCase((byte)(0b0_111_1111 ^ Mask), unchecked((short)-0b1_1111_1_000000_000))]
        public void SingleVariantConvertsCorrectly(byte value, short expected)
            => Assert.AreEqual(expected, (short)(ALawToSampleConverter.ConvertALawToSingle(value) * 32768.0f));
        [Test]
        public void SingleVariantConsistency()
            => Assert.Multiple(() =>
            {
                for (var i = 0; i < byte.MaxValue + 1; i++)
                {
                    Assert.AreEqual((float)ALawToSampleConverter.ConvertALawToInt16((byte)i), ALawToSampleConverter.ConvertALawToSingle((byte)i) * 32768.0f);
                }
            });
        [Test]
        public void BlockConvertsCorrectly()
        {
            PrepareBlock(out var buffer, out var bb);
            ALawToSampleConverter.ProcessStandard(bb, buffer);
            AssertBlock(buffer);
        }
        [Test]
        public void BlockConvertsCorrectlyAvx2()
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("Avx2 is not supported!");
                return;
            }
            PrepareBlock(out var buffer, out var bb);
            ALawToSampleConverter.ProcessAvx2(bb, buffer);
            AssertBlock(buffer);
        }
        [Test]
        public void BlockConvertsCorrectlySse41()
        {
            if (!Sse41.IsSupported)
            {
                Assert.Warn("Sse41 is not supported!");
                return;
            }
            PrepareBlock(out var buffer, out var bb);
            ALawToSampleConverter.ProcessSse41(bb, buffer);
            AssertBlock(buffer);
        }
        [Test]
        public void BlockConvertsCorrectlyAdvSimd64()
        {
            if (!AdvSimd.Arm64.IsSupported)
            {
                Assert.Warn("AdvSimd.Arm64 is not supported!");
                return;
            }
            PrepareBlock(out var buffer, out var bb);
            ALawToSampleConverter.ProcessAdvSimd64(bb, buffer);
            AssertBlock(buffer);
        }

        private static void AssertBlock(float[] buffer)
        {
            unchecked
            {
                Assert.Multiple(() =>
                {
                    long cnt = 0;
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        var s = Hash(i);
                        var expected = ALawToSampleConverter.ConvertALawToSingle(s) * 8192.0f;
                        var actual = buffer[i] * 8192.0f;
                        Assert.AreEqual(expected, actual, $"Comparing {i}th element, Conversion from {s ^ 0xd5:X2}:");
                        if (expected != actual)
                        {
                            if (cnt++ > 128)
                            {
                                Assert.Fail("More than 128 elements are wrong!");
                                return;
                            }
                        }
                    }
                });
            }
        }

        private static void PrepareBlock(out float[] buffer, out byte[] bb)
        {
            buffer = new float[8192 + 31];
            bb = new byte[buffer.Length];
            unchecked
            {
                for (var i = 0; i < bb.Length; i++)
                {
                    bb[i] = Hash(i);
                }
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static byte Hash(int i)
        {
            unchecked
            {
                return (byte)((byte)i + (byte)(i >> 8));
            }
        }
    }
}
