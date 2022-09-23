using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Utils.Tests
{
    [TestFixture]
    public class UnsafeUtilsTest
    {
        private static IEnumerable<int> LengthValues
        {
            get
            {
                for (var i = 1; i < 6; i++)
                {
                    yield return i * 2 - 1;
                }
                yield return sizeof(ulong) * 4;
                yield return sizeof(ulong) * 4 + 1;
                yield return Vector<byte>.Count * 4;
                yield return Vector<byte>.Count * 4 + 1;
                yield return Vector<byte>.Count * 16;
                yield return Vector<byte>.Count * 16 + 1;
                yield return Vector<byte>.Count * 20 + sizeof(ulong) * 6 - 1;
            }
        }

        private static IEnumerable<int> OffsetValues
        {
            get
            {
                for (var i = 1; i < 6; i++)
                {
                    yield return i * 2 - 1;
                }
            }
        }

        private static IEnumerable<TestCaseData> LengthTestCaseSource => LengthValues.Select(a => new TestCaseData(a));
        #region NoOverlap
        private static void PrepareByteArraysNoOverlap(int size, out PooledArray<byte> dst, out PooledArray<byte> exp, out PooledArray<byte> src, out Span<byte> sD, out Span<byte> sE, out Span<byte> sS, out Span<byte> sDstActual)
        {
            var guard = Vector<byte>.Count * 20 + sizeof(ulong) * 6 - 1;
            var dstSize = size + 2 * guard;
            dst = new PooledArray<byte>(dstSize);
            exp = new PooledArray<byte>(dstSize);
            src = new PooledArray<byte>(size);
            sD = dst.Span;
            sE = exp.Span;
            sS = src.Span;
            RandomNumberGenerator.Fill(sE);
            sE.CopyTo(sD);
            RandomNumberGenerator.Fill(sS);
            sS.CopyTo(sE.Slice(guard));
            sDstActual = sD.Slice(guard);
        }

        [TestCaseSource(nameof(LengthTestCaseSource))]
        public void CopyFromHeadCopiesBytesCorrectlyNoOverlap(int size)
        {
            PrepareByteArraysNoOverlap(size, out var dst, out var exp, out var src, out var sD, out var sE, out var sS, out var sDstActual);
            UnsafeUtils.CopyFromHead(ref MemoryMarshal.GetReference(sDstActual), ref MemoryMarshal.GetReference(sS), (nuint)sS.Length);
            Assert.That(sD.ToArray(), Is.EqualTo(sE.ToArray()));
            dst.Dispose();
            exp.Dispose();
            src.Dispose();
        }

        [TestCaseSource(nameof(LengthTestCaseSource))]
        public void CopyFromTailCopiesBytesCorrectlyNoOverlap(int size)
        {
            PrepareByteArraysNoOverlap(size, out var dst, out var exp, out var src, out var sD, out var sE, out var sS, out var sDstActual);
            UnsafeUtils.CopyFromTail(ref MemoryMarshal.GetReference(sDstActual), ref MemoryMarshal.GetReference(sS), (nuint)sS.Length);
            Assert.That(sD.ToArray(), Is.EqualTo(sE.ToArray()));
            dst.Dispose();
            exp.Dispose();
            src.Dispose();
        }
        #endregion
        private static IEnumerable<TestCaseData> LengthAndOffsetTestCaseSource => OffsetValues.Select(b => new TestCaseData(Vector<byte>.Count * 20 + sizeof(ulong) * 6 - 1, b));
        #region Overlapped
        [TestCaseSource(nameof(LengthAndOffsetTestCaseSource))]
        public void CopyFromHeadCopiesBytesCorrectlyOverlapped(int size, int offset)
        {
            var guard = Vector<byte>.Count * 20 + sizeof(ulong) * 6 - 1;
            var dstSize = size + 2 * guard + offset;
            var dst = new PooledArray<byte>(dstSize);
            var exp = new PooledArray<byte>(dstSize);
            var sD = dst.Span;
            var sE = exp.Span;
            RandomNumberGenerator.Fill(sE);
            sE.CopyTo(sD);
            sE.Slice(guard + offset, size).CopyTo(sE.Slice(guard));
            var sDstActual = sD.Slice(guard);
            var sSrcActual = sD.Slice(guard + offset, size);
            UnsafeUtils.CopyFromHead(ref MemoryMarshal.GetReference(sDstActual), ref MemoryMarshal.GetReference(sSrcActual), (nuint)size);
            Assert.That(sD.ToArray(), Is.EqualTo(sE.ToArray()));
            dst.Dispose();
            exp.Dispose();
        }

        [TestCaseSource(nameof(LengthAndOffsetTestCaseSource))]
        public void CopyFromTailCopiesBytesCorrectlyOverlapped(int size, int offset)
        {
            var guard = Vector<byte>.Count * 20 + sizeof(ulong) * 6 - 1;
            var dstSize = size + 2 * guard + offset;
            var dst = new PooledArray<byte>(dstSize);
            var exp = new PooledArray<byte>(dstSize);
            var sD = dst.Span;
            var sE = exp.Span;
            RandomNumberGenerator.Fill(sE);
            sE.CopyTo(sD);
            sE.Slice(guard, size).CopyTo(sE.Slice(guard + offset));
            var sDstActual = sD.Slice(guard + offset);
            var sSrcActual = sD.Slice(guard, size);
            UnsafeUtils.CopyFromTail(ref MemoryMarshal.GetReference(sDstActual), ref MemoryMarshal.GetReference(sSrcActual), (nuint)size);
            Assert.That(sD.ToArray(), Is.EqualTo(sE.ToArray()));
            dst.Dispose();
            exp.Dispose();
        }

        [TestCaseSource(nameof(LengthAndOffsetTestCaseSource))]
        public void MoveMemoryCopiesBytesCorrectlyOverlappedPositive(int size, int offset)
        {
            var guard = Vector<byte>.Count * 20 + sizeof(ulong) * 6 - 1;
            var dstSize = size + 2 * guard + offset;
            var dst = new PooledArray<byte>(dstSize);
            var exp = new PooledArray<byte>(dstSize);
            var sD = dst.Span;
            var sE = exp.Span;
            RandomNumberGenerator.Fill(sE);
            sE.CopyTo(sD);
            sE.Slice(guard + offset, size).CopyTo(sE.Slice(guard));
            var sDstActual = sD.Slice(guard);
            var sSrcActual = sD.Slice(guard + offset, size);
            UnsafeUtils.MoveMemory(ref MemoryMarshal.GetReference(sDstActual), ref MemoryMarshal.GetReference(sSrcActual), (nuint)size);
            Assert.That(sD.ToArray(), Is.EqualTo(sE.ToArray()));
            dst.Dispose();
            exp.Dispose();
        }

        [TestCaseSource(nameof(LengthAndOffsetTestCaseSource))]
        public void MoveMemoryCopiesBytesCorrectlyOverlappedNegative(int size, int offset)
        {
            var guard = Vector<byte>.Count * 20 + sizeof(ulong) * 6 - 1;
            var dstSize = size + 2 * guard + offset;
            var dst = new PooledArray<byte>(dstSize);
            var exp = new PooledArray<byte>(dstSize);
            var sD = dst.Span;
            var sE = exp.Span;
            RandomNumberGenerator.Fill(sE);
            sE.CopyTo(sD);
            sE.Slice(guard, size).CopyTo(sE.Slice(guard + offset));
            var sDstActual = sD.Slice(guard + offset);
            var sSrcActual = sD.Slice(guard, size);
            UnsafeUtils.MoveMemory(ref MemoryMarshal.GetReference(sDstActual), ref MemoryMarshal.GetReference(sSrcActual), (nuint)size);
            Assert.That(sD.ToArray(), Is.EqualTo(sE.ToArray()));
            dst.Dispose();
            exp.Dispose();
        }
        #endregion

    }
}
