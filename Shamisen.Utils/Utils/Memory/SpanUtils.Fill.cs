using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils;

namespace Shamisen
{
    public static partial class SpanUtils
    {
        #region QuickFill
        /// <summary>
        /// Quickly (but sometimes slower than <see cref="FastFill(Span{float}, float)"/>) fills the specified memory region, with the given <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TSample">The type of the sample.</typeparam>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to fill with.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void QuickFill<TSample>(this Span<TSample> span, TSample value)
        {
            if (span.IsEmpty) return;
            switch (default(TSample))
            {
                case float _:
                    FillWithReference(Unsafe.As<TSample, float>(ref value), ref Unsafe.As<TSample, float>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
                case double _:
                    FillWithReference(Unsafe.As<TSample, double>(ref value), ref Unsafe.As<TSample, double>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
            }
            if (Vector.IsHardwareAccelerated)
            {
                if (Unsafe.SizeOf<TSample>() == 1)
                {
                    FillWithReferencePreloaded(new(Unsafe.As<TSample, byte>(ref value)), ref Unsafe.As<TSample, byte>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
                }
                else if (Unsafe.SizeOf<TSample>() == 2)
                {
                    FillWithReferencePreloaded(new(Unsafe.As<TSample, ushort>(ref value)), ref Unsafe.As<TSample, ushort>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
                }
                else if (Unsafe.SizeOf<TSample>() == 3)
                {
                    FillWithReference3BytesVectorized(ref Unsafe.As<TSample, byte>(ref value), ref Unsafe.As<TSample, byte>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
                }
                else if (Unsafe.SizeOf<TSample>() == 4)
                {
                    FillWithReferencePreloaded(new(Unsafe.As<TSample, uint>(ref value)), ref Unsafe.As<TSample, uint>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
                }
                else if (Unsafe.SizeOf<TSample>() == 5)
                {
                    FillWithReference5BytesVectorized(ref Unsafe.As<TSample, byte>(ref value), ref Unsafe.As<TSample, byte>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
                }
                else if (Unsafe.SizeOf<TSample>() == 8)
                {
                    FillWithReferencePreloaded(new(Unsafe.As<TSample, ulong>(ref value)), ref Unsafe.As<TSample, ulong>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
                }
                else if (Unsafe.SizeOf<TSample>() == 16)
                {
                    FillWithReferenceVector4(Unsafe.As<TSample, Vector4>(ref value), ref Unsafe.As<TSample, Vector4>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
                }
                else if (Unsafe.SizeOf<TSample>() is > 16 && Unsafe.SizeOf<TSample>() == Vector<byte>.Count)
                {
                    FillWithReferenceVectorFit(Unsafe.As<TSample, Vector<byte>>(ref value), ref Unsafe.As<TSample, Vector<byte>>(ref MemoryMarshal.GetReference(span)), span.Length);
                    return;
                }
                else
                {
                    FillWithReferenceNBytes(ref Unsafe.As<TSample, byte>(ref value), (nuint)Unsafe.SizeOf<TSample>(), ref Unsafe.As<TSample, byte>(ref MemoryMarshal.GetReference(span)), (nuint)span.Length * (nuint)Unsafe.SizeOf<TSample>());
                    return;
                }
            }
            span.Fill(value);
        }

        /// <summary>
        /// Quickly (but sometimes slower than <see cref="FastFill(NativeSpan{float}, float)"/>) fills the specified memory region, with the given <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TSample">The type of the sample.</typeparam>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to fill with.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void QuickFill<TSample>(this NativeSpan<TSample> span, TSample value) => span.Fill(value);

        private static void CopyFill<TSample>(Span<TSample> span, TSample value)
        {
            if (span.Length < 32)
            {
                span.Fill(value);
                return;
            }
            var filled = span.SliceWhile(16);
            var remaining = span[filled.Length..];
            filled.Fill(value);
            do
            {
                filled.CopyTo(remaining);
                remaining = remaining[filled.Length..];
                filled = span.SliceWhile(filled.Length << 1);
            } while (remaining.Length >= filled.Length);
            filled[^remaining.Length..].CopyTo(remaining);
        }

        #endregion QuickFill

        #region Standard VectorFill
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void VectorFill<T>(Span<T> dst, T value) where T : unmanaged
        {
            if (!Vector.IsHardwareAccelerated)
            {
                dst.Fill(value);
                return;
            }
            ref var rdi = ref MemoryMarshal.GetReference(dst);
            nint length = dst.Length;
            FillWithReference(value, ref rdi, length);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void VectorFill<T>(NativeSpan<T> dst, T value) where T : unmanaged
        {
            if (!Vector.IsHardwareAccelerated)
            {
                dst.Fill(value);
                return;
            }
            ref var rdi = ref GetReference(dst);
            var length = dst.Length;
            FillWithReference(value, ref rdi, length);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FillWithReference<T>(T value, ref T rdi, nint length) where T : unmanaged
        {
            nint i = 0;
            nint olen;
            if (length >= Vector<T>.Count && Vector.IsHardwareAccelerated)
            {
                var vv = new Vector<T>(value);
                olen = length - 8 * Vector<T>.Count + 1;
                ref var rdx = ref Unsafe.Add(ref rdi, 7 * Vector<T>.Count);
                for (; i < olen; i += 8 * Vector<T>.Count)
                {
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 0) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 1) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 2) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 3) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 4) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 5) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 6) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 7) * Vector<T>.Count)) = vv;
                }
                rdx = ref Unsafe.Add(ref rdi, Vector<T>.Count);
                olen = length - 2 * Vector<T>.Count + 1;
                for (; i < olen; i += 2 * Vector<T>.Count)
                {
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - 1 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - 0 * Vector<T>.Count)) = vv;
                }
                olen = length - Vector<T>.Count + 1;
                if (i < olen)
                {
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 0 * Vector<T>.Count)) = vv;
                }
                Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, length - 1 * Vector<T>.Count)) = vv;
            }
            else
            {
                for (; i < length; i++)
                {
                    Unsafe.Add(ref rdi, i) = value;
                }
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FillWithReferencePreloaded<T>(Vector<T> vv, ref T rdi, nint length) where T : unmanaged
        {
            nint i = 0;
            nint olen;
            if (length >= Vector<T>.Count)
            {
                olen = length - 8 * Vector<T>.Count + 1;
                ref var rdx = ref Unsafe.Add(ref rdi, 7 * Vector<T>.Count);
                for (; i < olen; i += 8 * Vector<T>.Count)
                {
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 0) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 1) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 2) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 3) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 4) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 5) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 6) * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - (7 - 7) * Vector<T>.Count)) = vv;
                }
                rdx = ref Unsafe.Add(ref rdi, Vector<T>.Count);
                olen = length - 2 * Vector<T>.Count + 1;
                for (; i < olen; i += 2 * Vector<T>.Count)
                {
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - 1 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdx, i - 0 * Vector<T>.Count)) = vv;
                }
                olen = length - Vector<T>.Count + 1;
                if (i < olen)
                {
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 0 * Vector<T>.Count)) = vv;
                }
                Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, length - 1 * Vector<T>.Count)) = vv;
            }
            else
            {
                var value = vv[0];
                for (; i < length; i++)
                {
                    Unsafe.Add(ref rdi, i) = value;
                }
            }
        }

        #endregion

        #region FastFill for Larger or NPOT sizes
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FillWithReferenceVectorFit(Vector<byte> value, ref Vector<byte> rdi, nint length)
        {
            nint i = 0;
            nint olen;
            var vv = value;
            olen = length - 8 + 1;
            ref var rdx = ref Unsafe.Add(ref rdi, 7);
            for (; i < olen; i += 8)
            {
                ref var r8 = ref Unsafe.Add(ref rdx, i);
                Unsafe.Subtract(ref r8, 7) = vv;
                Unsafe.Subtract(ref r8, 6) = vv;
                Unsafe.Subtract(ref r8, 5) = vv;
                Unsafe.Subtract(ref r8, 4) = vv;
                Unsafe.Subtract(ref r8, 3) = vv;
                Unsafe.Subtract(ref r8, 2) = vv;
                Unsafe.Subtract(ref r8, 1) = vv;
                Unsafe.Subtract(ref r8, 0) = vv;
            }
            rdx = ref Unsafe.Add(ref rdi, 1);
            olen = length - 2 + 1;
            for (; i < olen; i += 2)
            {
                ref var r8 = ref Unsafe.Add(ref rdx, i);
                Unsafe.Subtract(ref r8, 1) = vv;
                Unsafe.Subtract(ref r8, 0) = vv;
            }
            if (i < length)
            {
                Unsafe.Add(ref rdi, i) = vv;
            }
        }

        #region Vector128
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FillWithReferenceVector4(Vector4 value, ref Vector4 rdi, nint length)
        {
            if (Vector256.IsHardwareAccelerated)
            {
                FillWithReferenceVector4V256(value, ref rdi, length);
                return;
            }
            FillWithReferenceVector4V128(value, ref rdi, length);
        }

        private static void FillWithReferenceVector4V256(Vector4 value, ref Vector4 x9, nint length)
        {
            nint i = 0, j = 0;
            nint olen;
            var vv128 = value.AsVector128();
            var vv = Vector256.Create(vv128, vv128).AsByte();
            var hlen = length >> 1;
            olen = hlen - 8 + 1;
            ref var rdi = ref Unsafe.As<Vector4, Vector256<byte>>(ref x9);
            ref var rdx = ref Unsafe.Add(ref rdi, 7);
            for (; i < olen; i += 8, j += 8 * Vector256<byte>.Count)
            {
                Unsafe.AddByteOffset(ref rdx, j - 7 * Vector256<byte>.Count) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 6 * Vector256<byte>.Count) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 5 * Vector256<byte>.Count) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 4 * Vector256<byte>.Count) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 3 * Vector256<byte>.Count) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 2 * Vector256<byte>.Count) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 1 * Vector256<byte>.Count) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 0 * Vector256<byte>.Count) = vv;
            }
            rdx = ref Unsafe.Add(ref rdi, 1);
            olen = hlen - 2 + 1;
            for (; i < olen; i += 2, j += 2 * Vector256<byte>.Count)
            {
                Unsafe.AddByteOffset(ref rdx, j - 1 * Vector256<byte>.Count) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 0 * Vector256<byte>.Count) = vv;
            }
            if (i < hlen)
            {
                Unsafe.AddByteOffset(ref rdi, j) = vv;
            }
            if ((length & 1) > 0)
            {
                Unsafe.Add(ref x9, i * 2 + 1) = vv128.AsVector4();
            }
        }

        private static void FillWithReferenceVector4V128(Vector4 value, ref Vector4 rdi, nint length)
        {
            nint i = 0, j = 0;
            nint olen;
            var vv = value;
            olen = length - 8 + 1;
            ref var rdx = ref Unsafe.Add(ref rdi, 7);
            for (; i < olen; i += 8, j += 8 * Unsafe.SizeOf<Vector4>())
            {
                Unsafe.AddByteOffset(ref rdx, j - 7 * Unsafe.SizeOf<Vector4>()) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 6 * Unsafe.SizeOf<Vector4>()) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 5 * Unsafe.SizeOf<Vector4>()) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 4 * Unsafe.SizeOf<Vector4>()) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 3 * Unsafe.SizeOf<Vector4>()) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 2 * Unsafe.SizeOf<Vector4>()) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 1 * Unsafe.SizeOf<Vector4>()) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 0 * Unsafe.SizeOf<Vector4>()) = vv;
            }
            rdx = ref Unsafe.Add(ref rdi, 1);
            olen = length - 2 + 1;
            for (; i < olen; i += 2, j += 2 * Unsafe.SizeOf<Vector4>())
            {
                Unsafe.AddByteOffset(ref rdx, j - 1 * Unsafe.SizeOf<Vector4>()) = vv;
                Unsafe.AddByteOffset(ref rdx, j - 0 * Unsafe.SizeOf<Vector4>()) = vv;
            }
            if (i < length)
            {
                Unsafe.AddByteOffset(ref rdi, j) = vv;
            }
        }

        #endregion

        #region UInt24
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FillWithReference3BytesVectorized(ref byte value, ref byte dst, nint u24length)
        {
            if (Vector<byte>.Count == Vector256<byte>.Count && Avx2.IsSupported)
            {
                FillWithReference3BytesAvx2(ref value, ref dst, u24length);
                return;
            }
            if (Vector<byte>.Count == Vector128<byte>.Count && Vector128.IsHardwareAccelerated)
            {
                FillWithReference3BytesV128(ref value, ref dst, u24length);
                return;
            }
            FillWithReferenceNBytes(ref value, 3, ref dst, (nuint)u24length * 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillWithReference3BytesV128(ref byte value, ref byte dst, nint u24length)
        {
            nint i = 0, length = u24length * 3;
            var v0_16b = Vector128.Create((byte)0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0);
            var v1_16b = Vector128.Create((byte)1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1);
            var v2_16b = Vector128.Create((byte)2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2);
            var v3_16b = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ushort>(ref value)).AsByte();
            v3_16b = v3_16b.WithElement(2, Unsafe.Add(ref value, 2));
            v0_16b = Vector128.Shuffle(v3_16b, v0_16b);
            v1_16b = Vector128.Shuffle(v3_16b, v1_16b);
            v2_16b = Vector128.Shuffle(v3_16b, v2_16b);
            var v0_ns = v0_16b;
            var v1_ns = v1_16b;
            var v2_ns = v2_16b;
            var v3_ns = v0_ns;
            var v4_ns = v1_ns;
            var olen = length - 8 * Vector128<byte>.Count + 1;
            for (; i < olen; i += 8 * Vector128<byte>.Count)
            {
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector128<byte>.Count)) = v0_ns;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector128<byte>.Count)) = v1_ns;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 2 * Vector128<byte>.Count)) = v2_ns;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 3 * Vector128<byte>.Count)) = v0_ns;
                v0_ns = v2_ns;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 4 * Vector128<byte>.Count)) = v1_ns;
                v1_ns = v3_ns;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 5 * Vector128<byte>.Count)) = v2_ns;
                v2_ns = v4_ns;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 6 * Vector128<byte>.Count)) = v3_ns;
                v3_ns = v0_ns;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 7 * Vector128<byte>.Count)) = v4_ns;
                v4_ns = v1_ns;
            }
            olen = length - 2 * Vector128<byte>.Count + 1;
            for (; i < olen; i += 2 * Vector128<byte>.Count)
            {
                v0_ns = v2_ns;
                v1_ns = v3_ns;
                v2_ns = v4_ns;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector128<byte>.Count)) = v3_ns;
                v3_ns = v0_ns;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector128<byte>.Count)) = v4_ns;
                v4_ns = v1_ns;
            }
            olen = length - Vector128<byte>.Count + 1;
            if (i < olen)
            {
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i)) = v3_ns;
                i += Vector128<byte>.Count;
            }
            var remaining = length - i;
            if (remaining == 0) return;
            var t = (nuint)i % 3;
            for (; i < length; i++)
            {
                Unsafe.Add(ref dst, i) = Unsafe.Add(ref value, t);
                var u = ++t - 3;
                if (u < t)
                {
                    t = u;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillWithReference3BytesAvx2(ref byte value, ref byte dst, nint u24length)
        {
            nint i = 0, length = u24length * 3;
            var ymm0 = Vector256.Create((byte)0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1);
            var ymm1 = Vector256.Create((byte)2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0);
            var ymm2 = Vector256.Create((byte)1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2);
            var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ushort>(ref value)).AsByte();
            xmm3 = xmm3.WithElement(2, Unsafe.Add(ref value, 2));
            var ymm3 = Vector256.Create(xmm3, xmm3);
            ymm0 = Avx2.Shuffle(ymm3, ymm0);
            ymm1 = Avx2.Shuffle(ymm3, ymm1);
            ymm2 = Avx2.Shuffle(ymm3, ymm2);
            ymm3 = ymm0;
            var ymm4 = ymm1;
            var olen = length - 8 * Vector256<byte>.Count + 1;
            for (; i < olen; i += 8 * Vector256<byte>.Count)
            {
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<byte>.Count)) = ymm0;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector256<byte>.Count)) = ymm1;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 2 * Vector256<byte>.Count)) = ymm2;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 3 * Vector256<byte>.Count)) = ymm0;
                ymm0 = ymm2;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 4 * Vector256<byte>.Count)) = ymm1;
                ymm1 = ymm3;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 5 * Vector256<byte>.Count)) = ymm2;
                ymm2 = ymm4;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 6 * Vector256<byte>.Count)) = ymm3;
                ymm3 = ymm0;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 7 * Vector256<byte>.Count)) = ymm4;
                ymm4 = ymm1;
            }
            olen = length - 2 * Vector256<byte>.Count + 1;
            for (; i < olen; i += 2 * Vector256<byte>.Count)
            {
                ymm0 = ymm2;
                ymm1 = ymm3;
                ymm2 = ymm4;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<byte>.Count)) = ymm3;
                ymm3 = ymm0;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector256<byte>.Count)) = ymm4;
                ymm4 = ymm1;
            }
            olen = length - Vector256<byte>.Count + 1;
            if (i < olen)
            {
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i)) = ymm3;
                i += Vector256<byte>.Count;
            }
            var remaining = length - i;
            if (remaining == 0) return;
            var t = (nuint)i % 3;
            for (; i < length; i++)
            {
                Unsafe.Add(ref dst, i) = Unsafe.Add(ref value, t);
                var u = ++t - 3;
                if (u < t)
                {
                    t = u;
                }
            }
        }
        #endregion

        #region 5Bytes
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FillWithReference5BytesVectorized(ref byte value, ref byte dst, nint b5length)
        {
            if (Vector<byte>.Count == Vector256<byte>.Count && Avx2.IsSupported)
            {
                FillWithReference5BytesAvx2(ref value, ref dst, b5length);
                return;
            }
            if (Vector<byte>.Count == Vector128<byte>.Count && Vector128.IsHardwareAccelerated)
            {
                FillWithReference5BytesV128(ref value, ref dst, b5length);
                return;
            }
            FillWithReferenceNBytes(ref value, 5, ref dst, (nuint)b5length * 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillWithReference5BytesV128(ref byte value, ref byte dst, nint b5length)
        {
            nint i = 0, length = b5length * 5;
            var v0_16b = Vector128.Create((byte)0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0);
            var v1_16b = Vector128.Create((byte)1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1);
            var v2_16b = Vector128.Create((byte)2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2);
            var v3_16b = Vector128.Create((byte)3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3);
            var v4_16b = Vector128.Create((byte)4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4);
            var v5_16b = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref value)).AsByte();
            v5_16b = v5_16b.WithElement(4, Unsafe.Add(ref value, 4));
            v0_16b = Vector128.Shuffle(v5_16b, v0_16b);
            v1_16b = Vector128.Shuffle(v5_16b, v1_16b);
            v2_16b = Vector128.Shuffle(v5_16b, v2_16b);
            v3_16b = Vector128.Shuffle(v5_16b, v3_16b);
            v4_16b = Vector128.Shuffle(v5_16b, v4_16b);
            v5_16b = v0_16b;
            var v6_16b = v1_16b;
            var v7_16b = v2_16b;
            var olen = length - 8 * Vector128<byte>.Count + 1;
            for (; i < olen; i += 8 * Vector128<byte>.Count)
            {
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector128<byte>.Count)) = v0_16b;
                v0_16b = v3_16b;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector128<byte>.Count)) = v1_16b;
                v1_16b = v4_16b;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 2 * Vector128<byte>.Count)) = v2_16b;
                v2_16b = v5_16b;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 3 * Vector128<byte>.Count)) = v3_16b;
                v3_16b = v6_16b;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 4 * Vector128<byte>.Count)) = v4_16b;
                v4_16b = v7_16b;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 5 * Vector128<byte>.Count)) = v5_16b;
                v5_16b = v0_16b;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 6 * Vector128<byte>.Count)) = v6_16b;
                v6_16b = v1_16b;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 7 * Vector128<byte>.Count)) = v7_16b;
                v7_16b = v2_16b;
            }
            olen = length - 2 * Vector128<byte>.Count + 1;
            for (; i < olen; i += 2 * Vector128<byte>.Count)
            {
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector128<byte>.Count)) = v0_16b;
                v0_16b = v2_16b;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector128<byte>.Count)) = v1_16b;
                v1_16b = v3_16b;
                v2_16b = v4_16b;
                v3_16b = v5_16b;
                v4_16b = v6_16b;
                v5_16b = v7_16b;
                v6_16b = v0_16b;
                v7_16b = v1_16b;
            }
            olen = length - Vector128<byte>.Count + 1;
            if (i < olen)
            {
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref dst, i)) = v5_16b;
                i += Vector128<byte>.Count;
            }
            var remaining = length - i;
            if (remaining == 0) return;
            var t = (nuint)i % 5;
            for (; i < length; i++)
            {
                Unsafe.Add(ref dst, i) = Unsafe.Add(ref value, t);
                var u = ++t - 5;
                if (u < t)
                {
                    t = u;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillWithReference5BytesAvx2(ref byte value, ref byte dst, nint b5length)
        {
            nint i = 0, length = b5length * 5;
            var ymm0 = Vector256.Create((byte)0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1);
            var ymm1 = Vector256.Create((byte)2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3);
            var ymm2 = Vector256.Create((byte)4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0);
            var ymm3 = Vector256.Create((byte)1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2);
            var ymm4 = Vector256.Create((byte)3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4);
            var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref value)).AsByte();
            xmm5 = xmm5.WithElement(4, Unsafe.Add(ref value, 4));
            var ymm5 = Vector256.Create(xmm5, xmm5);
            ymm0 = Avx2.Shuffle(ymm5, ymm0);
            ymm1 = Avx2.Shuffle(ymm5, ymm1);
            ymm2 = Avx2.Shuffle(ymm5, ymm2);
            ymm3 = Avx2.Shuffle(ymm5, ymm3);
            ymm4 = Avx2.Shuffle(ymm5, ymm4);
            ymm5 = ymm0;
            var ymm6 = ymm1;
            var ymm7 = ymm2;
            var olen = length - 8 * Vector256<byte>.Count + 1;
            for (; i < olen; i += 8 * Vector256<byte>.Count)
            {
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<byte>.Count)) = ymm0;
                ymm0 = ymm3;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector256<byte>.Count)) = ymm1;
                ymm1 = ymm4;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 2 * Vector256<byte>.Count)) = ymm2;
                ymm2 = ymm5;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 3 * Vector256<byte>.Count)) = ymm3;
                ymm3 = ymm6;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 4 * Vector256<byte>.Count)) = ymm4;
                ymm4 = ymm7;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 5 * Vector256<byte>.Count)) = ymm5;
                ymm5 = ymm0;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 6 * Vector256<byte>.Count)) = ymm6;
                ymm6 = ymm1;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 7 * Vector256<byte>.Count)) = ymm7;
                ymm7 = ymm2;
            }
            olen = length - 2 * Vector256<byte>.Count + 1;
            for (; i < olen; i += 2 * Vector256<byte>.Count)
            {
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<byte>.Count)) = ymm0;
                ymm0 = ymm2;
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector256<byte>.Count)) = ymm1;
                ymm1 = ymm3;
                ymm2 = ymm4;
                ymm3 = ymm5;
                ymm4 = ymm6;
                ymm5 = ymm7;
                ymm6 = ymm0;
                ymm7 = ymm1;
            }
            olen = length - Vector256<byte>.Count + 1;
            if (i < olen)
            {
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i)) = ymm5;
                i += Vector256<byte>.Count;
            }
            var remaining = length - i;
            if (remaining == 0) return;
            var t = (nuint)i % 5;
            for (; i < length; i++)
            {
                Unsafe.Add(ref dst, i) = Unsafe.Add(ref value, t);
                var u = ++t - 5;
                if (u < t)
                {
                    t = u;
                }
            }
        }
        #endregion

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FillWithReferenceNBytes(ref byte value, nuint valueLength, ref byte dst, nuint length)
        {
            nuint i = 0;
            if (length <= valueLength)
            {
                UnsafeUtils.MoveMemory(ref dst, ref value, length);
                return;
            }
            UnsafeUtils.MoveMemory(ref dst, ref value, valueLength);
            i += valueLength;
            while (i < length - i)
            {
                UnsafeUtils.CopyFromHead(ref Unsafe.Add(ref dst, i), ref dst, i);
                i += i;
            }
            UnsafeUtils.CopyFromHead(ref Unsafe.Add(ref dst, i), ref dst, length - i);
        }
        #endregion
    }
}
