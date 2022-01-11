using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx.AudioUtilsTest
{
    [TestFixture]
    public partial class AudioUtilsTests
    {

        [Test]
        public void FastAddAddsCorrectly()
        {
            Span<float> source = new float[32];
            Span<float> destination = new float[48];
            const int Value = 1;
            source.FastFill(Value);
            destination.FastFill(-1);

            AudioUtils.FastAdd(source, destination);
            for (var i = 0; i < source.Length; i++)
            {
                if (destination[i] != 0) Assert.Fail("The FastAdd doesn't add correctly!");
            }
            Assert.Pass();
        }

        [Test]
        public void FastScalarMultiplyScalesCorrectly()
        {
            Span<float> span = new float[32];
            span.FastFill(1);
            const float Value = MathF.PI;
            span.FastScalarMultiply(Value);
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] != Value) Assert.Fail("The FastScalarMultiply doesn't scale correctly!");
            }
            Assert.Pass();
        }

        [Test]
        public void FastMixMixesCorrectly()
        {
            Span<float> source = stackalloc float[32];
            Span<float> destination = stackalloc float[48];
            const int Value = 1;
            source.FastFill(Value);
            destination.FastFill(-1);

            AudioUtils.FastMix(source, destination, 2);
            for (var i = 0; i < source.Length; i++)
            {
                if (destination[i] != 1) Assert.Fail("The FastMix doesn't mix correctly!");
            }
            Assert.Pass();
        }

        #region ChannelsUtils

        internal static IEnumerable<int> SizeTestCaseGenerator() => new[] {
            //Small sizes
            1, 2, 3, 4, 5, 6, 7, 8,
            //2^n
            16, 32, 64,
            //partially odd
            96, 97,
            //tiny load test
            1024, 1048575, 1048576,
        };

        internal static void AssertArrayForInterleave(int[] b)
        {
            var q = new List<(int expected, int actual)>();
            for (var i = 0; i < b.Length; i++)
            {
                if (b[i] != i)
                    q.Add((i, b[i]));
            }
            Assert.IsEmpty(q, string.Join(", ", q.Select(a => $"({a.expected}, {a.actual})")));
        }
        internal static void PrepareDuplicate(int size, int channels, out int[] a, out int[] b)
        {
            a = new int[size];
            b = new int[a.Length * channels];
            for (var i = 0; i < a.Length; i++)
            {
                a[i] = i;
            }
        }
        internal static void PrepareStereo(int size, out int[] a0, out int[] a1, out int[] b)
        {
            a0 = new int[size];
            a1 = new int[a0.Length];
            b = new int[a0.Length * 2];
            for (var i = 0; i < a0.Length; i++)
            {
                a0[i] = i * 2;
            }
            for (var i = 0; i < a1.Length; i++)
            {
                a1[i] = i * 2 + 1;
            }
        }

        internal static void PrepareThree(int size, out int[] a0, out int[] a1, out int[] a2, out int[] b)
        {
            a0 = new int[size];
            a1 = new int[a0.Length];
            a2 = new int[a0.Length];
            b = new int[a0.Length * 3];
            for (var i = 0; i < a0.Length; i++)
            {
                a0[i] = i * 3;
            }
            for (var i = 0; i < a1.Length; i++)
            {
                a1[i] = i * 3 + 1;
            }
            for (var i = 0; i < a2.Length; i++)
            {
                a2[i] = i * 3 + 2;
            }
        }

        internal static void PrepareQuad(int size, out int[] a0, out int[] a1, out int[] a2, out int[] a3, out int[] b)
        {
            a0 = new int[size];
            a1 = new int[a0.Length];
            a2 = new int[a0.Length];
            a3 = new int[a0.Length];
            b = new int[a0.Length * 4];
            for (var i = 0; i < a0.Length; i++)
            {
                a0[i] = i * 4;
                a1[i] = i * 4 + 1;
                a2[i] = i * 4 + 2;
                a3[i] = i * 4 + 3;
            }
        }

        internal static void AssertArrayForDuplicate(int[] b, int channels)
        {
            var q = new List<(int expected, int actual)>();
            var j = 0;
            for (var i = 0; i < b.Length - channels + 1; i += channels, j++)
            {
                for (var ch = 0; ch < channels; ch++)
                {
                    if (b[i + ch] != j)
                    {
                        q.Add((i / channels, b[i]));
                    }
                }
            }
            Assert.IsEmpty(q, string.Join(", ", q.Select(a => $"({a.expected}, {a.actual})")));
        }
        #endregion

        #region Log2
        internal static void GenerateLog2TestArrays(int size, out float[] src, out float[] exp, out float[] act)
        {
            src = new float[size];
            exp = new float[size];
            act = new float[size];
            TestHelper.GenerateRandomRealNumbers(act);
            var u = 4.0f / exp.Length;
            for (var i = 0; i < exp.Length; i++)
            {
                var q = u * (i + 1);
                src[i] = q;
                exp[i] = MathF.Log2(q);
            }
        }
        #endregion
    }
}
