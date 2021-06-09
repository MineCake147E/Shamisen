using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Codecs.Flac;
using Shamisen.Core.Tests.CoreFx.AudioUtilsTest;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Flac
{
    public class FlacSideStereoUtilsTests
    {
        internal static IEnumerable<int> SizeTestCaseGenerator() => AudioUtilsTests.SizeTestCaseGenerator();

        [TestFixture]
        public class Fallback
        {
            private static IEnumerable<int> FallbackSizeTestCaseGenerator() => SizeTestCaseGenerator();

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void DecodeAndInterleaveLeftSideStereo(int size)
            {
                PrepareLeftSideStereo(size, out var a0, out var a1, out var b);
                FlacSideStereoUtils.Fallback.DecodeAndInterleaveLeftSideStereoInt32(b, a0, a1);
                AssertArray(b);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void DecodeAndInterleaveRightSideStereo(int size)
            {
                PrepareRightSideStereo(size, out var a0, out var a1, out var b);
                FlacSideStereoUtils.Fallback.DecodeAndInterleaveRightSideStereoInt32(b, a0, a1);
                AssertArray(b);
            }

            [TestCaseSource(nameof(FallbackSizeTestCaseGenerator))]
            public void DecodeAndInterleaveMidSideStereo(int size)
            {
                PrepareMidSideStereo(size, out var a0, out var a1, out var b);
                FlacSideStereoUtils.Fallback.DecodeAndInterleaveMidSideStereoInt32(b, a0, a1);
                AssertArray(b);
            }
        }

        [TestFixture(Category = "X86")]
        public class X86
        {
            private static IEnumerable<int> X86SizeTestCaseGenerator() => SizeTestCaseGenerator();

            [TestCaseSource(nameof(X86SizeTestCaseGenerator))]
            public void DecodeAndInterleaveLeftSideStereo(int size)
            {
                PrepareLeftSideStereo(size, out var a0, out var a1, out var b);
                FlacSideStereoUtils.X86.DecodeAndInterleaveLeftSideStereoInt32(b, a0, a1);
                AssertArray(b);
            }

            [TestCaseSource(nameof(X86SizeTestCaseGenerator))]
            public void DecodeAndInterleaveRightSideStereo(int size)
            {
                PrepareRightSideStereo(size, out var a0, out var a1, out var b);
                FlacSideStereoUtils.X86.DecodeAndInterleaveRightSideStereoInt32(b, a0, a1);
                AssertArray(b);
            }

            [TestCaseSource(nameof(X86SizeTestCaseGenerator))]
            public void DecodeAndInterleaveMidSideStereo(int size)
            {
                PrepareMidSideStereo(size, out var a0, out var a1, out var b);
                FlacSideStereoUtils.X86.DecodeAndInterleaveMidSideStereoInt32(b, a0, a1);
                AssertArray(b);
            }
        }

        internal static void AssertArray(int[] b) => AudioUtilsTests.AssertArray(b);

        internal static void PrepareLeftSideStereo(int size, out int[] a0, out int[] a1, out int[] b)
        {
            a0 = new int[size];
            a1 = new int[a0.Length];
            b = new int[a0.Length * 2];
            for (int i = 0; i < a0.Length; i++)
            {
                var s0 = i * 2;
                var s1 = i * 2 + 1;
                a0[i] = s0;
                a1[i] = s0 - s1;
            }
        }

        internal static void PrepareRightSideStereo(int size, out int[] a0, out int[] a1, out int[] b)
        {
            a0 = new int[size];
            a1 = new int[a0.Length];
            b = new int[a0.Length * 2];
            for (int i = 0; i < a0.Length; i++)
            {
                var s0 = i * 2;
                var s1 = i * 2 + 1;
                a0[i] = s0 - s1;
                a1[i] = s1;
            }
        }

        internal static void PrepareMidSideStereo(int size, out int[] a0, out int[] a1, out int[] b)
        {
            a0 = new int[size];
            a1 = new int[a0.Length];
            b = new int[a0.Length * 2];
            for (int i = 0; i < a0.Length; i++)
            {
                var s0 = i * 2;
                var s1 = i * 2 + 1;
                var mid = s0 + s1;
                var side = s0 - s1;
                mid >>= 1;
                a0[i] = mid;
                a1[i] = side;
            }
        }
    }
}
