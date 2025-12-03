using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Shamisen.Codecs.Flac
{
    public static partial class FlacUtils
    {
        /// <summary>
        /// Shifts the values in specified <paramref name="span"/> left with specified <paramref name="shift"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <param name="shift">The shift.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void ShiftLeftLogical(Span<int> span, int shift)
        {
            if ((shift & 31) == 0 || span.IsEmpty) return;
            if (ShiftLeftLogicalArm(span, shift)) return;
            if (ShiftLeftLogicalX86(span, shift)) return;
            //fallback
            if (ShiftLeftLogicalStandard(shift, span)) return;
            ShiftLeftLogicalSimple(shift, span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static bool ShiftLeftLogicalStandard(int shift, Span<int> span)
        {
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nint length = span.Length;
                var vlen = length - 4 + 1;
                nint i;
                for (i = 0; i < vlen; i += 4)
                {
                    var v0 = Unsafe.Add(ref head, i + 0);
                    var v1 = Unsafe.Add(ref head, i + 1);
                    var v2 = Unsafe.Add(ref head, i + 2);
                    var v3 = Unsafe.Add(ref head, i + 3);
                    v0 <<= shift;
                    v1 <<= shift;
                    v2 <<= shift;
                    v3 <<= shift;
                    Unsafe.Add(ref head, i + 0) = v0;
                    Unsafe.Add(ref head, i + 1) = v1;
                    Unsafe.Add(ref head, i + 2) = v2;
                    Unsafe.Add(ref head, i + 3) = v3;
                }
                for (; i < length; i++)
                {
                    var value = Unsafe.Add(ref head, i);
                    value <<= shift;
                    Unsafe.Add(ref head, i) = value;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void ShiftLeftLogicalSimple(int shift, Span<int> span)
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = span[i] << shift;
            }
        }

#if NET5_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static bool ShiftLeftLogicalArm(Span<int> span, int shift)
        {
            if (!AdvSimd.IsSupported) return false;
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nuint length = (uint)span.Length;
                var v15_4s = Vector128.Create(shift);
                nuint i = 0;
                var olen = length - 8 * (uint)Vector128<int>.Count + 1;
                if (olen < length)
                {
                    for (; i < olen; i += 8 * (uint)Vector128<int>.Count)
                    {
                        var v0_4s = Vector128.LoadUnsafe(ref head, i + 0 * (uint)Vector128<int>.Count);
                        var v1_4s = Vector128.LoadUnsafe(ref head, i + 1 * (uint)Vector128<int>.Count);
                        var v2_4s = Vector128.LoadUnsafe(ref head, i + 2 * (uint)Vector128<int>.Count);
                        var v3_4s = Vector128.LoadUnsafe(ref head, i + 3 * (uint)Vector128<int>.Count);
                        v0_4s = AdvSimd.ShiftLogical(v0_4s, v15_4s);
                        v1_4s = AdvSimd.ShiftLogical(v1_4s, v15_4s);
                        v2_4s = AdvSimd.ShiftLogical(v2_4s, v15_4s);
                        v3_4s = AdvSimd.ShiftLogical(v3_4s, v15_4s);
                        v0_4s.StoreUnsafe(ref head, i + 0 * (uint)Vector128<int>.Count);
                        v0_4s = Vector128.LoadUnsafe(ref head, i + 4 * (uint)Vector128<int>.Count);
                        v1_4s.StoreUnsafe(ref head, i + 1 * (uint)Vector128<int>.Count);
                        v1_4s = Vector128.LoadUnsafe(ref head, i + 5 * (uint)Vector128<int>.Count);
                        v2_4s.StoreUnsafe(ref head, i + 2 * (uint)Vector128<int>.Count);
                        v2_4s = Vector128.LoadUnsafe(ref head, i + 6 * (uint)Vector128<int>.Count);
                        v3_4s.StoreUnsafe(ref head, i + 3 * (uint)Vector128<int>.Count);
                        v3_4s = Vector128.LoadUnsafe(ref head, i + 7 * (uint)Vector128<int>.Count);
                        v0_4s = AdvSimd.ShiftLogical(v0_4s, v15_4s);
                        v1_4s = AdvSimd.ShiftLogical(v1_4s, v15_4s);
                        v2_4s = AdvSimd.ShiftLogical(v2_4s, v15_4s);
                        v3_4s = AdvSimd.ShiftLogical(v3_4s, v15_4s);
                        v0_4s.StoreUnsafe(ref head, i + 4 * (uint)Vector128<int>.Count);
                        v1_4s.StoreUnsafe(ref head, i + 5 * (uint)Vector128<int>.Count);
                        v2_4s.StoreUnsafe(ref head, i + 6 * (uint)Vector128<int>.Count);
                        v3_4s.StoreUnsafe(ref head, i + 7 * (uint)Vector128<int>.Count);
                    }
                }
                olen = length - 1 * (uint)Vector128<int>.Count + 1;
                if (olen < length)
                {
                    for (; i < olen; i += (uint)Vector128<int>.Count)
                    {
                        var v0_4s = Vector128.LoadUnsafe(ref head, i);
                        v0_4s = AdvSimd.ShiftLogical(v0_4s, v15_4s);
                        v0_4s.StoreUnsafe(ref head, i);
                    }
                }
                for (; i < length; i++)
                {
                    var r15d = Unsafe.Add(ref head, i);
                    r15d <<= shift;
                    Unsafe.Add(ref head, i) = r15d;
                }
            }
            return true;
        }

#endif
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static bool ShiftLeftLogicalX86(Span<int> span, int shift)
        {
            if (Avx2.IsSupported)
            {
                ShiftLeftLogicalAvx2(span, shift);
                return true;
            }
            if (Sse2.IsSupported)
            {
                ShiftLeftLogicalSse2(span, shift);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void ShiftLeftLogicalAvx2(Span<int> span, int shift)
        {
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nuint length = (uint)span.Length;
                var ymm15 = Vector256.Create((uint)shift);
                nuint i = 0;
                var olen = length - 8 * (uint)Vector256<int>.Count + 1;
                if (olen < length)
                {
                    for (; i < olen; i += 8 * (uint)Vector256<int>.Count)
                    {
                        var ymm0 = Vector256.LoadUnsafe(ref head, i + 0 * (uint)Vector256<int>.Count);
                        var ymm1 = Vector256.LoadUnsafe(ref head, i + 1 * (uint)Vector256<int>.Count);
                        var ymm2 = Vector256.LoadUnsafe(ref head, i + 2 * (uint)Vector256<int>.Count);
                        var ymm3 = Vector256.LoadUnsafe(ref head, i + 3 * (uint)Vector256<int>.Count);
                        ymm0 = Avx2.ShiftLeftLogicalVariable(ymm0, ymm15);
                        ymm1 = Avx2.ShiftLeftLogicalVariable(ymm1, ymm15);
                        ymm2 = Avx2.ShiftLeftLogicalVariable(ymm2, ymm15);
                        ymm3 = Avx2.ShiftLeftLogicalVariable(ymm3, ymm15);
                        ymm0.StoreUnsafe(ref head, i + 0 * (uint)Vector256<int>.Count);
                        ymm0 = Vector256.LoadUnsafe(ref head, i + 4 * (uint)Vector256<int>.Count);
                        ymm1.StoreUnsafe(ref head, i + 1 * (uint)Vector256<int>.Count);
                        ymm1 = Vector256.LoadUnsafe(ref head, i + 5 * (uint)Vector256<int>.Count);
                        ymm2.StoreUnsafe(ref head, i + 2 * (uint)Vector256<int>.Count);
                        ymm2 = Vector256.LoadUnsafe(ref head, i + 6 * (uint)Vector256<int>.Count);
                        ymm3.StoreUnsafe(ref head, i + 3 * (uint)Vector256<int>.Count);
                        ymm3 = Vector256.LoadUnsafe(ref head, i + 7 * (uint)Vector256<int>.Count);
                        ymm0 = Avx2.ShiftLeftLogicalVariable(ymm0, ymm15);
                        ymm1 = Avx2.ShiftLeftLogicalVariable(ymm1, ymm15);
                        ymm2 = Avx2.ShiftLeftLogicalVariable(ymm2, ymm15);
                        ymm3 = Avx2.ShiftLeftLogicalVariable(ymm3, ymm15);
                        ymm0.StoreUnsafe(ref head, i + 4 * (uint)Vector256<int>.Count);
                        ymm1.StoreUnsafe(ref head, i + 5 * (uint)Vector256<int>.Count);
                        ymm2.StoreUnsafe(ref head, i + 6 * (uint)Vector256<int>.Count);
                        ymm3.StoreUnsafe(ref head, i + 7 * (uint)Vector256<int>.Count);
                    }
                }
                olen = length - 1 * (uint)Vector256<int>.Count + 1;
                if (olen < length)
                {
                    for (; i < olen; i += (uint)Vector256<int>.Count)
                    {
                        var ymm0 = Vector256.LoadUnsafe(ref head, i);
                        ymm0 = Avx2.ShiftLeftLogicalVariable(ymm0, ymm15);
                        ymm0.StoreUnsafe(ref head, i);
                    }
                }
                for (; i < length; i++)
                {
                    var r15d = Unsafe.Add(ref head, i);
                    r15d <<= shift;
                    Unsafe.Add(ref head, i) = r15d;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void ShiftLeftLogicalSse2(Span<int> span, int shift)
        {
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nuint length = (uint)span.Length;
                var xmm15 = Vector128.Create(shift);
                nuint i = 0;
                var olen = length - 8 * (uint)Vector128<int>.Count + 1;
                if (olen < length)
                {
                    for (; i < olen; i += 8 * (uint)Vector128<int>.Count)
                    {
                        var xmm0 = Vector128.LoadUnsafe(ref head, i + 0 * (uint)Vector128<int>.Count);
                        var xmm1 = Vector128.LoadUnsafe(ref head, i + 1 * (uint)Vector128<int>.Count);
                        var xmm2 = Vector128.LoadUnsafe(ref head, i + 2 * (uint)Vector128<int>.Count);
                        var xmm3 = Vector128.LoadUnsafe(ref head, i + 3 * (uint)Vector128<int>.Count);
                        xmm0 = Sse2.ShiftLeftLogical(xmm0, xmm15);
                        xmm1 = Sse2.ShiftLeftLogical(xmm1, xmm15);
                        xmm2 = Sse2.ShiftLeftLogical(xmm2, xmm15);
                        xmm3 = Sse2.ShiftLeftLogical(xmm3, xmm15);
                        xmm0.StoreUnsafe(ref head, i + 0 * (uint)Vector128<int>.Count);
                        xmm0 = Vector128.LoadUnsafe(ref head, i + 4 * (uint)Vector128<int>.Count);
                        xmm1.StoreUnsafe(ref head, i + 1 * (uint)Vector128<int>.Count);
                        xmm1 = Vector128.LoadUnsafe(ref head, i + 5 * (uint)Vector128<int>.Count);
                        xmm2.StoreUnsafe(ref head, i + 2 * (uint)Vector128<int>.Count);
                        xmm2 = Vector128.LoadUnsafe(ref head, i + 6 * (uint)Vector128<int>.Count);
                        xmm3.StoreUnsafe(ref head, i + 3 * (uint)Vector128<int>.Count);
                        xmm3 = Vector128.LoadUnsafe(ref head, i + 7 * (uint)Vector128<int>.Count);
                        xmm0 = Sse2.ShiftLeftLogical(xmm0, xmm15);
                        xmm1 = Sse2.ShiftLeftLogical(xmm1, xmm15);
                        xmm2 = Sse2.ShiftLeftLogical(xmm2, xmm15);
                        xmm3 = Sse2.ShiftLeftLogical(xmm3, xmm15);
                        xmm0.StoreUnsafe(ref head, i + 4 * (uint)Vector128<int>.Count);
                        xmm1.StoreUnsafe(ref head, i + 5 * (uint)Vector128<int>.Count);
                        xmm2.StoreUnsafe(ref head, i + 6 * (uint)Vector128<int>.Count);
                        xmm3.StoreUnsafe(ref head, i + 7 * (uint)Vector128<int>.Count);
                    }
                }
                olen = length - 1 * (uint)Vector128<int>.Count + 1;
                if (olen < length)
                {
                    for (; i < olen; i += (uint)Vector128<int>.Count)
                    {
                        var xmm0 = Vector128.LoadUnsafe(ref head, i);
                        xmm0 = Sse2.ShiftLeftLogical(xmm0, xmm15);
                        xmm0.StoreUnsafe(ref head, i);
                    }
                }
                for (; i < length; i++)
                {
                    var r15d = Unsafe.Add(ref head, i);
                    r15d <<= shift;
                    Unsafe.Add(ref head, i) = r15d;
                }
            }
        }

#endif
    }
}
