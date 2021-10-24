using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.WaveToSampleConverters;

namespace Shamisen.Core.Tests.CoreFx.Conversion
{
    [TestFixture]
    public class Pcm8ToSampleConverterTest
    {
        [Test]
        public void BlockConvertsCorrectly()
        {
            PrepareBlock(out var buffer, out var bb);
            Pcm8ToSampleConverter.ProcessStandard(bb, buffer);
            AssertBlock(buffer);
        }
        [Test]
        public void BlockConvertsCorrectlyAvx2M()
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("Avx2 is not supported!");
                return;
            }
            PrepareBlock(out var buffer, out var bb);
            Pcm8ToSampleConverter.ProcessAvx2M(bb, buffer);
            AssertBlock(buffer);
        }
        [Test]
        public void BlockConvertsCorrectlyAvx2A()
        {
            if (!Avx2.IsSupported)
            {
                Assert.Warn("Avx2 is not supported!");
                return;
            }
            PrepareBlock(out var buffer, out var bb);
            Pcm8ToSampleConverter.ProcessAvx2A(bb, buffer);
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
                        float expected = s - 128;
                        var actual = buffer[i] * 128.0f;
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
