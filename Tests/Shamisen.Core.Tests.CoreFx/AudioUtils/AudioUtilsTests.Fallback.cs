using System.Collections.Generic;

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
                PrepareStereo(size, out var a0, out var a1, out var b);
                AudioUtils.Fallback.InterleaveStereoInt32(b, a0, a1);
                AssertArray(b);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void InterleaveThreeWorksCorrectly(int size)
            {
                PrepareThree(size, out var a0, out var a1, out var a2, out var b);
                AudioUtils.Fallback.InterleaveThreeInt32(b, a0, a1, a2);
                AssertArray(b);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void InterleaveQuadWorksCorrectly(int size)
            {
                PrepareQuad(size, out var a0, out var a1, out var a2, out var a3, out var b);
                AudioUtils.Fallback.InterleaveQuadInt32(b, a0, a1, a2, a3);
                AssertArray(b);
            }
        }
    }
}
