using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

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
                PrepareStereo(size, out int[] a0, out int[] a1, out int[] b);
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
                PrepareThree(size, out int[] a0, out int[] a1, out int[] a2, out int[] b);
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
                PrepareQuad(size, out int[] a0, out int[] a1, out int[] a2, out int[] a3, out int[] b);
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
                PrepareDuplicate(size, 2, out int[] a, out int[] b);
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
                PrepareDuplicate(size, 3, out int[] a, out int[] b);
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
                PrepareDuplicate(size, 4, out int[] a, out int[] b);
                var bf = MemoryMarshal.Cast<int, float>(b);
                var af = MemoryMarshal.Cast<int, float>(a);
                AudioUtils.Fallback.DuplicateMonauralTo4Channels(bf, af);
                AssertArrayForDuplicate(b, 4);
            }
            #endregion
        }
    }
}
