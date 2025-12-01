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
        public static void ShiftRightArithmetic(Span<int> span, int shift)
        {
            if ((shift & 31) == 0 || span.IsEmpty) return;
            if (ShiftRightArithmeticArm(span, shift)) return;
            if (ShiftRightArithmeticX86(span, shift)) return;
            //fallback
            if (ShiftRightArithmeticStandard(shift, span)) return;
            ShiftRightArithmeticSimple(shift, span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static bool ShiftRightArithmeticStandard(int shift, Span<int> span)
        {
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nint length = span.Length;
                var vlen = length - length % 4;
                nint i;
                for (i = 0; i < vlen; i += 4)
                {
                    var v0 = Unsafe.Add(ref head, i + 0);
                    var v1 = Unsafe.Add(ref head, i + 1);
                    var v2 = Unsafe.Add(ref head, i + 2);
                    var v3 = Unsafe.Add(ref head, i + 3);
                    v0 >>= shift;
                    v1 >>= shift;
                    v2 >>= shift;
                    v3 >>= shift;
                    Unsafe.Add(ref head, i + 0) = v0;
                    Unsafe.Add(ref head, i + 1) = v1;
                    Unsafe.Add(ref head, i + 2) = v2;
                    Unsafe.Add(ref head, i + 3) = v3;
                }
                for (; i < length; i++)
                {
                    var value = Unsafe.Add(ref head, i);
                    value >>= shift;
                    Unsafe.Add(ref head, i) = value;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void ShiftRightArithmeticSimple(int shift, Span<int> span)
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = span[i] >> shift;
            }
        }

#if NET5_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static bool ShiftRightArithmeticArm(Span<int> span, int shift)
        {
            if (!AdvSimd.IsSupported) return false;
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nint length = span.Length;
                var vlen = length - length % Vector128<int>.Count;
                var avlen = (vlen - vlen % (8 * Vector128<int>.Count)) * sizeof(int);
                var v0 = Vector128.Create(shift);
                ref var vhead = ref Unsafe.As<int, Vector128<int>>(ref head);
                nint i;
                const int Size = sizeof(int);
                for (i = 0; i < avlen; i += 8 * Vector128<int>.Count * sizeof(int))
                {
                    //Changing ways to offset to suppress RyuJIT allocating more registers for storing offset addresses.
                    var v1 = Unsafe.AddByteOffset(ref vhead, i + 16 * 0);
                    var v2 = Unsafe.AddByteOffset(ref vhead, i + 16 * 1);
                    var v3 = Unsafe.AddByteOffset(ref vhead, i + 16 * 2);
                    var v4 = Unsafe.AddByteOffset(ref vhead, i + 16 * 3);

                    v1 = AdvSimd.ShiftArithmetic(v1, v0);
                    v2 = AdvSimd.ShiftArithmetic(v2, v0);
                    v3 = AdvSimd.ShiftArithmetic(v3, v0);
                    v4 = AdvSimd.ShiftArithmetic(v4, v0);

                    Unsafe.AddByteOffset(ref vhead, i) = v1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 1) = v2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 2) = v3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 3) = v4;

                    v1 = Unsafe.AddByteOffset(ref vhead, i + 16 * 4);
                    v2 = Unsafe.AddByteOffset(ref vhead, i + 16 * 5);
                    v3 = Unsafe.AddByteOffset(ref vhead, i + 16 * 6);
                    v4 = Unsafe.AddByteOffset(ref vhead, i + 16 * 7);

                    v1 = AdvSimd.ShiftArithmetic(v1, v0);
                    v2 = AdvSimd.ShiftArithmetic(v2, v0);
                    v3 = AdvSimd.ShiftArithmetic(v3, v0);
                    v4 = AdvSimd.ShiftArithmetic(v4, v0);

                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 4) = v1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 5) = v2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 6) = v3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 7) = v4;
                }
                for (; i < vlen; i += Vector128<int>.Count)
                {
                    var v1 = Unsafe.AddByteOffset(ref vhead, Size * i);
                    v1 = AdvSimd.ShiftArithmetic(v1, v0);
                    Unsafe.AddByteOffset(ref vhead, Size * i) = v1;
                }

                for (; i < length; i++)
                {
                    var w15 = Unsafe.Add(ref head, i);
                    w15 >>= shift;
                    Unsafe.Add(ref head, i) = w15;
                }
            }
            return true;
        }

