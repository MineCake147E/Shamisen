#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Shamisen.Utils;
using Shamisen.Utils.Intrinsics;

namespace Shamisen.Analysis
{
    public static partial class CooleyTukeyFft
    {
        internal static partial class X86
        {
            #region Perform16
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Perform16X86(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                if (Avx.IsSupported)
                {
                    Perform16Avx(span, omegas);
                    return;
                }
                if (Sse3.IsSupported)
                {
                    Perform16Sse3(span, omegas);
                    return;
                }
                Perform16Sse(span, omegas);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform16Avx(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                ref var rdi = ref MemoryMarshal.GetReference(span);
                nint i = 0, length = span.Length;
                ref var rt = ref MemoryMarshal.GetReference(omegas);
                ref var rt2 = ref Unsafe.Add(ref rt, 4);
                var ymm15 = Avx.DuplicateEvenIndexed(Unsafe.As<ComplexF, Vector256<float>>(ref rt));
                var ymm14 = Avx.DuplicateOddIndexed(Unsafe.As<ComplexF, Vector256<float>>(ref rt));
                var ymm13 = Avx.DuplicateEvenIndexed(Unsafe.As<ComplexF, Vector256<float>>(ref rt2));
                var ymm12 = Avx.DuplicateOddIndexed(Unsafe.As<ComplexF, Vector256<float>>(ref rt2));
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm8 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0));
                    var ymm9 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 4));
                    var ymm0 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8));
                    var ymm1 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 12));
                    var ymm10 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16));
                    var ymm11 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 20));
                    var ymm2 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24));
                    var ymm3 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 28));
                    var ymm4 = Avx.Permute(ymm0, 0b10_11_00_01);
                    ymm0 = Avx.Multiply(ymm0, ymm15);
                    var ymm5 = Avx.Permute(ymm1, 0b10_11_00_01);
                    ymm1 = Avx.Multiply(ymm1, ymm13);
                    var ymm6 = Avx.Permute(ymm2, 0b10_11_00_01);
                    ymm2 = Avx.Multiply(ymm2, ymm15);
                    var ymm7 = Avx.Permute(ymm3, 0b10_11_00_01);
                    ymm3 = Avx.Multiply(ymm3, ymm13);
                    ymm4 = Avx.Multiply(ymm4, ymm14);
                    ymm4 = Avx.AddSubtract(ymm0, ymm4);
                    ymm5 = Avx.Multiply(ymm5, ymm12);
                    ymm5 = Avx.AddSubtract(ymm1, ymm5);
                    ymm6 = Avx.Multiply(ymm6, ymm14);
                    ymm6 = Avx.AddSubtract(ymm2, ymm6);
                    ymm7 = Avx.Multiply(ymm7, ymm12);
                    ymm7 = Avx.AddSubtract(ymm3, ymm7);
                    ymm0 = Avx.Add(ymm8, ymm4);
                    ymm4 = Avx.Subtract(ymm8, ymm4);
                    ymm1 = Avx.Add(ymm9, ymm5);
                    ymm5 = Avx.Subtract(ymm9, ymm5);
                    ymm2 = Avx.Add(ymm10, ymm6);
                    ymm6 = Avx.Subtract(ymm10, ymm6);
                    ymm3 = Avx.Add(ymm11, ymm7);
                    ymm7 = Avx.Subtract(ymm11, ymm7);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 0)) = ymm0;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 4)) = ymm1;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 8)) = ymm4;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 12)) = ymm5;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 16)) = ymm2;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 20)) = ymm3;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 24)) = ymm6;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 28)) = ymm7;
                }
                olen = length - 15;
                for (; i < olen; i += 16)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm8 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0));
                    var ymm9 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 4));
                    var ymm0 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8));
                    var ymm1 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 12));
                    var ymm4 = Avx.Permute(ymm0, 0b10_11_00_01);
                    ymm0 = Avx.Multiply(ymm0, ymm15);
                    var ymm5 = Avx.Permute(ymm1, 0b10_11_00_01);
                    ymm1 = Avx.Multiply(ymm1, ymm13);
                    ymm4 = Avx.Multiply(ymm4, ymm14);
                    ymm4 = Avx.AddSubtract(ymm0, ymm4);
                    ymm5 = Avx.Multiply(ymm5, ymm12);
                    ymm5 = Avx.AddSubtract(ymm1, ymm5);
                    ymm0 = Avx.Add(ymm8, ymm4);
                    ymm4 = Avx.Subtract(ymm8, ymm4);
                    ymm1 = Avx.Add(ymm9, ymm5);
                    ymm5 = Avx.Subtract(ymm9, ymm5);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 0)) = ymm0;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 4)) = ymm1;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 8)) = ymm4;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 12)) = ymm5;
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform16Sse3(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                ref var rdi = ref MemoryMarshal.GetReference(span);
                nint i = 0, length = span.Length;
                ref var rt = ref MemoryMarshal.GetReference(omegas);
                var xmm15 = Sse3.MoveLowAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref rt));
                var xmm14 = Sse3.MoveHighAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref rt));
                var xmm13 = Sse3.MoveLowAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 2)));
                var xmm12 = Sse3.MoveHighAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 2)));
                var xmm11 = Sse3.MoveLowAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 4)));
                var xmm10 = Sse3.MoveHighAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 4)));
                var xmm9 = Sse3.MoveLowAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 6)));
                var xmm8 = Sse3.MoveHighAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 6)));
                var olen = length - 15;
                for (; i < olen; i += 16)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm4 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0));
                    var xmm5 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 2));
                    var xmm0 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8));
                    var xmm1 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 10));
                    var xmm2 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm3 = Sse.Shuffle(xmm1, xmm1, 0b10_11_00_01);
                    xmm0 = Sse.Multiply(xmm0, xmm15);
                    xmm1 = Sse.Multiply(xmm1, xmm13);
                    xmm2 = Sse.Multiply(xmm2, xmm14);
                    xmm2 = Sse3.AddSubtract(xmm0, xmm2);
                    xmm3 = Sse.Multiply(xmm3, xmm12);
                    xmm3 = Sse3.AddSubtract(xmm1, xmm3);
                    xmm0 = Sse.Add(xmm4, xmm2);
                    xmm1 = Sse.Add(xmm5, xmm3);
                    xmm2 = Sse.Subtract(xmm4, xmm2);
                    xmm3 = Sse.Subtract(xmm5, xmm3);
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 0)) = xmm0;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 2)) = xmm1;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 8)) = xmm2;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 10)) = xmm3;
                    xmm4 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4));
                    xmm5 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 6));
                    xmm0 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12));
                    xmm1 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 14));
                    xmm2 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    xmm3 = Sse.Shuffle(xmm1, xmm1, 0b10_11_00_01);
                    xmm0 = Sse.Multiply(xmm0, xmm11);
                    xmm1 = Sse.Multiply(xmm1, xmm9);
                    xmm2 = Sse.Multiply(xmm2, xmm10);
                    xmm2 = Sse3.AddSubtract(xmm0, xmm2);
                    xmm3 = Sse.Multiply(xmm3, xmm8);
                    xmm3 = Sse3.AddSubtract(xmm1, xmm3);
                    xmm0 = Sse.Add(xmm4, xmm2);
                    xmm1 = Sse.Add(xmm5, xmm3);
                    xmm2 = Sse.Subtract(xmm4, xmm2);
                    xmm3 = Sse.Subtract(xmm5, xmm3);
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 4)) = xmm0;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 6)) = xmm1;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 12)) = xmm2;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 14)) = xmm3;
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform16Sse(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                ref var rdi = ref MemoryMarshal.GetReference(span);
                nint i = 0, length = span.Length;
                ref var rt = ref MemoryMarshal.GetReference(omegas);
                var xmm15 = Unsafe.As<ComplexF, Vector128<float>>(ref rt);
                var xmm13 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 2));
                var xmm11 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 4));
                var xmm9 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 6));
                var xmm7 = Vector128.Create(0x8000_0000_0000_0000ul).AsSingle();
                var xmm14 = Sse.Shuffle(xmm15, xmm15, 0b11_11_01_01);
                xmm14 = Sse.Xor(xmm14, xmm7);
                var xmm12 = Sse.Shuffle(xmm13, xmm13, 0b11_11_01_01);
                xmm12 = Sse.Xor(xmm12, xmm7);
                var xmm10 = Sse.Shuffle(xmm11, xmm11, 0b11_11_01_01);
                xmm10 = Sse.Xor(xmm10, xmm7);
                var xmm8 = Sse.Shuffle(xmm9, xmm9, 0b11_11_01_01);
                xmm8 = Sse.Xor(xmm8, xmm7);
                xmm15 = Sse.Shuffle(xmm15, xmm15, 0b10_10_00_00);
                xmm13 = Sse.Shuffle(xmm13, xmm13, 0b10_10_00_00);
                xmm11 = Sse.Shuffle(xmm11, xmm11, 0b10_10_00_00);
                xmm9 = Sse.Shuffle(xmm9, xmm9, 0b10_10_00_00);
                var olen = length - 15;
                for (; i < olen; i += 16)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm4 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0));
                    var xmm5 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 2));
                    var xmm0 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8));
                    var xmm1 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 10));
                    var xmm2 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm3 = Sse.Shuffle(xmm1, xmm1, 0b10_11_00_01);
                    xmm0 = Sse.Multiply(xmm0, xmm15);
                    xmm1 = Sse.Multiply(xmm1, xmm13);
                    xmm2 = Sse.Multiply(xmm2, xmm14);
                    xmm2 = Sse.Add(xmm0, xmm2);
                    xmm3 = Sse.Multiply(xmm3, xmm12);
                    xmm3 = Sse.Add(xmm1, xmm3);
                    xmm0 = Sse.Add(xmm4, xmm2);
                    xmm1 = Sse.Add(xmm5, xmm3);
                    xmm2 = Sse.Subtract(xmm4, xmm2);
                    xmm3 = Sse.Subtract(xmm5, xmm3);
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 0)) = xmm0;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 2)) = xmm1;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 8)) = xmm2;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 10)) = xmm3;
                    xmm4 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4));
                    xmm5 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 6));
                    xmm0 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12));
                    xmm1 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 14));
                    xmm2 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    xmm3 = Sse.Shuffle(xmm1, xmm1, 0b10_11_00_01);
                    xmm0 = Sse.Multiply(xmm0, xmm11);
                    xmm1 = Sse.Multiply(xmm1, xmm9);
                    xmm2 = Sse.Multiply(xmm2, xmm10);
                    xmm2 = Sse.Add(xmm0, xmm2);
                    xmm3 = Sse.Multiply(xmm3, xmm8);
                    xmm3 = Sse.Add(xmm1, xmm3);
                    xmm0 = Sse.Add(xmm4, xmm2);
                    xmm1 = Sse.Add(xmm5, xmm3);
                    xmm2 = Sse.Subtract(xmm4, xmm2);
                    xmm3 = Sse.Subtract(xmm5, xmm3);
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 4)) = xmm0;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 6)) = xmm1;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 12)) = xmm2;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 14)) = xmm3;
                }
            }
            #endregion

            #region PerformLargerOrder
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            private static void PerformLargerOrderX86(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                if (Avx.IsSupported)
                {
                    PerformLargerOrderAvx(span, omegas);
                    return;
                }
                PerformLargerOrderFallbackX86(span, omegas);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            private static void PerformLargerOrderFallbackX86(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                nint mHalf = omegas.Length;
                var m = mHalf * 2;
                var length = span.Length;
                ref var omH = ref MemoryMarshal.GetReference(omegas);
                ref var shead = ref MemoryMarshal.GetReference(span);
                ref var rA = ref shead;
                ref var rB = ref Unsafe.Add(ref rA, mHalf);
                for (nint k = 0; k < length; k += m)
                {
                    nint j = 0;
                    var oHalf = mHalf - 7;
                    for (; j < oHalf; j += 8)
                    {
                        ref var pA = ref Unsafe.Add(ref rA, j);
                        ref var pB = ref Unsafe.Add(ref rB, j);
                        PerformSingleOperation(ref Unsafe.Add(ref pA, 0), ref Unsafe.Add(ref pB, 0), Unsafe.Add(ref omH, j + 0));
                        PerformSingleOperation(ref Unsafe.Add(ref pA, 1), ref Unsafe.Add(ref pB, 1), Unsafe.Add(ref omH, j + 1));
                        PerformSingleOperation(ref Unsafe.Add(ref pA, 2), ref Unsafe.Add(ref pB, 2), Unsafe.Add(ref omH, j + 2));
                        PerformSingleOperation(ref Unsafe.Add(ref pA, 3), ref Unsafe.Add(ref pB, 3), Unsafe.Add(ref omH, j + 3));
                        PerformSingleOperation(ref Unsafe.Add(ref pA, 4), ref Unsafe.Add(ref pB, 4), Unsafe.Add(ref omH, j + 4));
                        PerformSingleOperation(ref Unsafe.Add(ref pA, 5), ref Unsafe.Add(ref pB, 5), Unsafe.Add(ref omH, j + 5));
                        PerformSingleOperation(ref Unsafe.Add(ref pA, 6), ref Unsafe.Add(ref pB, 6), Unsafe.Add(ref omH, j + 6));
                        PerformSingleOperation(ref Unsafe.Add(ref pA, 7), ref Unsafe.Add(ref pB, 7), Unsafe.Add(ref omH, j + 7));
                    }
                    rA = ref Unsafe.Add(ref rA, m);
                    rB = ref Unsafe.Add(ref rB, m);
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void PerformLargerOrderAvx(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                nint mHalf = omegas.Length;
                var m = mHalf * 2;
                var mhMask = mHalf - 1;
                nint i = 0, j = 0, k = 0, length = span.Length >> 1;
                ref var rdi = ref MemoryMarshal.GetReference(span);
                ref var rsi = ref MemoryMarshal.GetReference(omegas);
                var olen = length - 15;
                Vector256<float> ymm8, ymm9, ymm10, ymm11, ymm12, ymm13, ymm14, ymm15;
                ymm8 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, 0));
                ymm12 = Avx.DuplicateOddIndexed(ymm8);
                ymm9 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, 4));
                ymm13 = Avx.DuplicateOddIndexed(ymm9);
                ymm10 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, 8));
                ymm14 = Avx.DuplicateOddIndexed(ymm10);
                ymm11 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, 12));
                ymm15 = Avx.DuplicateOddIndexed(ymm11);
                ymm8 = Avx.DuplicateEvenIndexed(ymm8);
                ymm9 = Avx.DuplicateEvenIndexed(ymm9);
                ymm10 = Avx.DuplicateEvenIndexed(ymm10);
                ymm11 = Avx.DuplicateEvenIndexed(ymm11);
                for (; i < olen; i += 16)
                {
                    ref var r9 = ref Unsafe.Add(ref rdi, j + k + mHalf);
                    ref var r10 = ref Unsafe.Add(ref rdi, j + k);
                    k += 16;
                    var u = MathI.ZeroIfFalse(k >= mHalf, m);
                    k &= mhMask;
                    j += u;
                    var ymm0 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r9, 0));
                    var ymm4 = Avx.Permute(ymm0, 0b10_11_00_01);
                    var ymm1 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r9, 4));
                    var ymm5 = Avx.Permute(ymm1, 0b10_11_00_01);
                    var ymm2 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r9, 8));
                    var ymm6 = Avx.Permute(ymm2, 0b10_11_00_01);
                    var ymm3 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r9, 12));
                    var ymm7 = Avx.Permute(ymm3, 0b10_11_00_01);
                    ymm0 = Avx.Multiply(ymm0, ymm8);
                    ymm8 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r10, 0));
                    ymm1 = Avx.Multiply(ymm1, ymm9);
                    ymm9 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r10, 4));
                    ymm2 = Avx.Multiply(ymm2, ymm10);
                    ymm10 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r10, 8));
                    ymm3 = Avx.Multiply(ymm3, ymm11);
                    ymm11 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r10, 12));
                    r10 = ref Unsafe.Add(ref r10, mHalf);
                    r9 = ref Unsafe.Subtract(ref r9, mHalf);
                    ref var r8 = ref Unsafe.Add(ref rsi, k);
                    ymm4 = Avx.Multiply(ymm4, ymm12);
                    ymm4 = Avx.AddSubtract(ymm0, ymm4);
                    ymm5 = Avx.Multiply(ymm5, ymm13);
                    ymm5 = Avx.AddSubtract(ymm1, ymm5);
                    ymm6 = Avx.Multiply(ymm6, ymm14);
                    ymm6 = Avx.AddSubtract(ymm2, ymm6);
                    ymm7 = Avx.Multiply(ymm7, ymm15);
                    ymm7 = Avx.AddSubtract(ymm3, ymm7);
                    ymm0 = Avx.Add(ymm8, ymm4);
                    ymm1 = Avx.Add(ymm9, ymm5);
                    ymm12 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 0));
                    ymm13 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 4));
                    ymm2 = Avx.Add(ymm10, ymm6);
                    ymm3 = Avx.Add(ymm11, ymm7);
                    ymm14 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 8));
                    ymm15 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 12));
                    ymm4 = Avx.Subtract(ymm8, ymm4);
                    ymm5 = Avx.Subtract(ymm9, ymm5);
                    ymm8 = Avx.DuplicateEvenIndexed(ymm12);
                    ymm9 = Avx.DuplicateEvenIndexed(ymm13);
                    ymm6 = Avx.Subtract(ymm10, ymm6);
                    ymm7 = Avx.Subtract(ymm11, ymm7);
                    ymm10 = Avx.DuplicateEvenIndexed(ymm14);
                    ymm11 = Avx.DuplicateEvenIndexed(ymm15);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r9, 0)) = ymm0;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r9, 4)) = ymm1;
                    ymm12 = Avx.DuplicateOddIndexed(ymm12);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r9, 8)) = ymm2;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r9, 12)) = ymm3;
                    ymm13 = Avx.DuplicateOddIndexed(ymm13);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r10, 0)) = ymm4;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r10, 4)) = ymm5;
                    ymm14 = Avx.DuplicateOddIndexed(ymm14);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r10, 8)) = ymm6;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r10, 12)) = ymm7;
                    ymm15 = Avx.DuplicateOddIndexed(ymm15);
                }
            }
            #endregion

        }
    }
}
#endif