using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shamisen.Utils;

using NUnit.Framework;

namespace Shamisen.Core.Tests.CoreFx.AudioUtilsTest
{
    public partial class AudioUtilsTests
    {
        [TestFixture(Category = "X86")]
        public class X86
        {
            private static IEnumerable<int> X86SizeTestCaseGenerator() => SizeTestCaseGenerator();

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
                AssertArray(b);
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
                AssertArray(b);
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
                AssertArray(b);
            }
        }
    }
}
