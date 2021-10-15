using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Shamisen.Core.Tests.CoreFx.AudioUtilsTest
{
    public partial class AudioUtilsTests
    {
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
            for (int i = 0; i < b.Length; i++)
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
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = i;
            }
        }
        internal static void PrepareStereo(int size, out int[] a0, out int[] a1, out int[] b)
        {
            a0 = new int[size];
            a1 = new int[a0.Length];
            b = new int[a0.Length * 2];
            for (int i = 0; i < a0.Length; i++)
            {
                a0[i] = i * 2;
            }
            for (int i = 0; i < a1.Length; i++)
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
            for (int i = 0; i < a0.Length; i++)
            {
                a0[i] = i * 3;
            }
            for (int i = 0; i < a1.Length; i++)
            {
                a1[i] = i * 3 + 1;
            }
            for (int i = 0; i < a2.Length; i++)
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
            for (int i = 0; i < a0.Length; i++)
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
            int j = 0;
            for (int i = 0; i < b.Length - channels + 1; i += channels, j++)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    if (b[i + ch] != j)
                    {
                        q.Add((i / channels, b[i]));
                    }
                }
            }
            Assert.IsEmpty(q, string.Join(", ", q.Select(a => $"({a.expected}, {a.actual})")));
        }
    }
}
