using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using NUnit.Framework;

using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx.AudioUtilsTest
{
    public partial class AudioUtilsTests
    {
        [TestFixture]
        public class Fallback
        {
            private static IEnumerable<int> FallbackSizeTestCaseGenerator() => SizeTestCaseGenerator();
            #region Interleave
            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void InterleaveStereoWorksCorrectly(int size)
            {
                PrepareStereo(size, out var a0, out var a1, out var b);
                AudioUtils.Fallback.InterleaveStereoInt32(b, a0, a1);
                AssertArrayForInterleave(b);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void InterleaveThreeWorksCorrectly(int size)
            {
                PrepareThree(size, out var a0, out var a1, out var a2, out var b);
                AudioUtils.Fallback.InterleaveThreeInt32(b, a0, a1, a2);
                AssertArrayForInterleave(b);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void InterleaveQuadWorksCorrectly(int size)
            {
                PrepareQuad(size, out var a0, out var a1, out var a2, out var a3, out var b);
                AudioUtils.Fallback.InterleaveQuadInt32(b, a0, a1, a2, a3);
                AssertArrayForInterleave(b);
            }

            #endregion

            #region Duplicate

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void DuplicateMonauralToStereoWorksCorrectly(int size)
            {
                PrepareDuplicate(size, 2, out var a, out var b);
                var bf = MemoryMarshal.Cast<int, float>(b);
                var af = MemoryMarshal.Cast<int, float>(a);
                AudioUtils.Fallback.DuplicateMonauralToStereo(bf, af);
                AssertArrayForDuplicate(b, 2);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void DuplicateMonauralTo3ChannelsWorksCorrectly(int size)
            {
                PrepareDuplicate(size, 3, out var a, out var b);
                var bf = MemoryMarshal.Cast<int, float>(b);
                var af = MemoryMarshal.Cast<int, float>(a);
                AudioUtils.Fallback.DuplicateMonauralTo3Channels(bf, af);
                AssertArrayForDuplicate(b, 3);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void DuplicateMonauralTo4ChannelsWorksCorrectly(int size)
            {
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
            public void ReplaceNaNsWithFallbackReplacesCorrectly(int size, float value = 0.0f)
            {
                GenerateReplaceNaNsWithTestArrays(size, value, out var src, out var exp, out var act);
                AudioUtils.Fallback.ReplaceNaNsWithFallback(act, src, value);
                TestHelper.AssertArrays(exp, act);
            }
            #endregion

            #region Log2
            [TestCase(4095)]
            [TestCase(128)]
            public void FastLog2Order5FallbackCalculatesCorrectly(int size)
            {
                GenerateLog2TestArrays(size, out var src, out var exp, out var act);
                AudioUtils.Fallback.FastLog2Order5Fallback(act, src);
                TestHelper.AssertArrays(exp, act, MaxLog2Error);
            }
            #endregion
        }
    }
}
