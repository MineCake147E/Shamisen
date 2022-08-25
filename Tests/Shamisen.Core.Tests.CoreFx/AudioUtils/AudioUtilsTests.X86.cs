using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.TestUtils;
using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx.AudioUtilsTest
{
    public partial class AudioUtilsTests
    {
        [TestFixture(Category = "X86")]
        public class X86
        {
            private static IEnumerable<int> X86SizeTestCaseGenerator() => SizeTestCaseGenerator();

            private static IEnumerable<int> X86ChannelsTestCaseGenerator() => ChannelsTestCaseGenerator();
            #region Interleave
            [TestCaseSource(nameof(X86SizeTestCaseGenerator))]
            public void InterleaveStereoWorksCorrectly(int size)
            {
                PrepareStereo(size, out var a0, out var a1, out var b);
                if (!AudioUtils.X86.IsSupported)
                {
                    Assert.Warn("X86 intrinsics is not supported on this machine!");
                    return;
                }
                AudioUtils.X86.InterleaveStereoInt32(b, a0, a1);
                AssertArrayForInterleave(b);
            }

            [TestCaseSource(nameof(X86SizeTestCaseGenerator))]
            public void InterleaveThreeWorksCorrectly(int size)
            {
                PrepareThree(size, out var a0, out var a1, out var a2, out var b);
                if (!AudioUtils.X86.IsSupported)
                {
                    Assert.Warn("X86 intrinsics is not supported on this machine!");
                    return;
                }
                AudioUtils.X86.InterleaveThreeInt32(b, a0, a1, a2);
                AssertArrayForInterleave(b);
            }

            [TestCaseSource(nameof(X86SizeTestCaseGenerator))]
            public void InterleaveQuadWorksCorrectly(int size)
            {
                PrepareQuad(size, out var a0, out var a1, out var a2, out var a3, out var b);
                if (!AudioUtils.X86.IsSupported)
                {
                    Assert.Warn("X86 intrinsics is not supported on this machine!");
                    return;
                }
                AudioUtils.X86.InterleaveQuadInt32(b, a0, a1, a2, a3);
                AssertArrayForInterleave(b);
            }
            #endregion

            #region DuplicateMonauralToChannels
            [TestCaseSource(nameof(X86SizeTestCaseGenerator))]
            public void DuplicateMonauralToStereoWorksCorrectly(int size)
            {
                if (!AudioUtils.X86.IsSupported)
                {
                    Assert.Warn("X86 intrinsics is not supported on this machine!");
                    return;
                }
                PrepareDuplicate(size, 2, out var a, out var b);
                var bf = MemoryMarshal.Cast<int, float>(b);
                var af = MemoryMarshal.Cast<int, float>(a);
                AudioUtils.X86.DuplicateMonauralToStereo(bf, af);
                AssertArrayForDuplicate(b, 2);
            }

            [TestCaseSource(nameof(X86SizeTestCaseGenerator))]
            public void DuplicateMonauralTo3ChannelsWorksCorrectly(int size)
            {
                if (!AudioUtils.X86.IsSupported)
                {
                    Assert.Warn("X86 intrinsics is not supported on this machine!");
                    return;
                }
                PrepareDuplicate(size, 3, out var a, out var b);
                var bf = MemoryMarshal.Cast<int, float>(b);
                var af = MemoryMarshal.Cast<int, float>(a);
                AudioUtils.Fallback.DuplicateMonauralTo3Channels(bf, af);
                AssertArrayForDuplicate(b, 3);
            }

            [TestCaseSource(nameof(X86SizeTestCaseGenerator))]
            public void DuplicateMonauralTo4ChannelsWorksCorrectly(int size)
            {
                if (!AudioUtils.X86.IsSupported)
                {
                    Assert.Warn("X86 intrinsics is not supported on this machine!");
                    return;
                }
                PrepareDuplicate(size, 4, out var a, out var b);
                var bf = MemoryMarshal.Cast<int, float>(b);
                var af = MemoryMarshal.Cast<int, float>(a);
                AudioUtils.Fallback.DuplicateMonauralTo4Channels(bf, af);
                AssertArrayForDuplicate(b, 4);
            }
            #endregion

            #region Deinterleave
            [TestCaseSource(nameof(X86ChannelsTestCaseGenerator))]
            public void DeinterleaveChannelsAvx2WorksCorrectly(int channels)
            {
                if (!Avx2.IsSupported)
                {
                    Assert.Warn("AVX2 is not supported on this machine!");
                    return;
                }
                const int Size = 1023;
                PrepareDeinterleave(Size, channels, out var sA, out var dA);
                var src = MemoryMarshal.Cast<int, float>(sA.AsSpan());
                var dst = MemoryMarshal.Cast<int, float>(dA.AsSpan());
                AudioUtils.X86.DeinterleaveChannelsSingleAvx2(dst, src, channels, Size);
                AssertArrayForInterleave(dA);
            }
            #endregion

            #region Floating-Point Utils
            [TestCase(4094)]
            [TestCase(4095)]
            [TestCase(4096)]
            public void ReplaceNaNsWithAvxReplacesCorrectly(int size, float value = 0.0f)
            {
                if (!Avx.IsSupported)
                {
                    Assert.Warn($"{nameof(Avx)} is not supported on this machine!");
                    return;
                }
                GenerateReplaceNaNsWithTestArrays(size, value, out var src, out var exp, out var act);
                AudioUtils.X86.ReplaceNaNsWithAvx(act, src, value);
                TestHelper.AssertArrays(exp, act);
            }
            [TestCase(4094)]
            [TestCase(4095)]
            [TestCase(4096)]
            public void ReplaceNaNsWithAvx2ReplacesCorrectly(int size, float value = 0.0f)
            {
                if (!Avx2.IsSupported)
                {
                    Assert.Warn($"{nameof(Avx2)} is not supported on this machine!");
                    return;
                }
                GenerateReplaceNaNsWithTestArrays(size, value, out var src, out var exp, out var act);
                AudioUtils.X86.ReplaceNaNsWithAvx2(act, src, value);
                TestHelper.AssertArrays(exp, act);
            }
            #endregion

            #region Log2
            #region Rough assertion
            [TestCase(4095)]
            [TestCase(128)]
            public void FastLog2Order5FAvx2FmaCalculatesCorrectly(int size)
            {
                if (!Avx2.IsSupported || !Fma.IsSupported)
                {
                    Assert.Warn("Either AVX2 or FMA is not supported on this machine!");
                    return;
                }
                GenerateLog2TestArrays(size, out var src, out var exp, out var act);
                AudioUtils.X86.FastLog2Order5FAvx2Fma(act, src);
                TestHelper.AssertArrays(exp, act, MaxLog2Error);
            }

            [TestCase(4095)]
            [TestCase(128)]
            public void FastLog2Order5Avx2CalculatesCorrectly(int size)
            {
                if (!Avx2.IsSupported)
                {
                    Assert.Warn("AVX2 is not supported on this machine!");
                    return;
                }
                GenerateLog2TestArrays(size, out var src, out var exp, out var act);
                AudioUtils.X86.FastLog2Order5Avx2(act, src);
                TestHelper.AssertArrays(exp, act, MaxLog2Error);
            }
            [TestCase(4095)]
            [TestCase(128)]
            public void FastLog2Order5Sse2CalculatesCorrectly(int size)
            {
                if (!Sse2.IsSupported)
                {
                    Assert.Warn("SSE2 is not supported on this machine!");
                    return;
                }
                GenerateLog2TestArrays(size, out var src, out var exp, out var act);
                AudioUtils.X86.FastLog2Order5Sse2(act, src);
                TestHelper.AssertArrays(exp, act, MaxLog2Error);
            }
            #endregion
            #region Exhaustive test in parameter range [1.0f, 2.0f)
            [Test]
            public void FastLog2Order5FAvx2FmaCalculatesAccurately()
            {
                if (!Avx2.IsSupported || !Fma.IsSupported)
                {
                    Assert.Warn("Either AVX2 or FMA is not supported on this machine!");
                    return;
                }
                var src = new float[65536];
                var dst = new float[src.Length];
                var maxError = double.MinValue;
                var sumError = new NeumaierAccumulator(0.0, 0.0);
                ErrorUtils.GenerateIndexValuedArraySingle(src, 1.0f);
                var sSrc = src.AsSpan();
                var sDst = dst.AsSpan();
                var span = MemoryMarshal.Cast<float, int>(sSrc);
                for (var k = 0; k < 256; k++)
                {
                    AudioUtils.X86.FastLog2Order5FAvx2Fma(dst, src);
                    (maxError, sumError) = CheckLog2(maxError, sSrc, sDst, sumError);
                    AudioUtils.Offset(span, span, src.Length);
                }
                Console.WriteLine($"Maximum Error: {maxError}");
                Assert.AreEqual(0.0, maxError, 1.5E-5);
            }

            [Test]
            public void FastLog2Order5Avx2CalculatesAccurately()
            {
                if (!Avx2.IsSupported)
                {
                    Assert.Warn("Either AVX2 or FMA is not supported on this machine!");
                    return;
                }
                var src = new float[65536];
                var dst = new float[src.Length];
                var maxError = double.MinValue;
                var sumError = new NeumaierAccumulator(0.0, 0.0);
                ErrorUtils.GenerateIndexValuedArraySingle(src, 1.0f);
                var sSrc = src.AsSpan();
                var sDst = dst.AsSpan();
                var span = MemoryMarshal.Cast<float, int>(sSrc);
                for (var k = 0; k < 256; k++)
                {
                    AudioUtils.X86.FastLog2Order5Avx2(dst, src);
                    (maxError, sumError) = CheckLog2(maxError, sSrc, sDst, sumError);
                    AudioUtils.Offset(span, span, src.Length);
                }
                Console.WriteLine($"Maximum Error: {maxError}");
                Assert.AreEqual(0.0, maxError, 1.5E-5);
            }
            [Test]
            public void FastLog2Order5Sse2CalculatesAccurately()
            {
                if (!Sse2.IsSupported)
                {
                    Assert.Warn("SSE2 is not supported on this machine!");
                    return;
                }
                var src = new float[65536];
                var dst = new float[src.Length];
                var maxError = double.MinValue;
                var sumError = new NeumaierAccumulator(0.0, 0.0);
                ErrorUtils.GenerateIndexValuedArraySingle(src, 1.0f);
                var sSrc = src.AsSpan();
                var sDst = dst.AsSpan();
                var span = MemoryMarshal.Cast<float, int>(sSrc);
                for (var k = 0; k < 256; k++)
                {
                    AudioUtils.X86.FastLog2Order5Sse2(dst, src);
                    (maxError, sumError) = CheckLog2(maxError, sSrc, sDst, sumError);
                    AudioUtils.Offset(span, span, src.Length);
                }
                Console.WriteLine($"Maximum Error: {maxError}");
                Assert.AreEqual(0.0, maxError, 1.5E-5);
            }
            #endregion
            #endregion
        }
    }
}
