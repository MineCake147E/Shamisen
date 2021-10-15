using System.Collections.Generic;
using System.Runtime.InteropServices;

using NUnit.Framework;

using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx.AudioUtilsTest
{
    public partial class AudioUtilsTests
    {
        [TestFixture]
        public class Fallback
        {
            private static IEnumerable<int> FallbackSizeTestCaseGenerator() => SizeTestCaseGenerator();

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void InterleaveStereoWorksCorrectly(int size)
            {
                PrepareStereo(size, out int[] a0, out int[] a1, out int[] b);
                AudioUtils.Fallback.InterleaveStereoInt32(b, a0, a1);
                AssertArrayForInterleave(b);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void InterleaveThreeWorksCorrectly(int size)
            {
                PrepareThree(size, out int[] a0, out int[] a1, out int[] a2, out int[] b);
                AudioUtils.Fallback.InterleaveThreeInt32(b, a0, a1, a2);
                AssertArrayForInterleave(b);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void InterleaveQuadWorksCorrectly(int size)
            {
                PrepareQuad(size, out int[] a0, out int[] a1, out int[] a2, out int[] a3, out int[] b);
                AudioUtils.Fallback.InterleaveQuadInt32(b, a0, a1, a2, a3);
                AssertArrayForInterleave(b);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void DuplicateMonauralToStereoWorksCorrectly(int size)
            {
                PrepareDuplicate(size, 2, out int[] a, out int[] b);
                var bf = MemoryMarshal.Cast<int, float>(b);
                var af = MemoryMarshal.Cast<int, float>(a);
                AudioUtils.Fallback.DuplicateMonauralToStereo(bf, af);
                AssertArrayForDuplicate(b, 2);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void DuplicateMonauralTo3ChannelsWorksCorrectly(int size)
            {
                PrepareDuplicate(size, 3, out int[] a, out int[] b);
                var bf = MemoryMarshal.Cast<int, float>(b);
                var af = MemoryMarshal.Cast<int, float>(a);
                AudioUtils.Fallback.DuplicateMonauralTo3Channels(bf, af);
                AssertArrayForDuplicate(b, 3);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void DuplicateMonauralTo4ChannelsWorksCorrectly(int size)
            {
                PrepareDuplicate(size, 4, out int[] a, out int[] b);
                var bf = MemoryMarshal.Cast<int, float>(b);
                var af = MemoryMarshal.Cast<int, float>(a);
                AudioUtils.Fallback.DuplicateMonauralTo4Channels(bf, af);
                AssertArrayForDuplicate(b, 4);
            }
        }
    }
}
