using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

namespace Shamisen.Utils.Tests
{
    [TestFixture]
    public class VectorUtilsTests
    {
        private static IEnumerable<TestCaseData> LoadTailTestCaseSource(int size)
        {
            List<int> sizes = [0, 1, size / 2, size, size * 2, 65536];
            foreach (var elements in sizes)
            {
                for (int offset = int.Max(0, elements - size); offset <= elements + 1; offset++)
                {
                    yield return new TestCaseData(elements, offset);
                }
            }
        }
        private static IEnumerable<TestCaseData> LoadTail8TestCaseSource() => LoadTailTestCaseSource(Vector128<ushort>.Count);

        private static IEnumerable<TestCaseData> LoadTail16TestCaseSource() => LoadTailTestCaseSource(Vector128<byte>.Count);

        private static IEnumerable<TestCaseData> LoadTail32TestCaseSource() => LoadTailTestCaseSource(Vector256<byte>.Count);

        private static IEnumerable<TestCaseData> LoadTail64TestCaseSource() => LoadTailTestCaseSource(Vector512<byte>.Count);

        [TestCaseSource(nameof(LoadTail16TestCaseSource))]
        public void LoadTail128FallbackLoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (byte)a).ToArray();
            var arr2 = Vector128<byte>.Zero;
            var s2 = MemoryMarshal.AsBytes(new Span<Vector128<byte>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector128.Create(s2);
            var vec = VectorUtils.LoadTail128Fallback(offset, arr.AsNativeSpan().AsReadOnlyNativeSpan());
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail16TestCaseSource))]
        public void LoadTail128Ssse3LoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (byte)a).ToArray();
            var arr2 = Vector128<byte>.Zero;
            var s2 = MemoryMarshal.AsBytes(new Span<Vector128<byte>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector128.Create(s2);
            var vec = VectorUtils.LoadTail128Ssse3(offset, arr);
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail16TestCaseSource))]
        public void LoadTail128Avx512BWLoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (byte)a).ToArray();
            var arr2 = Vector128<byte>.Zero;
            var s2 = MemoryMarshal.AsBytes(new Span<Vector128<byte>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector128.Create(s2);
            var vec = VectorUtils.LoadTail128Avx512BW(offset, arr);
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail8TestCaseSource))]
        public void LoadTail128Avx512VLLoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (uint)a).ToArray();
            var arr2 = Vector128<uint>.Zero;
            var s2 = MemoryMarshal.Cast<Vector128<uint>, uint>(new Span<Vector128<uint>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector128.Create(s2);
            var vec = VectorUtils.LoadTail128Avx512VL(offset, arr);
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail32TestCaseSource))]
        public void LoadTail256FallbackLoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (byte)a).ToArray();
            var arr2 = Vector256<byte>.Zero;
            var s2 = MemoryMarshal.AsBytes(new Span<Vector256<byte>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector256.Create(s2);
            var vec = VectorUtils.LoadTail256Fallback(offset, arr.AsNativeSpan().AsReadOnlyNativeSpan());
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail32TestCaseSource))]
        public void LoadTail256ByteAvx2LoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (byte)a).ToArray();
            var arr2 = Vector256<byte>.Zero;
            var s2 = MemoryMarshal.AsBytes(new Span<Vector256<byte>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector256.Create(s2);
            var vec = VectorUtils.LoadTail256ByteAvx2(offset, arr);
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail32TestCaseSource))]
        public void LoadTail256Avx512BWLoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (byte)a).ToArray();
            var arr2 = Vector256<byte>.Zero;
            var s2 = MemoryMarshal.AsBytes(new Span<Vector256<byte>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector256.Create(s2);
            var vec = VectorUtils.LoadTail256Avx512BW(offset, arr);
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail16TestCaseSource))]
        public void LoadTail256Avx512VLLoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (uint)a).ToArray();
            var arr2 = Vector256<uint>.Zero;
            var s2 = MemoryMarshal.Cast<Vector256<uint>, uint>(new Span<Vector256<uint>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector256.Create(s2);
            var vec = VectorUtils.LoadTail256Avx512VL(offset, arr);
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail64TestCaseSource))]
        public void LoadTail512FallbackLoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (byte)a).ToArray();
            var arr2 = Vector512<byte>.Zero;
            var s2 = MemoryMarshal.AsBytes(new Span<Vector512<byte>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector512.Create(s2);
            var vec = VectorUtils.LoadTail512Fallback(offset, arr.AsNativeSpan().AsReadOnlyNativeSpan());
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail64TestCaseSource))]
        public void LoadTail512Avx512BWLoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (byte)a).ToArray();
            var arr2 = Vector512<byte>.Zero;
            var s2 = MemoryMarshal.AsBytes(new Span<Vector512<byte>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector512.Create(s2);
            var vec = VectorUtils.LoadTail512Avx512BW(offset, arr);
            Assert.That(vec, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(LoadTail32TestCaseSource))]
        public void LoadTail512Avx512FLoadsCorrectly(int elements, int offset)
        {
            var arr = Enumerable.Range(-offset, elements).Select(a => (uint)a).ToArray();
            var arr2 = Vector512<uint>.Zero;
            var s2 = MemoryMarshal.Cast<Vector512<uint>, uint>(new Span<Vector512<uint>>(ref arr2));
            if (offset <= arr.Length)
            {
                arr.AsSpan(offset).SliceWhileIfLongerThan(s2.Length).CopyTo(s2);
            }
            var expected = Vector512.Create(s2);
            var vec = VectorUtils.LoadTail512Avx512F(offset, arr);
            Assert.That(vec, Is.EqualTo(expected));
        }
    }
}