#endif
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static bool ShiftRightArithmeticX86(Span<int> span, int shift)
        {
            if (Avx2.IsSupported)
            {
                ShiftRightArithmeticAvx2(span, shift);
                return true;
            }
            if (Sse2.IsSupported)
            {
                ShiftRightArithmeticSse2(span, shift);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void ShiftRightArithmeticAvx2(Span<int> span, int shift)
        {
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nint length = span.Length;
                var vlen = length - length % Vector256<int>.Count;
                var avlen = (vlen - vlen % (8 * Vector256<int>.Count)) * sizeof(int);
                var ymm0 = Vector256.Create((uint)shift);
                ref var vhead = ref Unsafe.As<int, Vector256<int>>(ref head);
                nint i;
                const int Size = sizeof(int);
                for (i = 0; i < avlen; i += 8 * Vector256<int>.Count * sizeof(int))
                {
                    //Changing ways to offset to suppress RyuJIT allocating more registers for storing offset addresses.
                    var ymm1 = Unsafe.AddByteOffset(ref vhead, i + 32 * 0);
                    var ymm2 = Unsafe.AddByteOffset(ref vhead, i + 32 * 1);
                    var ymm3 = Unsafe.AddByteOffset(ref vhead, i + 32 * 2);
                    var ymm4 = Unsafe.AddByteOffset(ref vhead, i + 32 * 3);

                    ymm1 = Avx2.ShiftRightArithmeticVariable(ymm1, ymm0);
                    ymm2 = Avx2.ShiftRightArithmeticVariable(ymm2, ymm0);
                    ymm3 = Avx2.ShiftRightArithmeticVariable(ymm3, ymm0);
                    ymm4 = Avx2.ShiftRightArithmeticVariable(ymm4, ymm0);

                    Unsafe.AddByteOffset(ref vhead, i) = ymm1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 1) = ymm2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 2) = ymm3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 3) = ymm4;

                    ymm1 = Unsafe.AddByteOffset(ref vhead, i + 32 * 4);
                    ymm2 = Unsafe.AddByteOffset(ref vhead, i + 32 * 5);
                    ymm3 = Unsafe.AddByteOffset(ref vhead, i + 32 * 6);
                    ymm4 = Unsafe.AddByteOffset(ref vhead, i + 32 * 7);

                    ymm1 = Avx2.ShiftRightArithmeticVariable(ymm1, ymm0);
                    ymm2 = Avx2.ShiftRightArithmeticVariable(ymm2, ymm0);
                    ymm3 = Avx2.ShiftRightArithmeticVariable(ymm3, ymm0);
                    ymm4 = Avx2.ShiftRightArithmeticVariable(ymm4, ymm0);

                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 4) = ymm1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 5) = ymm2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 6) = ymm3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 7) = ymm4;
                }
                for (; i < vlen; i += Vector256<int>.Count)
                {
                    var ymm1 = Unsafe.AddByteOffset(ref vhead, Size * i);
                    ymm1 = Avx2.ShiftRightArithmeticVariable(ymm1, ymm0);
                    Unsafe.AddByteOffset(ref vhead, Size * i) = ymm1;
                }

                for (; i < length; i++)
                {
                    var r15d = Unsafe.Add(ref head, i);
                    r15d >>= shift;
                    Unsafe.Add(ref head, i) = r15d;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void ShiftRightArithmeticSse2(Span<int> span, int shift)
        {
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nint length = span.Length;
                var vlen = length - length % Vector128<int>.Count;
                var avlen = (vlen - vlen % (8 * Vector128<int>.Count)) * sizeof(int);
                var xmm0 = Vector128.Create(shift);
                ref var vhead = ref Unsafe.As<int, Vector128<int>>(ref head);
                nint i;
                const int Size = sizeof(int);
                for (i = 0; i < avlen; i += 8 * Vector128<int>.Count * sizeof(int))
                {
                    //Changing ways to offset to suppress RyuJIT allocating more registers for storing offset addresses.
                    var xmm1 = Unsafe.AddByteOffset(ref vhead, i + 16 * 0);
                    var xmm2 = Unsafe.AddByteOffset(ref vhead, i + 16 * 1);
                    var xmm3 = Unsafe.AddByteOffset(ref vhead, i + 16 * 2);
                    var xmm4 = Unsafe.AddByteOffset(ref vhead, i + 16 * 3);

                    xmm1 = Sse2.ShiftRightArithmetic(xmm1, xmm0);
                    xmm2 = Sse2.ShiftRightArithmetic(xmm2, xmm0);
                    xmm3 = Sse2.ShiftRightArithmetic(xmm3, xmm0);
                    xmm4 = Sse2.ShiftRightArithmetic(xmm4, xmm0);

                    Unsafe.AddByteOffset(ref vhead, i) = xmm1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 1) = xmm2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 2) = xmm3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 3) = xmm4;

                    xmm1 = Unsafe.AddByteOffset(ref vhead, i + 16 * 4);
                    xmm2 = Unsafe.AddByteOffset(ref vhead, i + 16 * 5);
                    xmm3 = Unsafe.AddByteOffset(ref vhead, i + 16 * 6);
                    xmm4 = Unsafe.AddByteOffset(ref vhead, i + 16 * 7);

                    xmm1 = Sse2.ShiftRightArithmetic(xmm1, xmm0);
                    xmm2 = Sse2.ShiftRightArithmetic(xmm2, xmm0);
                    xmm3 = Sse2.ShiftRightArithmetic(xmm3, xmm0);
                    xmm4 = Sse2.ShiftRightArithmetic(xmm4, xmm0);

                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 4) = xmm1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 5) = xmm2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 6) = xmm3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 7) = xmm4;
                }
                for (; i < vlen; i += Vector128<int>.Count)
                {
                    var xmm1 = Unsafe.AddByteOffset(ref vhead, Size * i);
                    xmm1 = Sse2.ShiftRightArithmetic(xmm1, xmm0);
                    Unsafe.AddByteOffset(ref vhead, Size * i) = xmm1;
                }

                for (; i < length; i++)
                {
                    var r15d = Unsafe.Add(ref head, i);
                    r15d >>= shift;
                    Unsafe.Add(ref head, i) = r15d;
                }
            }
        }

#endif
    }
}
