
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

using NUnit.Framework;

using Shamisen.Conversion.WaveToSampleConverters;

namespace Shamisen.Core.Tests.CoreFx.Conversion
{
    [TestFixture]
    public class MuLawToSampleConverterTest
    {
        private const int Mask = byte.MaxValue;
        [TestCase((byte)(0b1_001_1010 ^ Mask), unchecked((short)(-73 << 2)))]
        [TestCase((byte)(0b0_111_1111 ^ Mask), unchecked((short)(8031 << 2)))]
        public void ConvertsCorrectly(byte value, short expected)
            => Assert.AreEqual(expected, (short)(MuLawToSampleConverter.ConvertMuLawToSingle(value) * 32768.0f));
        [Test]
        public void BlockConvertsCorrectly()
        {
            PrepareBlock(out float[] buffer, out byte[] bb);
            MuLawToSampleConverter.ProcessStandard(bb, buffer);
            AssertBlock(buffer);
        }

        [Test]
        public void BlockConvertsCorrectlyAvx2MM256()
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("Avx2 is not supported!");
                return;
            }
            PrepareBlock(out float[] buffer, out byte[] bb);
            MuLawToSampleConverter.ProcessAvx2MM256(bb, buffer);
            AssertBlock(buffer);
        }
        [Test]
        public void BlockConvertsCorrectlyAvx2MM128()
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("Avx2 is not supported!");
                return;
            }
            PrepareBlock(out float[] buffer, out byte[] bb);
            MuLawToSampleConverter.ProcessAvx2MM128(bb, buffer);
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
            PrepareBlock(out float[] buffer, out byte[] bb);
            MuLawToSampleConverter.ProcessSse41(bb, buffer);
            AssertBlock(buffer);
        }
        [Test]
        public void BlockConvertsCorrectlyAdvSimd()
        {
            if (!AdvSimd.IsSupported)
            {
                Assert.Warn("AdvSimd is not supported!");
                return;
            }
            PrepareBlock(out float[] buffer, out byte[] bb);
            MuLawToSampleConverter.ProcessAdvSimd(bb, buffer);
            AssertBlock(buffer);
        }
        [Test]
        public void BlockConvertsCorrectlyAdvSimdArm64()
        {
            if (!AdvSimd.Arm64.IsSupported)
            {
                Assert.Warn("AdvSimd.Arm64 is not supported!");
                return;
            }
            PrepareBlock(out float[] buffer, out byte[] bb);
            MuLawToSampleConverter.ProcessAdvSimdArm64(bb, buffer);
            AssertBlock(buffer);
        }

        private static void AssertBlock(float[] buffer)
        {
            unchecked
            {
                Assert.Multiple(() =>
                {
                    long cnt = 0;
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        byte s = Hash(i);
                        float expected = MuLawToSampleConverter.ConvertMuLawToSingle(s) * 8192.0f;
                        float actual = buffer[i] * 8192.0f;
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
                for (int i = 0; i < bb.Length; i++)
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
