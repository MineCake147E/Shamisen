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
            internal static bool IsSupported => AudioUtils.X86.IsSupported;
            #region Perform2

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Perform2X86(Span<ComplexF> span)
            {
                if (Avx2.IsSupported)
                {
                    Perform2Avx2(span);
                    return;
                }
                if (Sse.IsSupported)
                {
                    Perform2Sse(span);
                    return;
                }
                Fallback.Perform2Fallback(span);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform2Avx2(Span<ComplexF> span)
            {
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(span));
                nint i = 0, length = span.Length;
                var xmm15 = Vector128<ulong>.Zero.WithElement(1, 0x8000_0000_8000_0000ul);
                var ymm15 = xmm15.ToVector256Unsafe().WithUpper(xmm15);
                var olen = length - 4 * Vector256<double>.Count + 1;
                for (; i < olen; i += 4 * Vector256<double>.Count)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm0 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * Vector256<double>.Count));
                    var ymm1 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * Vector256<double>.Count));
                    var ymm2 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 2 * Vector256<double>.Count));
                    var ymm3 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 3 * Vector256<double>.Count));
                    var ymm4 = Avx.Shuffle(ymm0.AsDouble(), ymm0.AsDouble(), 0b1111).AsSingle();
                    var ymm5 = Avx.Shuffle(ymm1.AsDouble(), ymm1.AsDouble(), 0b1111).AsSingle();
                    var ymm6 = Avx.Shuffle(ymm2.AsDouble(), ymm2.AsDouble(), 0b1111).AsSingle();
                    var ymm7 = Avx.Shuffle(ymm3.AsDouble(), ymm3.AsDouble(), 0b1111).AsSingle();
                    ymm0 = Avx.DuplicateEvenIndexed(ymm0.AsDouble()).AsSingle();
                    ymm4 = Avx2.Xor(ymm4.AsUInt64(), ymm15).AsSingle();
                    ymm1 = Avx.DuplicateEvenIndexed(ymm1.AsDouble()).AsSingle();
                    ymm5 = Avx2.Xor(ymm5.AsUInt64(), ymm15).AsSingle();
                    ymm2 = Avx.DuplicateEvenIndexed(ymm2.AsDouble()).AsSingle();
                    ymm6 = Avx2.Xor(ymm6.AsUInt64(), ymm15).AsSingle();
                    ymm3 = Avx.DuplicateEvenIndexed(ymm3.AsDouble()).AsSingle();
                    ymm7 = Avx2.Xor(ymm7.AsUInt64(), ymm15).AsSingle();
                    ymm0 = Avx.Add(ymm0, ymm4);
                    ymm1 = Avx.Add(ymm1, ymm5);
                    ymm2 = Avx.Add(ymm2, ymm6);
                    ymm3 = Avx.Add(ymm3, ymm7);
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 0 * Vector256<double>.Count)) = ymm0;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 1 * Vector256<double>.Count)) = ymm1;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 2 * Vector256<double>.Count)) = ymm2;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 3 * Vector256<double>.Count)) = ymm3;
                }
                olen = length - 2 * Vector128<double>.Count + 1;
                for (; i < olen; i += 2 * Vector128<double>.Count)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * Vector128<double>.Count));
                    var xmm1 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 1 * Vector128<double>.Count));
                    var xmm4 = Sse2.Shuffle(xmm0.AsDouble(), xmm0.AsDouble(), 0b11).AsSingle();
                    var xmm5 = Sse2.Shuffle(xmm1.AsDouble(), xmm1.AsDouble(), 0b11).AsSingle();
                    xmm0 = Sse3.MoveAndDuplicate(xmm0.AsDouble()).AsSingle();
                    xmm4 = Sse2.Xor(xmm4.AsUInt64(), ymm15.GetLower()).AsSingle();
                    xmm1 = Sse3.MoveAndDuplicate(xmm1.AsDouble()).AsSingle();
                    xmm5 = Sse2.Xor(xmm5.AsUInt64(), ymm15.GetLower()).AsSingle();
                    xmm0 = Sse.Add(xmm0, xmm4);
                    xmm1 = Sse.Add(xmm1, xmm5);
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm1;
                }
                olen = length - 1;
                for (; i < olen; i += 2)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i));
                    var xmm4 = Sse2.Shuffle(xmm0.AsDouble(), xmm0.AsDouble(), 0b11).AsSingle();
                    xmm0 = Sse3.MoveAndDuplicate(xmm0.AsDouble()).AsSingle();
                    xmm4 = Sse2.Xor(xmm4.AsUInt64(), ymm15.GetLower()).AsSingle();
                    xmm0 = Sse.Add(xmm0, xmm4);
                    Unsafe.As<double, Vector128<float>>(ref r8) = xmm0;
                }
            }
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform2Sse(Span<ComplexF> span)
            {
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(span));
                nint i = 0, length = span.Length;
                var xmm15 = Vector128<ulong>.Zero.WithElement(1, 0x8000_0000_8000_0000ul).AsSingle();
                var olen = length - 2 * Vector128<double>.Count + 1;
                for (; i < olen; i += 2 * Vector128<double>.Count)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * Vector128<double>.Count));
                    var xmm1 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 1 * Vector128<double>.Count));
                    var xmm4 = Sse.Shuffle(xmm0, xmm0, 0b11_10_11_10);
                    var xmm5 = Sse.Shuffle(xmm1, xmm1, 0b11_10_11_10);
                    xmm0 = Sse.Shuffle(xmm0, xmm0, 0b01_00_01_00);
                    xmm4 = Sse.Xor(xmm4, xmm15).AsSingle();
                    xmm1 = Sse.Shuffle(xmm1, xmm1, 0b01_00_01_00);
                    xmm5 = Sse.Xor(xmm5, xmm15).AsSingle();
                    xmm0 = Sse.Add(xmm0, xmm4);
                    xmm1 = Sse.Add(xmm1, xmm5);
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm1;
                }
                olen = length - 1;
                for (; i < olen; i += 2)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i));
                    var xmm4 = Sse.Shuffle(xmm0, xmm0, 0b11_10_11_10);
                    xmm0 = Sse.Shuffle(xmm0, xmm0, 0b01_00_01_00);
                    xmm4 = Sse.Xor(xmm4, xmm15).AsSingle();
                    xmm0 = Sse.Add(xmm0, xmm4);
                    Unsafe.As<double, Vector128<float>>(ref r8) = xmm0;
                }
            }
            #endregion

            #region Perform4
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Perform4X86(Span<ComplexF> span, FftMode mode)
            {
                if (Avx2.IsSupported)
                {
                    Perform4Avx2(span, mode);
                    return;
                }
                if (Sse.IsSupported)
                {
                    Perform4Sse(span, mode);
                    return;
                }
                Fallback.Perform4Fallback(span, mode);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform4Avx2(Span<ComplexF> span, FftMode mode)
            {
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(span));
                var s = MathI.ZeroIfFalse(mode == FftMode.Backward, 32);
                var g = 0x8000_0000ul << s;
                nint i = 0, length = span.Length;
                var ymm15 = Vector128<ulong>.Zero.WithElement(1, g).ToVector256Unsafe();
                ymm15 = Avx2.Permute2x128(ymm15, ymm15, 0b0000_1000);
                var ymm14 = Vector256.Create(4, 5, 7, 6, 4, 5, 7, 6);
                var ymm13 = Vector128.Create(0x8000_0000u).ToVector256Unsafe();
                ymm13 = Avx2.Permute2x128(ymm13, ymm13, 0b0000_1000);
                var olen = length - 15;
                for (; i < olen; i += 16)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm0 = Avx2.Xor(ymm15, Unsafe.As<double, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 0 * Vector256<double>.Count))).AsSingle();
                    var ymm1 = Avx2.Xor(ymm15, Unsafe.As<double, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 1 * Vector256<double>.Count))).AsSingle();
                    var ymm2 = Avx2.Xor(ymm15, Unsafe.As<double, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 2 * Vector256<double>.Count))).AsSingle();
                    var ymm3 = Avx2.Xor(ymm15, Unsafe.As<double, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 3 * Vector256<double>.Count))).AsSingle();
                    var ymm4 = Avx2.PermuteVar8x32(ymm0, ymm14);
                    ymm0 = Avx.Permute2x128(ymm0, ymm0, 0b0000_0000);
                    ymm4 = Avx2.Xor(ymm4.AsUInt32(), ymm13).AsSingle();
                    var ymm5 = Avx2.PermuteVar8x32(ymm1, ymm14);
                    ymm1 = Avx.Permute2x128(ymm1, ymm1, 0b0000_0000);
                    ymm5 = Avx2.Xor(ymm5.AsUInt32(), ymm13).AsSingle();
                    var ymm6 = Avx2.PermuteVar8x32(ymm2, ymm14);
                    ymm2 = Avx.Permute2x128(ymm2, ymm2, 0b0000_0000);
                    ymm6 = Avx2.Xor(ymm6.AsUInt32(), ymm13).AsSingle();
                    var ymm7 = Avx2.PermuteVar8x32(ymm3, ymm14);
                    ymm3 = Avx.Permute2x128(ymm3, ymm3, 0b0000_0000);
                    ymm7 = Avx2.Xor(ymm7.AsUInt32(), ymm13).AsSingle();
                    ymm0 = Avx.Add(ymm0, ymm4);
                    ymm1 = Avx.Add(ymm1, ymm5);
                    ymm2 = Avx.Add(ymm2, ymm6);
                    ymm3 = Avx.Add(ymm3, ymm7);
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 0 * Vector256<double>.Count)) = ymm0;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 1 * Vector256<double>.Count)) = ymm1;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 2 * Vector256<double>.Count)) = ymm2;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 3 * Vector256<double>.Count)) = ymm3;
                }
                ymm15 = Avx2.Permute2x128(ymm15, ymm15, 0b1000_0001);
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm1 = Sse2.Xor(ymm15.GetLower(), Unsafe.As<double, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 1 * Vector128<double>.Count))).AsSingle();
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * Vector128<double>.Count));
                    xmm1 = Sse.Shuffle(xmm1, xmm1, 0b10_11_01_00);
                    var xmm2 = Sse.Add(xmm0, xmm1);
                    xmm1 = Sse.Subtract(xmm0, xmm1);
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm2;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm1;
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform4Sse(Span<ComplexF> span, FftMode mode)
            {
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(span));
                var s = MathI.ZeroIfFalse(mode == FftMode.Backward, 32);
                var g = 0x8000_0000ul << s;
                nint i = 0, length = span.Length;
                var xmm15 = Vector128<ulong>.Zero.WithElement(1, g);
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm1 = Sse2.Xor(xmm15, Unsafe.As<double, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 1 * Vector128<double>.Count))).AsSingle();
                    var xmm3 = Sse2.Xor(xmm15, Unsafe.As<double, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 3 * Vector128<double>.Count))).AsSingle();
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * Vector128<double>.Count));
                    var xmm2 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 2 * Vector128<double>.Count));
                    xmm1 = Sse.Shuffle(xmm1, xmm1, 0b10_11_01_00);
                    xmm3 = Sse.Shuffle(xmm3, xmm3, 0b10_11_01_00);
                    var xmm4 = Sse.Add(xmm0, xmm1);
                    var xmm5 = Sse.Add(xmm2, xmm3);
                    xmm0 = Sse.Subtract(xmm0, xmm1);
                    xmm1 = Sse.Subtract(xmm2, xmm3);
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm4;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 2 * Vector128<double>.Count)) = xmm5;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 3 * Vector128<double>.Count)) = xmm1;
                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm1 = Sse2.Xor(xmm15, Unsafe.As<double, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 1 * Vector128<double>.Count))).AsSingle();
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * Vector128<double>.Count));
                    xmm1 = Sse.Shuffle(xmm1, xmm1, 0b10_11_01_00);
                    var xmm2 = Sse.Add(xmm0, xmm1);
                    xmm1 = Sse.Subtract(xmm0, xmm1);
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm2;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm1;
                }
            }
            #endregion
            #region Perform8
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Perform8X86(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                if (Avx.IsSupported)
                {
                    Perform8Avx(span, omegas);
                    return;
                }
                if (Sse3.IsSupported)
                {
                    Perform8Sse3(span, omegas);
                    return;
                }
                Perform8Sse(span, omegas);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform8Avx(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                ref var rdi = ref MemoryMarshal.GetReference(span);
                nint i = 0, length = span.Length;
                ref var rt = ref MemoryMarshal.GetReference(omegas);
                var ymm15 = Avx.DuplicateEvenIndexed(Unsafe.As<ComplexF, Vector256<float>>(ref rt));
                var ymm14 = Avx.DuplicateOddIndexed(Unsafe.As<ComplexF, Vector256<float>>(ref rt));
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm8 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0));
                    var ymm0 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 4));
                    var ymm9 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8));
                    var ymm1 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 12));
                    var ymm10 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16));
                    var ymm2 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 20));
                    var ymm11 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24));
                    var ymm3 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 28));
                    var ymm4 = Avx.Permute(ymm0, 0b10_11_00_01);
                    ymm0 = Avx.Multiply(ymm0, ymm15);
                    var ymm5 = Avx.Permute(ymm1, 0b10_11_00_01);
                    ymm1 = Avx.Multiply(ymm1, ymm15);
                    var ymm6 = Avx.Permute(ymm2, 0b10_11_00_01);
                    ymm2 = Avx.Multiply(ymm2, ymm15);
                    var ymm7 = Avx.Permute(ymm3, 0b10_11_00_01);
                    ymm3 = Avx.Multiply(ymm3, ymm15);
                    ymm4 = Avx.Multiply(ymm4, ymm14);
                    ymm4 = Avx.AddSubtract(ymm0, ymm4);
                    ymm5 = Avx.Multiply(ymm5, ymm14);
                    ymm5 = Avx.AddSubtract(ymm1, ymm5);
                    ymm6 = Avx.Multiply(ymm6, ymm14);
                    ymm6 = Avx.AddSubtract(ymm2, ymm6);
                    ymm7 = Avx.Multiply(ymm7, ymm14);
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
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 4)) = ymm4;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 8)) = ymm1;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 12)) = ymm5;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 16)) = ymm2;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 20)) = ymm6;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 24)) = ymm3;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 28)) = ymm7;
                }
                olen = length - 7;
                for (; i < olen; i += 8)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm8 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0));
                    var ymm0 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 4));
                    var ymm4 = Avx.Permute(ymm0, 0b10_11_00_01);
                    ymm0 = Avx.Multiply(ymm0, ymm15);
                    ymm4 = Avx.Multiply(ymm4, ymm14);
                    ymm4 = Avx.AddSubtract(ymm0, ymm4);
                    ymm0 = Avx.Add(ymm8, ymm4);
                    ymm4 = Avx.Subtract(ymm8, ymm4);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 0)) = ymm0;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 4)) = ymm4;
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform8Sse3(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                ref var rdi = ref MemoryMarshal.GetReference(span);
                nint i = 0, length = span.Length;
                ref var rt = ref MemoryMarshal.GetReference(omegas);
                var xmm15 = Sse3.MoveLowAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref rt));
                var xmm14 = Sse3.MoveHighAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref rt));
                var xmm13 = Sse3.MoveLowAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 2)));
                var xmm12 = Sse3.MoveHighAndDuplicate(Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 2)));
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm8 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0));
                    var xmm9 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 2));
                    var xmm0 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4));
                    var xmm1 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 6));
                    var xmm4 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm5 = Sse.Shuffle(xmm1, xmm1, 0b10_11_00_01);
                    xmm0 = Sse.Multiply(xmm0, xmm15);
                    xmm1 = Sse.Multiply(xmm1, xmm13);
                    xmm4 = Sse.Multiply(xmm4, xmm14);
                    xmm4 = Sse3.AddSubtract(xmm0, xmm4);
                    xmm5 = Sse.Multiply(xmm5, xmm12);
                    xmm5 = Sse3.AddSubtract(xmm1, xmm5);
                    xmm0 = Sse.Add(xmm8, xmm4);
                    xmm1 = Sse.Add(xmm9, xmm5);
                    xmm4 = Sse.Subtract(xmm8, xmm4);
                    xmm5 = Sse.Subtract(xmm9, xmm5);
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 0)) = xmm0;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 2)) = xmm1;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 4)) = xmm4;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 6)) = xmm5;
                }
            }
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform8Sse(Span<ComplexF> span, ReadOnlySpan<ComplexF> omegas)
            {
                ref var rdi = ref MemoryMarshal.GetReference(span);
                nint i = 0, length = span.Length;
                ref var rt = ref MemoryMarshal.GetReference(omegas);
                var xmm15 = Unsafe.As<ComplexF, Vector128<float>>(ref rt);
                var xmm13 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rt, 2));
                var xmm11 = Vector128.Create(0x8000_0000_0000_0000ul).AsSingle();
                var xmm14 = Sse.Shuffle(xmm15, xmm15, 0b11_11_01_01);
                xmm14 = Sse.Xor(xmm14, xmm11);
                var xmm12 = Sse.Shuffle(xmm13, xmm13, 0b11_11_01_01);
                xmm12 = Sse.Xor(xmm12, xmm11);
                xmm15 = Sse.Shuffle(xmm15, xmm15, 0b10_10_00_00);
                xmm13 = Sse.Shuffle(xmm13, xmm13, 0b10_10_00_00);
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4));
                    var xmm1 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 6));
                    var xmm4 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm5 = Sse.Shuffle(xmm1, xmm1, 0b10_11_00_01);
                    xmm0 = Sse.Multiply(xmm0, xmm15);
                    xmm1 = Sse.Multiply(xmm1, xmm13);
                    xmm4 = Sse.Multiply(xmm4, xmm14);
                    xmm4 = Sse.Add(xmm0, xmm4);
                    xmm5 = Sse.Multiply(xmm5, xmm12);
                    xmm5 = Sse.Add(xmm1, xmm5);
                    var xmm8 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0));
                    var xmm9 = Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 2));
                    xmm0 = Sse.Add(xmm8, xmm4);
                    xmm1 = Sse.Add(xmm9, xmm5);
                    xmm4 = Sse.Subtract(xmm8, xmm4);
                    xmm5 = Sse.Subtract(xmm9, xmm5);
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 0)) = xmm0;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 2)) = xmm1;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 4)) = xmm4;
                    Unsafe.As<ComplexF, Vector128<float>>(ref Unsafe.Add(ref r8, 6)) = xmm5;
                }
            }
            #endregion

            #region PerformLarge
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void PerformLargeX86(Span<ComplexF> span, FftMode mode)
            {
                var pool = ArrayPool<byte>.Shared;
                var length = span.Length;
                var region = pool.Rent(Unsafe.SizeOf<ComplexF>() * length / 2);
                var buffer = MemoryMarshal.Cast<byte, ComplexF>(region.AsSpan());
                var omegas = buffer.Slice(0, length / 2);
                var oo8 = mode == FftMode.Forward ? OmegasForwardOrder8 : OmegasBackwardOrder8;
                oo8.CopyTo(omegas.SliceFromEnd(oo8.Length));
                Perform8X86(span, omegas.SliceFromEnd(4));
                if (length < 16) return;
                ExpandCache(omegas.SliceFromEnd(8), mode);
                Perform16X86(span, omegas.SliceFromEnd(8));
                if (length < 32) return;
                for (var m = 32; m <= length; m <<= 1)
                {
                    nint mHalf = m >> 1;
                    var omorder = omegas.SliceFromEnd((int)mHalf);
                    ExpandCache(omorder, mode);
                    PerformLargerOrderX86(span, omorder);
                }
                pool.Return(region);
            }

            #endregion

            #region PerformSingleOperation
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void PerformSingleOperationX86(ref ComplexF pA, ref ComplexF pB, in ComplexF om)
            {
                if (Avx2.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<ComplexF, double>(ref pB)).AsSingle();
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<ComplexF, double>(ref Unsafe.AsRef(om))).AsSingle();
                    var xmm2 = Avx2.BroadcastScalarToVector128(xmm0);
                    var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<ComplexF, double>(ref pA)).AsSingle();
                    xmm2 = Sse.Multiply(xmm2, xmm1);
                    xmm0 = Sse3.MoveHighAndDuplicate(xmm0);
                    xmm1 = Avx.Permute(xmm1, 0xb1);
                    xmm0 = Sse.Multiply(xmm0, xmm1);
                    xmm0 = Sse3.AddSubtract(xmm2, xmm0);
                    xmm2 = Sse.Add(xmm3, xmm0);
                    xmm1 = Sse.Subtract(xmm3, xmm0);
                    Unsafe.As<ComplexF, double>(ref pA) = xmm2.AsDouble().GetElement(0);
                    Unsafe.As<ComplexF, double>(ref pB) = xmm1.AsDouble().GetElement(0);
                    return;
                }
                if (Sse3.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<ComplexF, double>(ref pB)).AsSingle();
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<ComplexF, double>(ref Unsafe.AsRef(om))).AsSingle();
                    var xmm2 = Sse3.MoveLowAndDuplicate(xmm0);
                    var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<ComplexF, double>(ref pA)).AsSingle();
                    xmm2 = Sse.Multiply(xmm2, xmm1);
                    xmm0 = Sse3.MoveHighAndDuplicate(xmm0);
                    xmm1 = Sse.Shuffle(xmm1, xmm1, 0xb1);
                    xmm0 = Sse.Multiply(xmm0, xmm1);
                    xmm0 = Sse3.AddSubtract(xmm2, xmm0);
                    xmm2 = Sse.Add(xmm3, xmm0);
                    xmm1 = Sse.Subtract(xmm3, xmm0);
                    Unsafe.As<ComplexF, double>(ref pA) = xmm2.AsDouble().GetElement(0);
                    Unsafe.As<ComplexF, double>(ref pB) = xmm1.AsDouble().GetElement(0);
                    return;
                }
                Fallback.PerformSingleOperationFallback(ref pA, ref pB, om);
            }
            #endregion

            #region ExpandCache

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ExpandCacheX86(Span<ComplexF> span, FftMode mode)
            {
                if (Avx2.IsSupported)
                {
                    ExpandCacheAvx2(span, mode);
                    return;
                }

                if (Avx.IsSupported)
                {
                    ExpandCacheAvx(span, mode);
                    return;
                }
                if (Sse.IsSupported)
                {
                    ExpandCacheSse(span, mode);
                    return;
                }
                Fallback.ExpandCacheFallback(span, mode);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ExpandCacheAvx2(Span<ComplexF> span, FftMode mode)
            {
                var order = MathI.LogBase2((uint)span.Length) + 1;
                nint i = 0, length = span.Length >> 1;
                var v2m = (ComplexF)GetValueToMultiply(order);
                v2m = mode == FftMode.Forward ? ComplexF.Conjugate(v2m) : v2m;
                var ymm15 = Vector256.Create(v2m.Real);
                var ymm14 = Vector256.Create(v2m.Imaginary);
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(span));
                ref var rsi = ref Unsafe.Add(ref rdi, length);
                var olen = length - 15;
                for (; i < olen; i += 16)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var ymm0 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector256<double>.Count));
                    var ymm1 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector256<double>.Count));
                    var ymm2 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 2 * Vector256<double>.Count));
                    var ymm3 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 3 * Vector256<double>.Count));
                    var ymm4 = Avx.Permute(ymm0, 0b10_11_00_01);
                    var ymm8 = Avx.Multiply(ymm0, ymm15);
                    ymm0 = Avx2.Permute4x64(ymm0.AsDouble(), 0b01_11_00_10).AsSingle();
                    var ymm5 = Avx.Permute(ymm1, 0b10_11_00_01);
                    var ymm9 = Avx.Multiply(ymm1, ymm15);
                    ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 0b01_11_00_10).AsSingle();
                    var ymm6 = Avx.Permute(ymm2, 0b10_11_00_01);
                    var ymm10 = Avx.Multiply(ymm2, ymm15);
                    ymm2 = Avx2.Permute4x64(ymm2.AsDouble(), 0b01_11_00_10).AsSingle();
                    var ymm7 = Avx.Permute(ymm3, 0b10_11_00_01);
                    var ymm11 = Avx.Multiply(ymm3, ymm15);
                    ymm3 = Avx2.Permute4x64(ymm3.AsDouble(), 0b01_11_00_10).AsSingle();
                    ymm4 = Avx.Multiply(ymm4, ymm14);
                    ymm8 = Avx.AddSubtract(ymm8, ymm4);
                    ymm5 = Avx.Multiply(ymm5, ymm14);
                    ymm9 = Avx.AddSubtract(ymm9, ymm5);
                    ymm6 = Avx.Multiply(ymm6, ymm14);
                    ymm10 = Avx.AddSubtract(ymm10, ymm6);
                    ymm7 = Avx.Multiply(ymm7, ymm14);
                    ymm11 = Avx.AddSubtract(ymm11, ymm7);
                    ymm8 = Avx2.Permute4x64(ymm8.AsDouble(), 0b11_01_10_00).AsSingle();
                    ymm9 = Avx2.Permute4x64(ymm9.AsDouble(), 0b11_01_10_00).AsSingle();
                    ymm10 = Avx2.Permute4x64(ymm10.AsDouble(), 0b11_01_10_00).AsSingle();
                    ymm11 = Avx2.Permute4x64(ymm11.AsDouble(), 0b11_01_10_00).AsSingle();
                    ymm4 = Avx.Blend(ymm0.AsDouble(), ymm8.AsDouble(), 0b1010).AsSingle();
                    ymm0 = Avx.Shuffle(ymm0.AsDouble(), ymm8.AsDouble(), 0b0101).AsSingle();
                    ymm5 = Avx.Blend(ymm1.AsDouble(), ymm9.AsDouble(), 0b1010).AsSingle();
                    ymm1 = Avx.Shuffle(ymm1.AsDouble(), ymm9.AsDouble(), 0b0101).AsSingle();
                    ymm6 = Avx.Blend(ymm2.AsDouble(), ymm10.AsDouble(), 0b1010).AsSingle();
                    ymm2 = Avx.Shuffle(ymm2.AsDouble(), ymm10.AsDouble(), 0b0101).AsSingle();
                    ymm7 = Avx.Blend(ymm3.AsDouble(), ymm11.AsDouble(), 0b1010).AsSingle();
                    ymm3 = Avx.Shuffle(ymm3.AsDouble(), ymm11.AsDouble(), 0b0101).AsSingle();
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 0 * Vector256<double>.Count)) = ymm0;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 1 * Vector256<double>.Count)) = ymm4;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 2 * Vector256<double>.Count)) = ymm1;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 3 * Vector256<double>.Count)) = ymm5;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 4 * Vector256<double>.Count)) = ymm2;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 5 * Vector256<double>.Count)) = ymm6;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 6 * Vector256<double>.Count)) = ymm3;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 7 * Vector256<double>.Count)) = ymm7;
                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector128<double>.Count));
                    var xmm1 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector128<double>.Count));
                    var xmm2 = Avx.Permute(xmm0, 0b10_11_00_01);
                    var xmm3 = Avx.Permute(xmm1, 0b10_11_00_01);
                    var xmm4 = Sse.Multiply(xmm0, ymm15.GetLower());
                    var xmm5 = Sse.Multiply(xmm1, ymm15.GetLower());
                    xmm2 = Sse.Multiply(xmm2, ymm14.GetLower());
                    xmm2 = Sse3.AddSubtract(xmm4, xmm2);
                    xmm3 = Sse.Multiply(xmm3, ymm14.GetLower());
                    xmm3 = Sse3.AddSubtract(xmm5, xmm3);
                    xmm4 = Sse2.UnpackLow(xmm0.AsDouble(), xmm2.AsDouble()).AsSingle();
                    xmm0 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm2.AsDouble()).AsSingle();
                    xmm5 = Sse2.UnpackLow(xmm1.AsDouble(), xmm3.AsDouble()).AsSingle();
                    xmm1 = Sse2.UnpackHigh(xmm1.AsDouble(), xmm3.AsDouble()).AsSingle();
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm4;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 2 * Vector128<double>.Count)) = xmm5;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 3 * Vector128<double>.Count)) = xmm1;
                }
                for (; i < length; i++)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i)).AsSingle();
                    var xmm1 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm2 = Sse.Multiply(xmm0, ymm15.GetLower());
                    xmm1 = Sse.Multiply(xmm1, ymm14.GetLower());
                    xmm1 = Sse3.AddSubtract(xmm2, xmm1);
                    xmm0 = Sse.Shuffle(xmm0, xmm1, 0b01_00_01_00);
                    Unsafe.As<double, Vector128<float>>(ref r8) = xmm0;
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ExpandCacheAvx(Span<ComplexF> span, FftMode mode)
            {
                var order = MathI.LogBase2((uint)span.Length) + 1;
                nint i = 0, length = span.Length >> 1;
                var v2m = (ComplexF)GetValueToMultiply(order);
                v2m = mode == FftMode.Forward ? ComplexF.Conjugate(v2m) : v2m;
                var ymm15 = Vector256.Create(v2m.Real);
                var ymm14 = Vector256.Create(v2m.Imaginary);
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(span));
                ref var rsi = ref Unsafe.Add(ref rdi, length);
                var olen = length - 15;
                for (; i < olen; i += 16)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var ymm0 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector256<double>.Count));
                    var ymm1 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector256<double>.Count));
                    var ymm2 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 2 * Vector256<double>.Count));
                    var ymm3 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 3 * Vector256<double>.Count));
                    var ymm4 = Avx.Permute(ymm0, 0b10_11_00_01);
                    var ymm8 = Avx.Multiply(ymm0, ymm15);
                    var ymm5 = Avx.Permute(ymm1, 0b10_11_00_01);
                    var ymm9 = Avx.Multiply(ymm1, ymm15);
                    var ymm6 = Avx.Permute(ymm2, 0b10_11_00_01);
                    var ymm10 = Avx.Multiply(ymm2, ymm15);
                    var ymm7 = Avx.Permute(ymm3, 0b10_11_00_01);
                    var ymm11 = Avx.Multiply(ymm3, ymm15);
                    ymm4 = Avx.Multiply(ymm4, ymm14);
                    ymm8 = Avx.AddSubtract(ymm8, ymm4);
                    ymm5 = Avx.Multiply(ymm5, ymm14);
                    ymm9 = Avx.AddSubtract(ymm9, ymm5);
                    ymm6 = Avx.Multiply(ymm6, ymm14);
                    ymm10 = Avx.AddSubtract(ymm10, ymm6);
                    ymm7 = Avx.Multiply(ymm7, ymm14);
                    ymm11 = Avx.AddSubtract(ymm11, ymm7);
                    ymm4 = Avx.Shuffle(ymm0.AsDouble(), ymm8.AsDouble(), 0xff).AsSingle();
                    ymm0 = Avx.Shuffle(ymm0.AsDouble(), ymm8.AsDouble(), 0x00).AsSingle();
                    ymm5 = Avx.Shuffle(ymm1.AsDouble(), ymm9.AsDouble(), 0xff).AsSingle();
                    ymm1 = Avx.Shuffle(ymm1.AsDouble(), ymm9.AsDouble(), 0x00).AsSingle();
                    ymm6 = Avx.Shuffle(ymm2.AsDouble(), ymm10.AsDouble(), 0xff).AsSingle();
                    ymm2 = Avx.Shuffle(ymm2.AsDouble(), ymm10.AsDouble(), 0x00).AsSingle();
                    ymm7 = Avx.Shuffle(ymm3.AsDouble(), ymm11.AsDouble(), 0xff).AsSingle();
                    ymm3 = Avx.Shuffle(ymm3.AsDouble(), ymm11.AsDouble(), 0x00).AsSingle();
                    ymm8 = Avx.Permute2x128(ymm0, ymm4, 0x20);
                    ymm0 = Avx.Permute2x128(ymm0, ymm4, 0x31);
                    ymm9 = Avx.Permute2x128(ymm1, ymm5, 0x20);
                    ymm1 = Avx.Permute2x128(ymm1, ymm5, 0x31);
                    ymm10 = Avx.Permute2x128(ymm2, ymm6, 0x20);
                    ymm2 = Avx.Permute2x128(ymm2, ymm6, 0x31);
                    ymm11 = Avx.Permute2x128(ymm3, ymm7, 0x20);
                    ymm3 = Avx.Permute2x128(ymm3, ymm7, 0x31);
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 0 * Vector256<double>.Count)) = ymm8;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 1 * Vector256<double>.Count)) = ymm0;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 2 * Vector256<double>.Count)) = ymm9;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 3 * Vector256<double>.Count)) = ymm1;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 4 * Vector256<double>.Count)) = ymm10;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 5 * Vector256<double>.Count)) = ymm2;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 6 * Vector256<double>.Count)) = ymm11;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref r8, 7 * Vector256<double>.Count)) = ymm3;
                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector128<double>.Count));
                    var xmm1 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector128<double>.Count));
                    var xmm2 = Avx.Permute(xmm0, 0b10_11_00_01);
                    var xmm3 = Avx.Permute(xmm1, 0b10_11_00_01);
                    var xmm4 = Sse.Multiply(xmm0, ymm15.GetLower());
                    var xmm5 = Sse.Multiply(xmm1, ymm15.GetLower());
                    xmm2 = Sse.Multiply(xmm2, ymm14.GetLower());
                    xmm2 = Sse3.AddSubtract(xmm4, xmm2);
                    xmm3 = Sse.Multiply(xmm3, ymm14.GetLower());
                    xmm3 = Sse3.AddSubtract(xmm5, xmm3);
                    xmm4 = Sse2.UnpackLow(xmm0.AsDouble(), xmm2.AsDouble()).AsSingle();
                    xmm0 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm2.AsDouble()).AsSingle();
                    xmm5 = Sse2.UnpackLow(xmm1.AsDouble(), xmm3.AsDouble()).AsSingle();
                    xmm1 = Sse2.UnpackHigh(xmm1.AsDouble(), xmm3.AsDouble()).AsSingle();
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm4;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 2 * Vector128<double>.Count)) = xmm5;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 3 * Vector128<double>.Count)) = xmm1;
                }
                for (; i < length; i++)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i)).AsSingle();
                    var xmm1 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm2 = Sse.Multiply(xmm0, ymm15.GetLower());
                    xmm1 = Sse.Multiply(xmm1, ymm14.GetLower());
                    xmm1 = Sse3.AddSubtract(xmm2, xmm1);
                    xmm0 = Sse.Shuffle(xmm0, xmm1, 0b01_00_01_00);
                    Unsafe.As<double, Vector128<float>>(ref r8) = xmm0;
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ExpandCacheSse3(Span<ComplexF> span, FftMode mode)
            {
                var order = MathI.LogBase2((uint)span.Length) + 1;
                nint i = 0, length = span.Length >> 1;
                var v2m = (ComplexF)GetValueToMultiply(order);
                v2m = mode == FftMode.Forward ? ComplexF.Conjugate(v2m) : v2m;
                var xmm15 = Vector128.Create(v2m.Real);
                var xmm14 = Vector128.Create(v2m.Imaginary);
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(span));
                ref var rsi = ref Unsafe.Add(ref rdi, length);
                var olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector128<double>.Count));
                    var xmm1 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector128<double>.Count));
                    var xmm2 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm3 = Sse.Shuffle(xmm1, xmm1, 0b10_11_00_01);
                    var xmm4 = Sse.Multiply(xmm0, xmm15);
                    var xmm5 = Sse.Multiply(xmm1, xmm15);
                    xmm2 = Sse.Multiply(xmm2, xmm14);
                    xmm4 = Sse3.AddSubtract(xmm4, xmm2);
                    xmm3 = Sse.Multiply(xmm3, xmm14);
                    xmm5 = Sse3.AddSubtract(xmm5, xmm3);
                    xmm2 = Sse2.UnpackLow(xmm0.AsDouble(), xmm4.AsDouble()).AsSingle();
                    xmm0 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm4.AsDouble()).AsSingle();
                    xmm3 = Sse2.UnpackLow(xmm1.AsDouble(), xmm5.AsDouble()).AsSingle();
                    xmm1 = Sse2.UnpackHigh(xmm1.AsDouble(), xmm5.AsDouble()).AsSingle();
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm2;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 2 * Vector128<double>.Count)) = xmm3;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 3 * Vector128<double>.Count)) = xmm1;
                }
                for (; i < length; i++)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i)).AsSingle();
                    var xmm1 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm2 = Sse.Multiply(xmm0, xmm15);
                    xmm1 = Sse.Multiply(xmm1, xmm14);
                    xmm2 = Sse3.AddSubtract(xmm2, xmm1);
                    xmm0 = Sse.Shuffle(xmm0, xmm2, 0b01_00_01_00);
                    Unsafe.As<double, Vector128<float>>(ref r8) = xmm0;
                }
            }
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ExpandCacheSse2(Span<ComplexF> span, FftMode mode)
            {
                var order = MathI.LogBase2((uint)span.Length) + 1;
                nint i = 0, length = span.Length >> 1;
                var v2m = (ComplexF)GetValueToMultiply(order);
                v2m = mode == FftMode.Forward ? ComplexF.Conjugate(v2m) : v2m;
                var xmm15 = Vector128.Create(v2m.Real);
                var xmm14 = Vector128.Create(v2m.Imaginary);
                xmm14 = Sse.Xor(xmm14, Vector128.Create(0x8000_0000u, 0, 0x8000_0000u, 0).AsSingle());
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(span));
                ref var rsi = ref Unsafe.Add(ref rdi, length);
                var olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector128<double>.Count));
                    var xmm1 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector128<double>.Count));
                    var xmm2 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm3 = Sse.Shuffle(xmm1, xmm1, 0b10_11_00_01);
                    var xmm4 = Sse.Multiply(xmm0, xmm15);
                    var xmm5 = Sse.Multiply(xmm1, xmm15);
                    xmm2 = Sse.Multiply(xmm2, xmm14);
                    xmm4 = Sse.Add(xmm4, xmm2);
                    xmm3 = Sse.Multiply(xmm3, xmm14);
                    xmm5 = Sse.Add(xmm5, xmm3);
                    xmm2 = Sse2.UnpackLow(xmm0.AsDouble(), xmm4.AsDouble()).AsSingle();
                    xmm0 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm4.AsDouble()).AsSingle();
                    xmm3 = Sse2.UnpackLow(xmm1.AsDouble(), xmm5.AsDouble()).AsSingle();
                    xmm1 = Sse2.UnpackHigh(xmm1.AsDouble(), xmm5.AsDouble()).AsSingle();
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm2;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 2 * Vector128<double>.Count)) = xmm3;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 3 * Vector128<double>.Count)) = xmm1;
                }
                for (; i < length; i++)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i)).AsSingle();
                    var xmm1 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm2 = Sse.Multiply(xmm0, xmm15);
                    xmm1 = Sse.Multiply(xmm1, xmm14);
                    xmm2 = Sse.Add(xmm2, xmm1);
                    xmm0 = Sse.Shuffle(xmm0, xmm2, 0b01_00_01_00);
                    Unsafe.As<double, Vector128<float>>(ref r8) = xmm0;
                }
            }
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ExpandCacheSse(Span<ComplexF> span, FftMode mode)
            {
                var order = MathI.LogBase2((uint)span.Length) + 1;
                nint i = 0, length = span.Length >> 1;
                var v2m = (ComplexF)GetValueToMultiply(order);
                v2m = mode == FftMode.Forward ? ComplexF.Conjugate(v2m) : v2m;
                var xmm15 = Vector128.Create(v2m.Real);
                var xmm14 = Vector128.Create(v2m.Imaginary);
                xmm14 = Sse.Xor(xmm14, Vector128.Create(0x8000_0000u, 0, 0x8000_0000u, 0).AsSingle());
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(span));
                ref var rsi = ref Unsafe.Add(ref rdi, length);
                var olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector128<double>.Count));
                    var xmm1 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector128<double>.Count));
                    var xmm2 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm3 = Sse.Shuffle(xmm1, xmm1, 0b10_11_00_01);
                    var xmm4 = Sse.Multiply(xmm0, xmm15);
                    var xmm5 = Sse.Multiply(xmm1, xmm15);
                    xmm2 = Sse.Multiply(xmm2, xmm14);
                    xmm4 = Sse.Add(xmm4, xmm2);
                    xmm3 = Sse.Multiply(xmm3, xmm14);
                    xmm5 = Sse.Add(xmm5, xmm3);
                    xmm2 = Sse.Shuffle(xmm0, xmm4, 0b01_00_01_00);
                    xmm0 = Sse.Shuffle(xmm0, xmm4, 0b11_10_11_10);
                    xmm3 = Sse.Shuffle(xmm1, xmm5, 0b01_00_01_00);
                    xmm1 = Sse.Shuffle(xmm1, xmm5, 0b11_10_11_10);
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 0 * Vector128<double>.Count)) = xmm2;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 1 * Vector128<double>.Count)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 2 * Vector128<double>.Count)) = xmm3;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref r8, 3 * Vector128<double>.Count)) = xmm1;
                }
                for (; i < length; i++)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i)).AsSingle();
                    var xmm1 = Sse.Shuffle(xmm0, xmm0, 0b10_11_00_01);
                    var xmm2 = Sse.Multiply(xmm0, xmm15);
                    xmm1 = Sse.Multiply(xmm1, xmm14);
                    xmm2 = Sse.Add(xmm2, xmm1);
                    xmm0 = Sse.Shuffle(xmm0, xmm2, 0b01_00_01_00);
                    Unsafe.As<double, Vector128<float>>(ref r8) = xmm0;
                }
            }
            #endregion
        }
    }
}
#endif