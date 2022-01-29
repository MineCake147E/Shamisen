using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx.AudioUtilsTest
{
    public partial class AudioUtilsTests
    {
        [TestFixture(Category = "X86")]
        public class X86
        {
            private static IEnumerable<int> X86SizeTestCaseGenerator() => SizeTestCaseGenerator();
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
                TestHelper.AssertArrays(exp, act, 0.1);
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
                TestHelper.AssertArrays(exp, act, 0.1);
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
                TestHelper.AssertArrays(exp, act, 0.1);
            }
            #endregion
        }
    }
}
