#if NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils.Intrinsics;

namespace Shamisen.Utils
{
    public static partial class AudioUtils
    {
        internal static partial class X86
        {
            internal static bool IsSupported => Sse.IsSupported;

            #region Interleave

            #region Stereo

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                if (Avx.IsSupported && MathI.Min(buffer.Length / 2, MathI.Min(left.Length, right.Length)) >= 128)
                {
                    InterleaveStereoInt32Avx(buffer, left, right);
                    return;
                }
                if (Sse.IsSupported)
                {
                    InterleaveStereoInt32Sse(buffer, left, right);
                    return;
                }
                Fallback.InterleaveStereoInt32(buffer, left, right);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoInt32Avx(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                if (left.Length < Vector256<int>.Count)
                {
                    Fallback.InterleaveStereoInt32(buffer, left, right);
                    return;
                }
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                ref var r8 = ref MemoryMarshal.GetReference(left);
                ref var r9 = ref MemoryMarshal.GetReference(right);
                nint i = 0, j = 0, length = MathI.Min(buffer.Length / 2, MathI.Min(left.Length, right.Length));
                var olen = length - 31;
                for (; i < olen; i += 32, j += 64)
                {
                    var xmm0 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i));
                    var xmm2 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i + 4));
                    var xmm4 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i + 8));
                    var xmm6 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i + 12));
                    var xmm1 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i));
                    var xmm3 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i + 4));
                    var xmm5 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i + 8));
                    var xmm7 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i + 12));
                    var xmm8 = Sse.UnpackLow(xmm0, xmm1);
                    var xmm9 = Sse.UnpackHigh(xmm0, xmm1);
                    var xmm10 = Sse.UnpackLow(xmm2, xmm3);
                    var xmm11 = Sse.UnpackHigh(xmm2, xmm3);
                    var xmm12 = Sse.UnpackLow(xmm4, xmm5);
                    var xmm13 = Sse.UnpackHigh(xmm4, xmm5);
                    var xmm14 = Sse.UnpackLow(xmm6, xmm7);
                    var xmm15 = Sse.UnpackHigh(xmm6, xmm7);
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 0)) = xmm8;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 4)) = xmm9;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 8)) = xmm10;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 12)) = xmm11;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 16)) = xmm12;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 20)) = xmm13;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 24)) = xmm14;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 28)) = xmm15;
                    xmm0 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i + 16));
                    xmm2 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i + 20));
                    xmm4 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i + 24));
                    xmm6 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i + 28));
                    xmm1 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i + 16));
                    xmm3 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i + 20));
                    xmm5 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i + 24));
                    xmm7 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i + 28));
                    xmm8 = Sse.UnpackLow(xmm0, xmm1);
                    xmm9 = Sse.UnpackHigh(xmm0, xmm1);
                    xmm10 = Sse.UnpackLow(xmm2, xmm3);
                    xmm11 = Sse.UnpackHigh(xmm2, xmm3);
                    xmm12 = Sse.UnpackLow(xmm4, xmm5);
                    xmm13 = Sse.UnpackHigh(xmm4, xmm5);
                    xmm14 = Sse.UnpackLow(xmm6, xmm7);
                    xmm15 = Sse.UnpackHigh(xmm6, xmm7);
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 32)) = xmm8;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 36)) = xmm9;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 40)) = xmm10;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 44)) = xmm11;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 48)) = xmm12;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 52)) = xmm13;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 56)) = xmm14;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 60)) = xmm15;
                }
                olen = length - 3;
                for (; i < olen; i += 4, j += 8)
                {
                    var ymm0 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i));
                    var ymm1 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i));
                    var ymm4 = Sse.UnpackLow(ymm0, ymm1);
                    var ymm5 = Sse.UnpackHigh(ymm0, ymm1);
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 0)) = ymm4;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 4)) = ymm5;
                }
                for (; i < length; i += 1, j += 2)
                {
                    var a = Unsafe.Add(ref r8, i);
                    Unsafe.Add(ref rdi, j) = a;
                    a = Unsafe.Add(ref r9, i);
                    Unsafe.Add(ref Unsafe.Add(ref rdi, j), 1) = a;
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoInt32Sse(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                if (left.Length < Vector128<int>.Count)
                {
                    Fallback.InterleaveStereoInt32(buffer, left, right);
                    return;
                }
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                ref var r8 = ref MemoryMarshal.GetReference(left);
                ref var r9 = ref MemoryMarshal.GetReference(right);
                nint i = 0, j = 0, length = MathI.Min(buffer.Length / 2, MathI.Min(left.Length, right.Length));
                var olen = length - 7;
                for (; i < olen; i += 8, j += 16)
                {
                    var xmm0 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i));
                    var xmm1 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i));
                    var xmm4 = Sse.UnpackLow(xmm0, xmm1);
                    var xmm5 = Sse.UnpackHigh(xmm0, xmm1);
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 0)) = xmm4;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 4)) = xmm5;
                    xmm0 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r8, i + 4));
                    xmm1 = Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref r9, i + 4));
                    xmm4 = Sse.UnpackLow(xmm0, xmm1);
                    xmm5 = Sse.UnpackHigh(xmm0, xmm1);
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 8)) = xmm4;
                    Unsafe.As<int, Vector128<float>>(ref Unsafe.Add(ref rdi, j + 12)) = xmm5;
                }
                for (; i < length; i += 1, j += 2)
                {
                    var a = Unsafe.Add(ref r8, i);
                    Unsafe.Add(ref rdi, j) = a;
                    a = Unsafe.Add(ref r9, i);
                    Unsafe.Add(ref Unsafe.Add(ref rdi, j), 1) = a;
                }
            }

            #endregion Stereo

            #region Three

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveThreeInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
            {
                if (Avx2.IsSupported)
                {
                    InterleaveThreeInt32Avx2(buffer, left, right, center);
                    return;
                }
                if (Sse41.IsSupported)
                {
                    InterleaveThreeInt32Sse41(buffer, left, right, center);
                    return;
                }
                Fallback.InterleaveThreeInt32(buffer, left, right, center);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveThreeInt32Avx2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
            {
                unsafe
                {
                    nint i = 0, j = 0, length = MathI.Min(buffer.Length / 2, MathI.Min(left.Length, right.Length));
                    if (left.Length < Vector256<int>.Count)
                    {
                        Fallback.InterleaveThreeInt32(buffer, left, right, center);
                        return;
                    }
                    ref var rL = ref MemoryMarshal.GetReference(left);
                    ref var rR = ref MemoryMarshal.GetReference(right);
                    ref var rC = ref MemoryMarshal.GetReference(center);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    var ymm13 = Vector256.Create(0, 0, 0, 1, 1, 1, 2, 2);
                    var ymm14 = Vector256.Create(2, 3, 3, 3, 4, 4, 4, 5);
                    var ymm15 = Vector256.Create(5, 5, 6, 6, 6, 7, 7, 7);
                    var olen = length - 31;
                    for (; i < olen; i += 32, j += 96)
                    {
                        var ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rL, i));
                        var ymm5 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rR, i));
                        var ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rC, i));
                        var ymm0 = Avx2.PermuteVar8x32(ymm2, ymm13);    //00011122
                        var ymm3 = Avx2.PermuteVar8x32(ymm5, ymm13);    //888999aa
                        var ymm1 = Avx2.PermuteVar8x32(ymm2, ymm14);    //23334445
                        var ymm4 = Avx2.PermuteVar8x32(ymm5, ymm14);    //abbbcccd
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);        //55666777
                        ymm5 = Avx2.PermuteVar8x32(ymm5, ymm15);        //ddeeefff
                        ymm0 = Avx.Blend(ymm0, ymm3, 0b10110110);
                        var ymm6 = Avx2.PermuteVar8x32(ymm8, ymm13);    //ggghhhii
                        ymm1 = Avx.Blend(ymm1, ymm4, 0b00100100);
                        var ymm7 = Avx2.PermuteVar8x32(ymm8, ymm14);    //ijjjkkkl
                        ymm2 = Avx.Blend(ymm2, ymm5, 0b11011011);
                        ymm8 = Avx2.PermuteVar8x32(ymm8, ymm15);        //llmmmnnn
                        ymm0 = Avx.Blend(ymm0, ymm6, 0b00100100);
                        ymm3 = Avx.Blend(ymm1, ymm7, 0b01001001);
                        ymm6 = Avx.Blend(ymm2, ymm8, 0b10010010);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 0)) = ymm0;
                        ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rL, i + 8));
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 8)) = ymm3;
                        ymm5 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rR, i + 8));
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 16)) = ymm6;
                        ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rC, i + 8));
                        ymm0 = Avx2.PermuteVar8x32(ymm2, ymm13);        //00011122
                        ymm3 = Avx2.PermuteVar8x32(ymm5, ymm13);        //888999aa
                        ymm1 = Avx2.PermuteVar8x32(ymm2, ymm14);        //23334445
                        ymm4 = Avx2.PermuteVar8x32(ymm5, ymm14);        //abbbcccd
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);        //55666777
                        ymm5 = Avx2.PermuteVar8x32(ymm5, ymm15);        //ddeeefff
                        ymm0 = Avx.Blend(ymm0, ymm3, 0b10110110);
                        ymm6 = Avx2.PermuteVar8x32(ymm8, ymm13);        //ggghhhii
                        ymm1 = Avx.Blend(ymm1, ymm4, 0b00100100);
                        ymm7 = Avx2.PermuteVar8x32(ymm8, ymm14);        //ijjjkkkl
                        ymm2 = Avx.Blend(ymm2, ymm5, 0b11011011);
                        ymm8 = Avx2.PermuteVar8x32(ymm8, ymm15);        //llmmmnnn
                        ymm0 = Avx.Blend(ymm0, ymm6, 0b00100100);
                        ymm3 = Avx.Blend(ymm1, ymm7, 0b01001001);
                        ymm6 = Avx.Blend(ymm2, ymm8, 0b10010010);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 24)) = ymm0;
                        ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rL, i + 16));
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 32)) = ymm3;
                        ymm5 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rR, i + 16));
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 40)) = ymm6;
                        ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rC, i + 16));
                        ymm0 = Avx2.PermuteVar8x32(ymm2, ymm13);        //00011122
                        ymm3 = Avx2.PermuteVar8x32(ymm5, ymm13);        //888999aa
                        ymm1 = Avx2.PermuteVar8x32(ymm2, ymm14);        //23334445
                        ymm4 = Avx2.PermuteVar8x32(ymm5, ymm14);        //abbbcccd
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);        //55666777
                        ymm5 = Avx2.PermuteVar8x32(ymm5, ymm15);        //ddeeefff
                        ymm0 = Avx.Blend(ymm0, ymm3, 0b10110110);
                        ymm6 = Avx2.PermuteVar8x32(ymm8, ymm13);        //ggghhhii
                        ymm1 = Avx.Blend(ymm1, ymm4, 0b00100100);
                        ymm7 = Avx2.PermuteVar8x32(ymm8, ymm14);        //ijjjkkkl
                        ymm2 = Avx.Blend(ymm2, ymm5, 0b11011011);
                        ymm8 = Avx2.PermuteVar8x32(ymm8, ymm15);        //llmmmnnn
                        ymm0 = Avx.Blend(ymm0, ymm6, 0b00100100);
                        ymm3 = Avx.Blend(ymm1, ymm7, 0b01001001);
                        ymm6 = Avx.Blend(ymm2, ymm8, 0b10010010);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 48)) = ymm0;
                        ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rL, i + 24));
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 56)) = ymm3;
                        ymm5 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rR, i + 24));
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 64)) = ymm6;
                        ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rC, i + 24));
                        ymm0 = Avx2.PermuteVar8x32(ymm2, ymm13);        //00011122
                        ymm3 = Avx2.PermuteVar8x32(ymm5, ymm13);        //888999aa
                        ymm1 = Avx2.PermuteVar8x32(ymm2, ymm14);        //23334445
                        ymm4 = Avx2.PermuteVar8x32(ymm5, ymm14);        //abbbcccd
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);        //55666777
                        ymm5 = Avx2.PermuteVar8x32(ymm5, ymm15);        //ddeeefff
                        ymm0 = Avx.Blend(ymm0, ymm3, 0b10110110);
                        ymm6 = Avx2.PermuteVar8x32(ymm8, ymm13);        //ggghhhii
                        ymm1 = Avx.Blend(ymm1, ymm4, 0b00100100);
                        ymm7 = Avx2.PermuteVar8x32(ymm8, ymm14);        //ijjjkkkl
                        ymm2 = Avx.Blend(ymm2, ymm5, 0b11011011);
                        ymm8 = Avx2.PermuteVar8x32(ymm8, ymm15);        //llmmmnnn
                        ymm0 = Avx.Blend(ymm0, ymm6, 0b00100100);
                        ymm3 = Avx.Blend(ymm1, ymm7, 0b01001001);
                        ymm6 = Avx.Blend(ymm2, ymm8, 0b10010010);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 72)) = ymm0;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 80)) = ymm3;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 88)) = ymm6;
                    }
                    olen = length - 7;
                    for (; i < olen; i += 8, j += 24)
                    {
                        var ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rL, i));
                        var ymm5 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rR, i));
                        var ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rC, i));
                        var ymm0 = Avx2.PermuteVar8x32(ymm2, ymm13);    //00011122
                        var ymm3 = Avx2.PermuteVar8x32(ymm5, ymm13);    //888999aa
                        var ymm1 = Avx2.PermuteVar8x32(ymm2, ymm14);    //23334445
                        var ymm4 = Avx2.PermuteVar8x32(ymm5, ymm14);    //abbbcccd
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);        //55666777
                        ymm5 = Avx2.PermuteVar8x32(ymm5, ymm15);        //ddeeefff
                        ymm0 = Avx.Blend(ymm0, ymm3, 0b10110110);
                        var ymm6 = Avx2.PermuteVar8x32(ymm8, ymm13);    //ggghhhii
                        ymm1 = Avx.Blend(ymm1, ymm4, 0b00100100);
                        var ymm7 = Avx2.PermuteVar8x32(ymm8, ymm14);    //ijjjkkkl
                        ymm2 = Avx.Blend(ymm2, ymm5, 0b11011011);
                        ymm8 = Avx2.PermuteVar8x32(ymm8, ymm15);        //llmmmnnn
                        ymm0 = Avx.Blend(ymm0, ymm6, 0b00100100);
                        ymm3 = Avx.Blend(ymm1, ymm7, 0b01001001);
                        ymm6 = Avx.Blend(ymm2, ymm8, 0b10010010);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 0)) = ymm0;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 8)) = ymm3;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 16)) = ymm6;
                    }
                    for (; i < length; i += 1, j += 3)
                    {
                        var a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        a = Unsafe.Add(ref rC, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveThreeInt32Sse41(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
            {
                unsafe
                {
                    nint i = 0, j = 0, length = MathI.Min(buffer.Length / 2, MathI.Min(left.Length, right.Length));
                    if (left.Length < Vector256<int>.Count)
                    {
                        Fallback.InterleaveThreeInt32(buffer, left, right, center);
                        return;
                    }
                    ref var rL = ref MemoryMarshal.GetReference(left);
                    ref var rR = ref MemoryMarshal.GetReference(right);
                    ref var rC = ref MemoryMarshal.GetReference(center);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    var olen = length - 7;
                    for (; i < olen; i += 8, j += 24)
                    {
                        var xmm0 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rL, i));
                        var xmm1 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rR, i));
                        var xmm2 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rC, i));
                        var xmm3 = Sse2.Shuffle(xmm1, 80);
                        var xmm4 = Sse2.Shuffle(xmm0, 68);
                        xmm4 = Sse41.Blend(xmm4.AsInt16(), xmm3.AsInt16(), 12).AsInt32();
                        xmm3 = Sse2.Shuffle(xmm2, 68);
                        xmm3 = Sse41.Blend(xmm3.AsInt16(), xmm4.AsInt16(), 207).AsInt32();
                        xmm4 = Sse2.Shuffle(xmm1, 165);
                        xmm4 = Sse41.Blend(xmm4.AsInt16(), xmm2.AsInt16(), 12).AsInt32();
                        xmm4 = Sse41.Blend(xmm4.AsInt16(), xmm0.AsInt16(), 48).AsInt32();
                        xmm0 = Sse2.Shuffle(xmm0, 238);
                        xmm2 = Sse2.Shuffle(xmm2, 238);
                        xmm2 = Sse41.Blend(xmm2.AsInt16(), xmm0.AsInt16(), 12).AsInt32();
                        xmm0 = Sse2.Shuffle(xmm1, 250);
                        xmm0 = Sse41.Blend(xmm0.AsInt16(), xmm2.AsInt16(), 207).AsInt32();
                        Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, j + 8)) = xmm0;
                        Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, j + 4)) = xmm4;
                        Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, j + 0)) = xmm3;
                        xmm0 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rL, i + 8));
                        xmm1 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rR, i + 8));
                        xmm2 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rC, i + 8));
                        xmm3 = Sse2.Shuffle(xmm1, 80);
                        xmm4 = Sse2.Shuffle(xmm0, 68);
                        xmm4 = Sse41.Blend(xmm4.AsInt16(), xmm3.AsInt16(), 12).AsInt32();
                        xmm3 = Sse2.Shuffle(xmm2, 68);
                        xmm3 = Sse41.Blend(xmm3.AsInt16(), xmm4.AsInt16(), 207).AsInt32();
                        xmm4 = Sse2.Shuffle(xmm1, 165);
                        xmm4 = Sse41.Blend(xmm4.AsInt16(), xmm2.AsInt16(), 12).AsInt32();
                        xmm4 = Sse41.Blend(xmm4.AsInt16(), xmm0.AsInt16(), 48).AsInt32();
                        xmm0 = Sse2.Shuffle(xmm0, 238);
                        xmm2 = Sse2.Shuffle(xmm2, 238);
                        xmm2 = Sse41.Blend(xmm2.AsInt16(), xmm0.AsInt16(), 12).AsInt32();
                        xmm0 = Sse2.Shuffle(xmm1, 250);
                        xmm0 = Sse41.Blend(xmm0.AsInt16(), xmm2.AsInt16(), 207).AsInt32();
                        Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, j + 20)) = xmm0;
                        Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, j + 16)) = xmm4;
                        Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, j + 12)) = xmm3;
                    }
                    olen = length - 3;
                    for (; i < olen; i += 4, j += 12)
                    {
                        var xmm0 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rL, i));
                        var xmm1 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rR, i));
                        var xmm2 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rC, i));
                        var xmm3 = Sse2.Shuffle(xmm1, 80);
                        var xmm4 = Sse2.Shuffle(xmm0, 68);
                        xmm4 = Sse41.Blend(xmm4.AsInt16(), xmm3.AsInt16(), 12).AsInt32();
                        xmm3 = Sse2.Shuffle(xmm2, 68);
                        xmm3 = Sse41.Blend(xmm3.AsInt16(), xmm4.AsInt16(), 207).AsInt32();
                        xmm4 = Sse2.Shuffle(xmm1, 165);
                        xmm4 = Sse41.Blend(xmm4.AsInt16(), xmm2.AsInt16(), 12).AsInt32();
                        xmm4 = Sse41.Blend(xmm4.AsInt16(), xmm0.AsInt16(), 48).AsInt32();
                        xmm0 = Sse2.Shuffle(xmm0, 238);
                        xmm2 = Sse2.Shuffle(xmm2, 238);
                        xmm2 = Sse41.Blend(xmm2.AsInt16(), xmm0.AsInt16(), 12).AsInt32();
                        xmm0 = Sse2.Shuffle(xmm1, 250);
                        xmm0 = Sse41.Blend(xmm0.AsInt16(), xmm2.AsInt16(), 207).AsInt32();
                        Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, j + 8)) = xmm0;
                        Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, j + 4)) = xmm4;
                        Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, j + 0)) = xmm3;
                    }
                    for (; i < length; i += 1, j += 3)
                    {
                        var a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        a = Unsafe.Add(ref rC, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                    }
                }
            }

            #endregion Three

            #region Quad

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveQuadInt32(Span<int> buffer, ReadOnlySpan<int> frontLeft, ReadOnlySpan<int> frontRight, ReadOnlySpan<int> rearLeft, ReadOnlySpan<int> rearRight)
            {
                if (Avx2.IsSupported)
                {
                    InterleaveQuadInt32Avx2(buffer, frontLeft, frontRight, rearLeft, rearRight);
                    return;
                }
                Fallback.InterleaveQuadInt32(buffer, frontLeft, frontRight, rearLeft, rearRight);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveQuadInt32Avx2(Span<int> buffer, ReadOnlySpan<int> frontLeft, ReadOnlySpan<int> frontRight, ReadOnlySpan<int> rearLeft, ReadOnlySpan<int> rearRight)
            {
                unsafe
                {
                    ref var r8 = ref MemoryMarshal.GetReference(frontLeft);
                    ref var r9 = ref MemoryMarshal.GetReference(frontRight);
                    ref var r10 = ref MemoryMarshal.GetReference(rearLeft);
                    ref var r15 = ref MemoryMarshal.GetReference(rearRight);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = MathI.Min(buffer.Length / 4, MathI.Min(MathI.Min(frontLeft.Length, frontRight.Length), MathI.Min(rearLeft.Length, rearRight.Length)));
                    nint i = 0, j = 0;
                    var olen = length - 31;
                    for (; i < olen; i += 32, j += 128)
                    {
                        var ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r8, i));
                        var ymm1 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r9, i));
                        var ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r10, i));
                        var ymm3 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r15, i));
                        var xmm4 = ymm3.GetLower();
                        var xmm5 = ymm2.GetLower();
                        var xmm6 = Sse.UnpackLow(xmm5, xmm4);
                        var ymm6 = Avx2.Permute4x64(xmm6.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        var xmm7 = ymm1.GetLower();
                        var xmm0 = ymm8.GetLower();
                        var xmm9 = Sse.UnpackLow(xmm0, xmm7);
                        var ymm9 = Avx2.Permute4x64(xmm9.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm6 = Avx.Blend(ymm9, ymm6, 204);
                        xmm4 = Sse.UnpackHigh(xmm5, xmm4);
                        var ymm4 = Avx2.Permute4x64(xmm4.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        xmm0 = Sse.UnpackHigh(xmm0, xmm7);
                        var ymm0 = Avx2.Permute4x64(xmm0.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm0 = Avx.Blend(ymm0, ymm4, 204);
                        ymm4 = Avx.UnpackHigh(ymm2, ymm3);
                        ymm4 = Avx2.Permute4x64(ymm4.AsDouble(), 232).AsSingle();
                        var ymm5 = Avx.UnpackHigh(ymm8, ymm1);
                        ymm5 = Avx2.Permute4x64(ymm5.AsDouble(), 246).AsSingle();
                        ymm4 = Avx.Blend(ymm5, ymm4, 204);
                        ymm2 = Avx.UnpackLow(ymm2, ymm3);
                        ymm2 = Avx2.Permute4x64(ymm2.AsDouble(), 232).AsSingle();
                        ymm1 = Avx.UnpackLow(ymm8, ymm1);
                        ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 246).AsSingle();
                        ymm1 = Avx.Blend(ymm1, ymm2, 204);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 16)) = ymm1;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 24)) = ymm4;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 8)) = ymm0;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 0)) = ymm6;
                        ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r8, i + 8));
                        ymm1 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r9, i + 8));
                        ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r10, i + 8));
                        ymm3 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r15, i + 8));
                        xmm4 = ymm3.GetLower();
                        xmm5 = ymm2.GetLower();
                        xmm6 = Sse.UnpackLow(xmm5, xmm4);
                        ymm6 = Avx2.Permute4x64(xmm6.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        xmm7 = ymm1.GetLower();
                        xmm0 = ymm8.GetLower();
                        xmm9 = Sse.UnpackLow(xmm0, xmm7);
                        ymm9 = Avx2.Permute4x64(xmm9.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm6 = Avx.Blend(ymm9, ymm6, 204);
                        xmm4 = Sse.UnpackHigh(xmm5, xmm4);
                        ymm4 = Avx2.Permute4x64(xmm4.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        xmm0 = Sse.UnpackHigh(xmm0, xmm7);
                        ymm0 = Avx2.Permute4x64(xmm0.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm0 = Avx.Blend(ymm0, ymm4, 204);
                        ymm4 = Avx.UnpackHigh(ymm2, ymm3);
                        ymm4 = Avx2.Permute4x64(ymm4.AsDouble(), 232).AsSingle();
                        ymm5 = Avx.UnpackHigh(ymm8, ymm1);
                        ymm5 = Avx2.Permute4x64(ymm5.AsDouble(), 246).AsSingle();
                        ymm4 = Avx.Blend(ymm5, ymm4, 204);
                        ymm2 = Avx.UnpackLow(ymm2, ymm3);
                        ymm2 = Avx2.Permute4x64(ymm2.AsDouble(), 232).AsSingle();
                        ymm1 = Avx.UnpackLow(ymm8, ymm1);
                        ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 246).AsSingle();
                        ymm1 = Avx.Blend(ymm1, ymm2, 204);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 48)) = ymm1;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 56)) = ymm4;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 40)) = ymm0;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 32)) = ymm6;
                        ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r8, i + 16));
                        ymm1 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r9, i + 16));
                        ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r10, i + 16));
                        ymm3 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r15, i + 16));
                        xmm4 = ymm3.GetLower();
                        xmm5 = ymm2.GetLower();
                        xmm6 = Sse.UnpackLow(xmm5, xmm4);
                        ymm6 = Avx2.Permute4x64(xmm6.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        xmm7 = ymm1.GetLower();
                        xmm0 = ymm8.GetLower();
                        xmm9 = Sse.UnpackLow(xmm0, xmm7);
                        ymm9 = Avx2.Permute4x64(xmm9.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm6 = Avx.Blend(ymm9, ymm6, 204);
                        xmm4 = Sse.UnpackHigh(xmm5, xmm4);
                        ymm4 = Avx2.Permute4x64(xmm4.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        xmm0 = Sse.UnpackHigh(xmm0, xmm7);
                        ymm0 = Avx2.Permute4x64(xmm0.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm0 = Avx.Blend(ymm0, ymm4, 204);
                        ymm4 = Avx.UnpackHigh(ymm2, ymm3);
                        ymm4 = Avx2.Permute4x64(ymm4.AsDouble(), 232).AsSingle();
                        ymm5 = Avx.UnpackHigh(ymm8, ymm1);
                        ymm5 = Avx2.Permute4x64(ymm5.AsDouble(), 246).AsSingle();
                        ymm4 = Avx.Blend(ymm5, ymm4, 204);
                        ymm2 = Avx.UnpackLow(ymm2, ymm3);
                        ymm2 = Avx2.Permute4x64(ymm2.AsDouble(), 232).AsSingle();
                        ymm1 = Avx.UnpackLow(ymm8, ymm1);
                        ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 246).AsSingle();
                        ymm1 = Avx.Blend(ymm1, ymm2, 204);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 80)) = ymm1;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 88)) = ymm4;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 72)) = ymm0;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 64)) = ymm6;
                        ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r8, i + 24));
                        ymm1 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r9, i + 24));
                        ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r10, i + 24));
                        ymm3 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r15, i + 24));
                        xmm4 = ymm3.GetLower();
                        xmm5 = ymm2.GetLower();
                        xmm6 = Sse.UnpackLow(xmm5, xmm4);
                        ymm6 = Avx2.Permute4x64(xmm6.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        xmm7 = ymm1.GetLower();
                        xmm0 = ymm8.GetLower();
                        xmm9 = Sse.UnpackLow(xmm0, xmm7);
                        ymm9 = Avx2.Permute4x64(xmm9.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm6 = Avx.Blend(ymm9, ymm6, 204);
                        xmm4 = Sse.UnpackHigh(xmm5, xmm4);
                        ymm4 = Avx2.Permute4x64(xmm4.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        xmm0 = Sse.UnpackHigh(xmm0, xmm7);
                        ymm0 = Avx2.Permute4x64(xmm0.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm0 = Avx.Blend(ymm0, ymm4, 204);
                        ymm4 = Avx.UnpackHigh(ymm2, ymm3);
                        ymm4 = Avx2.Permute4x64(ymm4.AsDouble(), 232).AsSingle();
                        ymm5 = Avx.UnpackHigh(ymm8, ymm1);
                        ymm5 = Avx2.Permute4x64(ymm5.AsDouble(), 246).AsSingle();
                        ymm4 = Avx.Blend(ymm5, ymm4, 204);
                        ymm2 = Avx.UnpackLow(ymm2, ymm3);
                        ymm2 = Avx2.Permute4x64(ymm2.AsDouble(), 232).AsSingle();
                        ymm1 = Avx.UnpackLow(ymm8, ymm1);
                        ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 246).AsSingle();
                        ymm1 = Avx.Blend(ymm1, ymm2, 204);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 112)) = ymm1;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 120)) = ymm4;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 104)) = ymm0;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 96)) = ymm6;
                    }
                    olen = length - 7;
                    for (; i < olen; i += 8, j += 32)
                    {
                        var ymm8 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r8, i));
                        var ymm1 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r9, i));
                        var ymm2 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r10, i));
                        var ymm3 = Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref r15, i));
                        var xmm4 = ymm3.GetLower();
                        var xmm5 = ymm2.GetLower();
                        var xmm6 = Sse.UnpackLow(xmm5, xmm4);
                        var ymm6 = Avx2.Permute4x64(xmm6.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        var xmm7 = ymm1.GetLower();
                        var xmm0 = ymm8.GetLower();
                        var xmm9 = Sse.UnpackLow(xmm0, xmm7);
                        var ymm9 = Avx2.Permute4x64(xmm9.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm6 = Avx.Blend(ymm9, ymm6, 204);
                        xmm4 = Sse.UnpackHigh(xmm5, xmm4);
                        var ymm4 = Avx2.Permute4x64(xmm4.AsDouble().ToVector256Unsafe(), 96).AsSingle();
                        xmm0 = Sse.UnpackHigh(xmm0, xmm7);
                        var ymm0 = Avx2.Permute4x64(xmm0.AsDouble().ToVector256Unsafe(), 212).AsSingle();
                        ymm0 = Avx.Blend(ymm0, ymm4, 204);
                        ymm4 = Avx.UnpackHigh(ymm2, ymm3);
                        ymm4 = Avx2.Permute4x64(ymm4.AsDouble(), 232).AsSingle();
                        var ymm5 = Avx.UnpackHigh(ymm8, ymm1);
                        ymm5 = Avx2.Permute4x64(ymm5.AsDouble(), 246).AsSingle();
                        ymm4 = Avx.Blend(ymm5, ymm4, 204);
                        ymm2 = Avx.UnpackLow(ymm2, ymm3);
                        ymm2 = Avx2.Permute4x64(ymm2.AsDouble(), 232).AsSingle();
                        ymm1 = Avx.UnpackLow(ymm8, ymm1);
                        ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 246).AsSingle();
                        ymm1 = Avx.Blend(ymm1, ymm2, 204);
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 16)) = ymm1;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 24)) = ymm4;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 8)) = ymm0;
                        Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref rB, j + 0)) = ymm6;
                    }
                    for (; i < length; i += 1, j += 4)
                    {
                        var a = Unsafe.Add(ref r8, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref r9, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        a = Unsafe.Add(ref r10, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                        a = Unsafe.Add(ref r15, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
                    }
                }
            }

            #endregion Quad

            #endregion Interleave

            #region DuplicateMonauralToChannels

            #region Stereo

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralToStereo(Span<float> destination, ReadOnlySpan<float> source)
            {
                if (Avx2.IsSupported)
                {
                    DuplicateMonauralToStereoAvx2(destination, source);
                    return;
                }
                if (Avx.IsSupported)
                {
                    DuplicateMonauralToStereoAvx(destination, source);
                    return;
                }
                if (Sse.IsSupported)
                {
                    DuplicateMonauralToStereoSse(destination, source);
                    return;
                }
                Fallback.DuplicateMonauralToStereo(destination, source);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralToStereoAvx2(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var src = ref MemoryMarshal.GetReference(source);
                ref var dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = Math.Min(source.Length, destination.Length / 2);
                var olen = length - 31;
                var ymm0 = Vector256.Create(0, 0, 1, 1, 2, 2, 3, 3);
                var ymm1 = Vector256.Create(4, 4, 5, 5, 6, 6, 7, 7);
                for (; i < olen; i += 32)
                {
                    var ymm2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0));
                    var ymm3 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 8));
                    var ymm4 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 16));
                    var ymm5 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 24));
                    var ymm6 = Avx2.PermuteVar8x32(ymm2, ymm0);
                    ymm2 = Avx2.PermuteVar8x32(ymm2, ymm1);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, 2 * i + 0)) = ymm6;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, 2 * i + 8)) = ymm2;
                    ymm2 = Avx2.PermuteVar8x32(ymm3, ymm0);
                    ymm3 = Avx2.PermuteVar8x32(ymm3, ymm1);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, 2 * i + 16)) = ymm2;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, 2 * i + 24)) = ymm3;
                    ymm2 = Avx2.PermuteVar8x32(ymm4, ymm0);
                    ymm3 = Avx2.PermuteVar8x32(ymm4, ymm1);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, 2 * i + 32)) = ymm2;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, 2 * i + 40)) = ymm3;
                    ymm2 = Avx2.PermuteVar8x32(ymm5, ymm0);
                    ymm3 = Avx2.PermuteVar8x32(ymm5, ymm1);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, 2 * i + 48)) = ymm2;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, 2 * i + 56)) = ymm3;
                }
                for (; i < length; i++)
                {
                    var h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 2 + 0) = h;
                    Unsafe.Add(ref dst, i * 2 + 1) = h;
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralToStereoAvx(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var src = ref MemoryMarshal.GetReference(source);
                ref var dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = Math.Min(source.Length, destination.Length / 2);
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, i + 0));
                    var xmm2 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, i + 4));
                    var xmm1 = Avx.Permute(xmm0, 0x50);
                    xmm0 = Avx.Permute(xmm0, 0xfa);
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i * 2 + 0)) = xmm1;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i * 2 + 4)) = xmm0;
                    xmm1 = Avx.Permute(xmm2, 0x50);
                    xmm0 = Avx.Permute(xmm2, 0xfa);
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i * 2 + 8)) = xmm1;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i * 2 + 12)) = xmm0;
                }
                for (; i < length; i++)
                {
                    var h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 2 + 0) = h;
                    Unsafe.Add(ref dst, i * 2 + 1) = h;
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralToStereoSse(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var src = ref MemoryMarshal.GetReference(source);
                ref var dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = Math.Min(source.Length, destination.Length / 2);
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, i + 0));
                    var xmm1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, i + 4));
                    var xmm2 = Sse.UnpackLow(xmm0, xmm0);
                    xmm0 = Sse.UnpackHigh(xmm0, xmm0);
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i * 2 + 0)) = xmm0;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i * 2 + 4)) = xmm2;
                    xmm2 = Sse.UnpackLow(xmm1, xmm1);
                    xmm0 = Sse.UnpackHigh(xmm1, xmm1);
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i * 2 + 8)) = xmm0;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i * 2 + 12)) = xmm2;
                }
                for (; i < length; i++)
                {
                    var h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 2 + 0) = h;
                    Unsafe.Add(ref dst, i * 2 + 1) = h;
                }
            }

            #endregion Stereo

            #region Three

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo3Channels(Span<float> destination, ReadOnlySpan<float> source)
            {
                if (Avx2.IsSupported)
                {
                    DuplicateMonauralTo3ChannelsAvx2(destination, source);
                    return;
                }
                if (Sse2.IsSupported)
                {
                    DuplicateMonauralTo3ChannelsSse2(destination, source);
                    return;
                }
                Fallback.DuplicateMonauralTo3Channels(destination, source);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo3ChannelsAvx2(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var src = ref MemoryMarshal.GetReference(source);
                ref var dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(source.Length, destination.Length / 3);
                var olen = length - 63;
                var ymm15 = Vector256.Create(0, 0, 0, 1, 1, 1, 2, 2);
                var ymm14 = Vector256.Create(2, 3, 3, 3, 4, 4, 4, 5);
                var ymm13 = Vector256.Create(5, 5, 6, 6, 6, 7, 7, 7);
                for (; i < olen; i += 64)
                {
                    var v = 2 * i + i;
                    var ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0));
                    var ymm1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 8));
                    var ymm2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 16));
                    var ymm3 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 24));
                    var ymm4 = Avx2.PermuteVar8x32(ymm0, ymm15);
                    var ymm5 = Avx.PermuteVar(ymm0, ymm14);
                    var ymm6 = Avx2.PermuteVar8x32(ymm0, ymm13);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 0)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 8)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 16)) = ymm6;
                    ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 32));
                    ymm4 = Avx2.PermuteVar8x32(ymm1, ymm15);
                    ymm5 = Avx.PermuteVar(ymm1, ymm14);
                    ymm6 = Avx2.PermuteVar8x32(ymm1, ymm13);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 24)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 32)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 40)) = ymm6;
                    ymm1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 40));
                    ymm4 = Avx2.PermuteVar8x32(ymm2, ymm15);
                    ymm5 = Avx.PermuteVar(ymm2, ymm14);
                    ymm6 = Avx2.PermuteVar8x32(ymm2, ymm13);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 48)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 56)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 64)) = ymm6;
                    ymm2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 48));
                    ymm4 = Avx2.PermuteVar8x32(ymm3, ymm15);
                    ymm5 = Avx.PermuteVar(ymm3, ymm14);
                    ymm6 = Avx2.PermuteVar8x32(ymm3, ymm13);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 72)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 80)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 88)) = ymm6;
                    v += 96;
                    ymm3 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 56));
                    ymm4 = Avx2.PermuteVar8x32(ymm0, ymm15);
                    ymm5 = Avx.PermuteVar(ymm0, ymm14);
                    ymm6 = Avx2.PermuteVar8x32(ymm0, ymm13);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 0)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 8)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 16)) = ymm6;
                    ymm4 = Avx2.PermuteVar8x32(ymm1, ymm15);
                    ymm5 = Avx.PermuteVar(ymm1, ymm14);
                    ymm6 = Avx2.PermuteVar8x32(ymm1, ymm13);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 24)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 32)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 40)) = ymm6;
                    ymm4 = Avx2.PermuteVar8x32(ymm2, ymm15);
                    ymm5 = Avx.PermuteVar(ymm2, ymm14);
                    ymm6 = Avx2.PermuteVar8x32(ymm2, ymm13);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 48)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 56)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 64)) = ymm6;
                    ymm4 = Avx2.PermuteVar8x32(ymm3, ymm15);
                    ymm5 = Avx.PermuteVar(ymm3, ymm14);
                    ymm6 = Avx2.PermuteVar8x32(ymm3, ymm13);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 72)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 80)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 88)) = ymm6;
                }
                olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var v = 3 * i;
                    var ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0));
                    var ymm4 = Avx2.PermuteVar8x32(ymm0, ymm15);
                    var ymm5 = Avx.PermuteVar(ymm0, ymm14);
                    var ymm6 = Avx2.PermuteVar8x32(ymm0, ymm13);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 0)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 8)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref dst, v + 16)) = ymm6;
                }
                for (; i < length; i++)
                {
                    var h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 3 + 0) = h;
                    Unsafe.Add(ref dst, i * 3 + 1) = h;
                    Unsafe.Add(ref dst, i * 3 + 2) = h;
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo3ChannelsSse2(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var src = ref MemoryMarshal.GetReference(source);
                ref var dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(source.Length, destination.Length / 3);
                var olen = length - 15;
                for (; i < olen; i += 16)
                {
                    var v = 2 * i + i;
                    var xmm0 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, i + 0));
                    var xmm1 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, i + 4));
                    var xmm4 = Sse2.Shuffle(xmm0, 64);
                    var xmm5 = Sse2.Shuffle(xmm0, 165);
                    var xmm6 = Sse2.Shuffle(xmm0, 254);
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 0)) = xmm4;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 4)) = xmm5;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 8)) = xmm6;
                    xmm4 = Sse2.Shuffle(xmm1, 64);
                    xmm5 = Sse2.Shuffle(xmm1, 165);
                    xmm6 = Sse2.Shuffle(xmm1, 254);
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 12)) = xmm4;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 16)) = xmm5;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 20)) = xmm6;
                    v += 24;
                    xmm0 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, i + 8));
                    xmm1 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, i + 12));
                    xmm4 = Sse2.Shuffle(xmm0, 64);
                    xmm5 = Sse2.Shuffle(xmm0, 165);
                    xmm6 = Sse2.Shuffle(xmm0, 254);
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 0)) = xmm4;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 4)) = xmm5;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 8)) = xmm6;
                    xmm4 = Sse2.Shuffle(xmm1, 64);
                    xmm5 = Sse2.Shuffle(xmm1, 165);
                    xmm6 = Sse2.Shuffle(xmm1, 254);
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 12)) = xmm4;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 16)) = xmm5;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 20)) = xmm6;
                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    var v = 2 * i + i;
                    var xmm0 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, i + 0));
                    var xmm4 = Sse2.Shuffle(xmm0, 64);
                    var xmm5 = Sse2.Shuffle(xmm0, 165);
                    var xmm6 = Sse2.Shuffle(xmm0, 254);
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 0)) = xmm4;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 4)) = xmm5;
                    Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref src, v + 8)) = xmm6;
                }
                for (; i < length; i++)
                {
                    var h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 3 + 0) = h;
                    Unsafe.Add(ref dst, i * 3 + 1) = h;
                    Unsafe.Add(ref dst, i * 3 + 2) = h;
                }
            }

            #endregion Three

            #region Quad

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo4Channels(Span<float> destination, ReadOnlySpan<float> source)
            {
                if (Avx2.IsSupported)
                {
                    DuplicateMonauralTo4ChannelsAvx2(destination, source);
                    return;
                }
                if (Sse2.IsSupported)
                {
                    DuplicateMonauralTo4ChannelsSse2(destination, source);
                    return;
                }
                Fallback.DuplicateMonauralTo4Channels(destination, source);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo4ChannelsAvx2(Span<float> destination, ReadOnlySpan<float> source)
            {
                unsafe
                {
                    ref var rsi = ref MemoryMarshal.GetReference(source);
                    ref var rdi = ref MemoryMarshal.GetReference(destination);
                    nint i = 0, length = MathI.Min(source.Length * 4, destination.Length);
                    var olen = length - 3 * 8;
                    var ymm0 = Vector256.Create(0, 0, 0, 0, 1, 1, 1, 1);
                    var ymm1 = Vector256.Create(2, 2, 2, 2, 3, 3, 3, 3);
                    var ymm2 = Vector256.Create(4, 4, 4, 4, 5, 5, 5, 5);
                    var ymm3 = Vector256.Create(6, 6, 6, 6, 7, 7, 7, 7);
                    for (; i < olen; i += 32)
                    {
                        var ymm4 = Unsafe.As<float, Vector256<float>>(ref Unsafe.AddByteOffset(ref rsi, i));
                        var ymm5 = Avx2.PermuteVar8x32(ymm4, ymm0);
                        var ymm6 = Avx2.PermuteVar8x32(ymm4, ymm1);
                        var ymm7 = Avx2.PermuteVar8x32(ymm4, ymm2);
                        ymm4 = Avx2.PermuteVar8x32(ymm4, ymm3);
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm4;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm7;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm6;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm5;
                    }
                    for (; i < length; i += 4)
                    {
                        var xmm4 = AvxUtils.BroadcastScalarToVector128(ref Unsafe.AddByteOffset(ref rsi, i));
                        Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm4;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo4ChannelsSse2(Span<float> destination, ReadOnlySpan<float> source)
            {
                unsafe
                {
                    ref var rsi = ref MemoryMarshal.GetReference(source);
                    ref var rdi = ref MemoryMarshal.GetReference(destination);
                    nint i = 0, length = MathI.Min(source.Length * 4, destination.Length);
                    var olen = length - 3 * 4;
                    for (; i < olen; i += 16)
                    {
                        var xmm3 = Unsafe.As<float, Vector128<int>>(ref Unsafe.AddByteOffset(ref rsi, i));
                        var xmm0 = Sse2.Shuffle(xmm3, 0);
                        var xmm1 = Sse2.Shuffle(xmm3, 85);
                        var xmm2 = Sse2.Shuffle(xmm3, 170);
                        xmm3 = Sse2.Shuffle(xmm3, 255);
                        Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm3;
                        Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm2;
                        Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
                        Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                    }
                    for (; i < length; i += 4)
                    {
                        var xmm0 = Vector128.Create(Unsafe.AddByteOffset(ref rsi, i));
                        Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm0;
                    }
                }
            }

            #endregion Quad

            #endregion DuplicateMonauralToChannels

            #region Deinterleave
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DeinterleaveStereoSingleX86(ReadOnlySpan<float> buffer, Span<float> left, Span<float> right)
            {
                if (Avx2.IsSupported)
                {
                    DeinterleaveStereoSingleAvx2(buffer, left, right);
                    return;
                }
                if (Sse.IsSupported)
                {
                    DeinterleaveStereoSingleSse(buffer, left, right);
                    return;
                }
                Fallback.DeinterleaveStereoSingle(buffer, left, right);
            }
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DeinterleaveStereoSingleAvx2(ReadOnlySpan<float> buffer, Span<float> left, Span<float> right)
            {
                if (Avx2.IsSupported)
                {
                    nint i, length = MathI.Min(MathI.Min(left.Length, right.Length), buffer.Length / 2);
                    ref var rsi = ref MemoryMarshal.GetReference(buffer);
                    ref var r8 = ref MemoryMarshal.GetReference(left);
                    ref var r9 = ref MemoryMarshal.GetReference(right);
                    var olen = length - 31;
                    for (i = 0; i < olen; i += 32)
                    {
                        var ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 0 * Vector256<float>.Count));
                        var ymm1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 1 * Vector256<float>.Count));
                        var ymm2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 2 * Vector256<float>.Count));
                        var ymm3 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 3 * Vector256<float>.Count));
                        var ymm4 = Avx.Shuffle(ymm0, ymm1, 0x88);
                        ymm4 = Avx2.Permute4x64(ymm4.AsDouble(), 0xd8).AsSingle();
                        var ymm5 = Avx.Shuffle(ymm2, ymm3, 0x88);
                        ymm5 = Avx2.Permute4x64(ymm5.AsDouble(), 0xd8).AsSingle();
                        ymm0 = Avx.Shuffle(ymm0, ymm1, 0xdd);
                        ymm0 = Avx2.Permute4x64(ymm0.AsDouble(), 0xd8).AsSingle();
                        ymm1 = Avx.Shuffle(ymm2, ymm3, 0xdd);
                        ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 0xd8).AsSingle();
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i + 0 * Vector256<float>.Count)) = ymm4;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i + 1 * Vector256<float>.Count)) = ymm5;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r9, i + 0 * Vector256<float>.Count)) = ymm0;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r9, i + 1 * Vector256<float>.Count)) = ymm1;
                        ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 4 * Vector256<float>.Count));
                        ymm1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 5 * Vector256<float>.Count));
                        ymm2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 6 * Vector256<float>.Count));
                        ymm3 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 7 * Vector256<float>.Count));
                        ymm4 = Avx.Shuffle(ymm0, ymm1, 0x88);
                        ymm4 = Avx2.Permute4x64(ymm4.AsDouble(), 0xd8).AsSingle();
                        ymm5 = Avx.Shuffle(ymm2, ymm3, 0x88);
                        ymm5 = Avx2.Permute4x64(ymm5.AsDouble(), 0xd8).AsSingle();
                        ymm0 = Avx.Shuffle(ymm0, ymm1, 0xdd);
                        ymm0 = Avx2.Permute4x64(ymm0.AsDouble(), 0xd8).AsSingle();
                        ymm1 = Avx.Shuffle(ymm2, ymm3, 0xdd);
                        ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 0xd8).AsSingle();
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i + 2 * Vector256<float>.Count)) = ymm4;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i + 3 * Vector256<float>.Count)) = ymm5;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r9, i + 2 * Vector256<float>.Count)) = ymm0;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r9, i + 3 * Vector256<float>.Count)) = ymm1;
                    }
                    olen = length - 15;
                    for (i = 0; i < olen; i += 16)
                    {
                        var ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 0 * Vector256<float>.Count));
                        var ymm1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 1 * Vector256<float>.Count));
                        var ymm2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 2 * Vector256<float>.Count));
                        var ymm3 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, 2 * i + 3 * Vector256<float>.Count));
                        var ymm4 = Avx.Shuffle(ymm0, ymm1, 0x88);
                        ymm4 = Avx2.Permute4x64(ymm4.AsDouble(), 0xd8).AsSingle();
                        var ymm5 = Avx.Shuffle(ymm2, ymm3, 0x88);
                        ymm5 = Avx2.Permute4x64(ymm5.AsDouble(), 0xd8).AsSingle();
                        ymm0 = Avx.Shuffle(ymm0, ymm1, 0xdd);
                        ymm0 = Avx2.Permute4x64(ymm0.AsDouble(), 0xd8).AsSingle();
                        ymm1 = Avx.Shuffle(ymm2, ymm3, 0xdd);
                        ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 0xd8).AsSingle();
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i + 0 * Vector256<float>.Count)) = ymm4;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i + 1 * Vector256<float>.Count)) = ymm5;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r9, i + 0 * Vector256<float>.Count)) = ymm0;
                        Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r9, i + 1 * Vector256<float>.Count)) = ymm1;
                    }
                    for (; i < length; i++)
                    {
                        var l = Unsafe.Add(ref rsi, 2 * i);
                        var r = Unsafe.Add(ref rsi, 2 * i + 1);
                        Unsafe.Add(ref r8, i) = l;
                        Unsafe.Add(ref r9, i) = r;
                    }
                }
            }
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DeinterleaveStereoSingleSse(ReadOnlySpan<float> buffer, Span<float> left, Span<float> right)
            {
                if (Sse.IsSupported)
                {
                    nint i, length = MathI.Min(MathI.Min(left.Length, right.Length), buffer.Length / 2);
                    ref var rsi = ref MemoryMarshal.GetReference(buffer);
                    ref var r8 = ref MemoryMarshal.GetReference(left);
                    ref var r9 = ref MemoryMarshal.GetReference(right);
                    var olen = length - 15;
                    for (i = 0; i < olen; i += 16)
                    {
                        var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, 2 * i + 0 * Vector128<float>.Count));
                        var xmm1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, 2 * i + 1 * Vector128<float>.Count));
                        var xmm2 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, 2 * i + 2 * Vector128<float>.Count));
                        var xmm3 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, 2 * i + 3 * Vector128<float>.Count));
                        var xmm4 = Sse.Shuffle(xmm0, xmm1, 0x88);
                        var xmm5 = Sse.Shuffle(xmm2, xmm3, 0x88);
                        xmm0 = Sse.Shuffle(xmm0, xmm1, 0xdd);
                        xmm1 = Sse.Shuffle(xmm2, xmm3, 0xdd);
                        Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r8, i + 0 * Vector128<float>.Count)) = xmm4;
                        Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r8, i + 1 * Vector128<float>.Count)) = xmm5;
                        Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r9, i + 0 * Vector128<float>.Count)) = xmm0;
                        Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r9, i + 1 * Vector128<float>.Count)) = xmm1;
                    }
                    for (; i < length; i++)
                    {
                        var l = Unsafe.Add(ref rsi, 2 * i);
                        var r = Unsafe.Add(ref rsi, 2 * i + 1);
                        Unsafe.Add(ref r8, i) = l;
                        Unsafe.Add(ref r9, i) = r;
                    }
                }
            }
            #endregion

            #region Log2

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void FastLog2Order5X86(Span<float> destination, ReadOnlySpan<float> source, bool allowFma = true)
            {
                if (allowFma && Avx2.IsSupported && Fma.IsSupported)
                {
                    FastLog2Order5FAvx2Fma(destination, source);
                    return;
                }
                if (Avx2.IsSupported)
                {
                    FastLog2Order5Avx2(destination, source);
                    return;
                }
                if (Sse2.IsSupported)
                {
                    FastLog2Order5Sse2(destination, source);
                    return;
                }
                Fallback.FastLog2Order5Fallback(destination, source);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void FastLog2Order5FAvx2Fma(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var rsi = ref MemoryMarshal.GetReference(source);
                ref var rdi = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(destination.Length, source.Length);
                const float C0 = 4.6385368923465681e-2f;
                const float C1 = -1.9626965970062927e-1f;
                const float C2 = 4.1759580460198917e-1f;
                const float C3 = -7.0966282913858282e-1f;
                const float C4 = 1.4419656175182945f;
                var r15 = BitConverter.SingleToInt32Bits(C0);
                var r14 = BitConverter.SingleToInt32Bits(C1);
                var r13 = BitConverter.SingleToInt32Bits(C2);
                var r12 = BitConverter.SingleToInt32Bits(C3);
                var r11 = BitConverter.SingleToInt32Bits(C4);
                var ymm15 = Vector256.Create(0x7f00_0000u);
                var ymm14 = Vector256.Create(0x3f80_0000u);
                Vector256<float> ymm13, ymm12;
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    ymm13 = Vector256.Create(r14).AsSingle();
                    ymm12 = Vector256.Create(r13).AsSingle();
                    var ymm0 = Avx2.ShiftLeftLogical(Unsafe.As<float, Vector256<uint>>(ref Unsafe.Add(ref rsi, i + 0 * Vector256<float>.Count)), 1).AsSingle();
                    var ymm1 = Avx2.ShiftLeftLogical(Unsafe.As<float, Vector256<uint>>(ref Unsafe.Add(ref rsi, i + 1 * Vector256<float>.Count)), 1).AsSingle();
                    var ymm2 = Avx2.ShiftLeftLogical(Unsafe.As<float, Vector256<uint>>(ref Unsafe.Add(ref rsi, i + 2 * Vector256<float>.Count)), 1).AsSingle();
                    var ymm3 = Avx2.ShiftLeftLogical(Unsafe.As<float, Vector256<uint>>(ref Unsafe.Add(ref rsi, i + 3 * Vector256<float>.Count)), 1).AsSingle();
                    var ymm4 = Avx2.ShiftLeftLogical(ymm0.AsUInt32(), 8).AsSingle();
                    var ymm5 = Avx2.ShiftLeftLogical(ymm1.AsUInt32(), 8).AsSingle();
                    var ymm6 = Avx2.ShiftLeftLogical(ymm2.AsUInt32(), 8).AsSingle();
                    var ymm7 = Avx2.ShiftLeftLogical(ymm3.AsUInt32(), 8).AsSingle();
                    ymm0 = Avx2.Subtract(ymm0.AsUInt32(), ymm15).AsSingle();
                    ymm1 = Avx2.Subtract(ymm1.AsUInt32(), ymm15).AsSingle();
                    ymm2 = Avx2.Subtract(ymm2.AsUInt32(), ymm15).AsSingle();
                    ymm3 = Avx2.Subtract(ymm3.AsUInt32(), ymm15).AsSingle();
                    ymm4 = Avx2.ShiftRightLogical(ymm4.AsUInt32(), 9).AsSingle();
                    ymm5 = Avx2.ShiftRightLogical(ymm5.AsUInt32(), 9).AsSingle();
                    ymm6 = Avx2.ShiftRightLogical(ymm6.AsUInt32(), 9).AsSingle();
                    ymm7 = Avx2.ShiftRightLogical(ymm7.AsUInt32(), 9).AsSingle();
                    ymm0 = Avx2.ShiftRightArithmetic(ymm0.AsInt32(), 24).AsSingle();
                    ymm0 = Avx.ConvertToVector256Single(ymm0.AsInt32());
                    ymm1 = Avx2.ShiftRightArithmetic(ymm1.AsInt32(), 24).AsSingle();
                    ymm1 = Avx.ConvertToVector256Single(ymm1.AsInt32());
                    ymm2 = Avx2.ShiftRightArithmetic(ymm2.AsInt32(), 24).AsSingle();
                    ymm2 = Avx.ConvertToVector256Single(ymm2.AsInt32());
                    ymm3 = Avx2.ShiftRightArithmetic(ymm3.AsInt32(), 24).AsSingle();
                    ymm3 = Avx.ConvertToVector256Single(ymm3.AsInt32());
                    var ymm8 = Vector256.Create(r15).AsSingle();
                    ymm4 = Avx2.Add(ymm14, ymm4.AsUInt32()).AsSingle();
                    ymm4 = Avx.Subtract(ymm4, ymm14.AsSingle());
                    ymm5 = Avx2.Add(ymm14, ymm5.AsUInt32()).AsSingle();
                    ymm5 = Avx.Subtract(ymm5, ymm14.AsSingle());
                    ymm6 = Avx2.Add(ymm14, ymm6.AsUInt32()).AsSingle();
                    ymm6 = Avx.Subtract(ymm6, ymm14.AsSingle());
                    ymm7 = Avx2.Add(ymm14, ymm7.AsUInt32()).AsSingle();
                    ymm7 = Avx.Subtract(ymm7, ymm14.AsSingle());
                    var ymm9 = ymm8;
                    var ymm10 = ymm8;
                    var ymm11 = ymm8;
                    ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm13);
                    ymm9 = Fma.MultiplyAdd(ymm5, ymm9, ymm13);
                    ymm10 = Fma.MultiplyAdd(ymm6, ymm10, ymm13);
                    ymm11 = Fma.MultiplyAdd(ymm7, ymm11, ymm13);
                    ymm13 = Vector256.Create(r12).AsSingle();
                    ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm12);
                    ymm9 = Fma.MultiplyAdd(ymm5, ymm9, ymm12);
                    ymm10 = Fma.MultiplyAdd(ymm6, ymm10, ymm12);
                    ymm11 = Fma.MultiplyAdd(ymm7, ymm11, ymm12);
                    ymm12 = Vector256.Create(r11).AsSingle();
                    ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm13);
                    ymm9 = Fma.MultiplyAdd(ymm5, ymm9, ymm13);
                    ymm10 = Fma.MultiplyAdd(ymm6, ymm10, ymm13);
                    ymm11 = Fma.MultiplyAdd(ymm7, ymm11, ymm13);
                    ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm12);
                    ymm9 = Fma.MultiplyAdd(ymm5, ymm9, ymm12);
                    ymm10 = Fma.MultiplyAdd(ymm6, ymm10, ymm12);
                    ymm11 = Fma.MultiplyAdd(ymm7, ymm11, ymm12);
                    ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm0);
                    ymm9 = Fma.MultiplyAdd(ymm5, ymm9, ymm1);
                    ymm10 = Fma.MultiplyAdd(ymm6, ymm10, ymm2);
                    ymm11 = Fma.MultiplyAdd(ymm7, ymm11, ymm3);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, 0 * Vector256<float>.Count)) = ymm8;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, 1 * Vector256<float>.Count)) = ymm9;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, 2 * Vector256<float>.Count)) = ymm10;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, 3 * Vector256<float>.Count)) = ymm11;
                }
                unchecked
                {
                    ymm13 = Vector256.Create(C0);
                    ymm12 = Vector256.Create(C1);
                    var ymm11 = Vector256.Create(C2);
                    var ymm10 = Vector256.Create(C3);
                    var ymm9 = Vector256.Create(C4);
                    olen = length - 7;
                    for (; i < olen; i += 8)
                    {
                        ref var r8 = ref Unsafe.Add(ref rdi, i);
                        var ymm0 = Avx2.ShiftLeftLogical(Unsafe.As<float, Vector256<uint>>(ref Unsafe.Add(ref rsi, i + 0 * Vector256<float>.Count)), 1).AsSingle();
                        var ymm4 = Avx2.ShiftLeftLogical(ymm0.AsUInt32(), 8).AsSingle();
                        ymm0 = Avx2.Subtract(ymm0.AsUInt32(), ymm15).AsSingle();
                        ymm4 = Avx2.ShiftRightLogical(ymm4.AsUInt32(), 9).AsSingle();
                        ymm0 = Avx2.ShiftRightArithmetic(ymm0.AsInt32(), 24).AsSingle();
                        ymm0 = Avx.ConvertToVector256Single(ymm0.AsInt32());
                        ymm4 = Avx2.Add(ymm14, ymm4.AsUInt32()).AsSingle();
                        var ymm8 = ymm13;
                        ymm4 = Avx.Subtract(ymm4, ymm14.AsSingle());
                        ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm12);
                        ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm11);
                        ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm10);
                        ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm9);
                        ymm8 = Fma.MultiplyAdd(ymm4, ymm8, ymm0);
                        Unsafe.As<float, Vector256<float>>(ref r8) = ymm8;
                    }
                    for (; i < length; i++)
                    {
                        ref var r8 = ref Unsafe.Add(ref rdi, i);
                        var xmm0 = Sse2.ShiftLeftLogical(Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i)).AsUInt32(), 1).AsSingle();
                        var xmm4 = Sse2.ShiftLeftLogical(xmm0.AsUInt32(), 8).AsSingle();
                        xmm0 = Sse2.Subtract(xmm0.AsUInt32(), ymm15.GetLower()).AsSingle();
                        xmm4 = Sse2.ShiftRightLogical(xmm4.AsUInt32(), 9).AsSingle();
                        xmm0 = Sse2.ShiftRightArithmetic(xmm0.AsInt32(), 24).AsSingle();
                        xmm0 = Sse2.ConvertToVector128Single(xmm0.AsInt32());
                        xmm4 = Sse2.Add(ymm14.GetLower(), xmm4.AsUInt32()).AsSingle();
                        var xmm8 = ymm13.GetLower();
                        xmm4 = Sse.Subtract(xmm4, ymm14.GetLower().AsSingle());
                        xmm8 = Fma.MultiplyAdd(xmm4, xmm8, ymm12.GetLower());
                        xmm8 = Fma.MultiplyAdd(xmm4, xmm8, ymm11.GetLower());
                        xmm8 = Fma.MultiplyAdd(xmm4, xmm8, ymm10.GetLower());
                        xmm8 = Fma.MultiplyAdd(xmm4, xmm8, ymm9.GetLower());
                        xmm8 = Fma.MultiplyAdd(xmm4, xmm8, xmm0);
                        r8 = xmm8.GetElement(0);
                    }
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void FastLog2Order5Avx2(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var rsi = ref MemoryMarshal.GetReference(source);
                ref var rdi = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(destination.Length, source.Length);
                const float C0 = 4.6385369e-2f;
                const float C1 = -1.9626966e-1f;
                const float C2 = 4.175958e-1f;
                const float C3 = -7.0966283e-1f;
                const float C4 = 1.4419656f;
                var ymm14 = Vector256.Create(0x3f80_0000u);
                Vector256<float> ymm13, ymm12;
                var olen = length - 15;
                ymm13 = Vector256.Create(C0);
                ymm12 = Vector256.Create(C1);
                var ymm11 = Vector256.Create(C2);
                var ymm10 = Vector256.Create(C3);
                var ymm9 = Vector256.Create(C4);
                var ymm8 = Vector256.Create(0x7fff_ffffu);
                var ymm7 = Vector256.Create(0x7f80_0000u);
                for (; i < olen; i += 16)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm0 = Avx2.And(ymm8, Unsafe.As<float, Vector256<uint>>(ref Unsafe.Add(ref rsi, i + 0 * Vector256<float>.Count))).AsSingle();
                    var ymm1 = Avx2.And(ymm8, Unsafe.As<float, Vector256<uint>>(ref Unsafe.Add(ref rsi, i + 1 * Vector256<float>.Count))).AsSingle();
                    var ymm2 = Avx2.AndNot(ymm7, ymm0.AsUInt32()).AsSingle();
                    var ymm3 = Avx2.AndNot(ymm7, ymm1.AsUInt32()).AsSingle();
                    ymm0 = Avx2.Subtract(ymm0.AsUInt32(), ymm14).AsSingle();
                    ymm1 = Avx2.Subtract(ymm1.AsUInt32(), ymm14).AsSingle();
                    ymm0 = Avx2.ShiftRightArithmetic(ymm0.AsInt32(), 23).AsSingle();
                    ymm0 = Avx.ConvertToVector256Single(ymm0.AsInt32());
                    ymm1 = Avx2.ShiftRightArithmetic(ymm1.AsInt32(), 23).AsSingle();
                    ymm1 = Avx.ConvertToVector256Single(ymm1.AsInt32());
                    ymm2 = Avx2.Add(ymm14, ymm2.AsUInt32()).AsSingle();
                    ymm2 = Avx.Subtract(ymm2, ymm14.AsSingle());
                    ymm3 = Avx2.Add(ymm14, ymm3.AsUInt32()).AsSingle();
                    ymm3 = Avx.Subtract(ymm3, ymm14.AsSingle());
                    var ymm4 = ymm13;
                    var ymm5 = ymm13;
                    ymm4 = Avx.Multiply(ymm4, ymm2);
                    ymm5 = Avx.Multiply(ymm5, ymm3);
                    ymm4 = Avx.Add(ymm4, ymm12);
                    ymm5 = Avx.Add(ymm5, ymm12);
                    ymm4 = Avx.Multiply(ymm4, ymm2);
                    ymm5 = Avx.Multiply(ymm5, ymm3);
                    ymm4 = Avx.Add(ymm4, ymm11);
                    ymm5 = Avx.Add(ymm5, ymm11);
                    ymm4 = Avx.Multiply(ymm4, ymm2);
                    ymm5 = Avx.Multiply(ymm5, ymm3);
                    ymm4 = Avx.Add(ymm4, ymm10);
                    ymm5 = Avx.Add(ymm5, ymm10);
                    ymm4 = Avx.Multiply(ymm4, ymm2);
                    ymm5 = Avx.Multiply(ymm5, ymm3);
                    ymm4 = Avx.Add(ymm4, ymm9);
                    ymm5 = Avx.Add(ymm5, ymm9);
                    ymm4 = Avx.Multiply(ymm4, ymm2);
                    ymm5 = Avx.Multiply(ymm5, ymm3);
                    ymm4 = Avx.Add(ymm4, ymm0);
                    ymm5 = Avx.Add(ymm5, ymm1);
                    Unsafe.As<float, Vector256<float>>(ref r8) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, Vector256<float>.Count)) = ymm5;
                }
                unchecked
                {
                    olen = length - 7;
                    for (; i < olen; i += 8)
                    {
                        ref var r8 = ref Unsafe.Add(ref rdi, i);
                        var ymm0 = Avx2.And(ymm8, Unsafe.As<float, Vector256<uint>>(ref Unsafe.Add(ref rsi, i))).AsSingle();
                        var ymm2 = Avx2.AndNot(ymm7, ymm0.AsUInt32()).AsSingle();
                        ymm0 = Avx2.Subtract(ymm0.AsUInt32(), ymm14).AsSingle();
                        ymm0 = Avx2.ShiftRightArithmetic(ymm0.AsInt32(), 23).AsSingle();
                        ymm0 = Avx.ConvertToVector256Single(ymm0.AsInt32());
                        ymm2 = Avx2.Add(ymm14, ymm2.AsUInt32()).AsSingle();
                        ymm2 = Avx.Subtract(ymm2, ymm14.AsSingle());
                        var ymm4 = ymm13;
                        ymm4 = Avx.Multiply(ymm4, ymm2);
                        ymm4 = Avx.Add(ymm4, ymm12);
                        ymm4 = Avx.Multiply(ymm4, ymm2);
                        ymm4 = Avx.Add(ymm4, ymm11);
                        ymm4 = Avx.Multiply(ymm4, ymm2);
                        ymm4 = Avx.Add(ymm4, ymm10);
                        ymm4 = Avx.Multiply(ymm4, ymm2);
                        ymm4 = Avx.Add(ymm4, ymm9);
                        ymm4 = Avx.Multiply(ymm4, ymm2);
                        ymm4 = Avx.Add(ymm4, ymm0);
                        Unsafe.As<float, Vector256<float>>(ref r8) = ymm4;
                    }
                    for (; i < length; i++)
                    {
                        ref var r8 = ref Unsafe.Add(ref rdi, i);
                        var eax = Unsafe.As<float, uint>(ref Unsafe.Add(ref rsi, i)) << 1;
                        var edx = eax << 8;
                        eax -= 0x7f00_0000u;
                        edx >>= 9;
                        eax >>= 24;
                        var xmm0 = (float)eax;
                        edx = 0x3f80_0000u + edx;
                        var xmm1 = BinaryExtensions.UInt32BitsToSingle(edx) - 1.0f;
                        var xmm2 = ymm13.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += ymm12.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += ymm11.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += ymm10.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += ymm9.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += xmm0;
                        r8 = xmm2;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void FastLog2Order5Sse2(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var rsi = ref MemoryMarshal.GetReference(source);
                ref var rdi = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(destination.Length, source.Length);
                const float C0 = 4.6385369e-2f;
                const float C1 = -1.9626966e-1f;
                const float C2 = 4.175958e-1f;
                const float C3 = -7.0966283e-1f;
                const float C4 = 1.4419656f;
                var xmm14 = Vector128.Create(0x3f80_0000u);
                Vector128<float> xmm13, xmm12;
                var olen = length - 7;
                xmm13 = Vector128.Create(C0);
                xmm12 = Vector128.Create(C1);
                var xmm11 = Vector128.Create(C2);
                var xmm10 = Vector128.Create(C3);
                var xmm9 = Vector128.Create(C4);
                var xmm8 = Vector128.Create(0x7fff_ffffu);
                var xmm7 = Vector128.Create(0x7f80_0000u);
                for (; i < olen; i += 8)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Sse2.And(xmm8, Unsafe.As<float, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 0 * Vector128<float>.Count))).AsSingle();
                    var xmm1 = Sse2.And(xmm8, Unsafe.As<float, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 1 * Vector128<float>.Count))).AsSingle();
                    var xmm2 = Sse2.AndNot(xmm7, xmm0.AsUInt32()).AsSingle();
                    var xmm3 = Sse2.AndNot(xmm7, xmm1.AsUInt32()).AsSingle();
                    xmm0 = Sse2.Subtract(xmm0.AsUInt32(), xmm14).AsSingle();
                    xmm1 = Sse2.Subtract(xmm1.AsUInt32(), xmm14).AsSingle();
                    xmm0 = Sse2.ShiftRightArithmetic(xmm0.AsInt32(), 23).AsSingle();
                    xmm0 = Sse2.ConvertToVector128Single(xmm0.AsInt32());
                    xmm1 = Sse2.ShiftRightArithmetic(xmm1.AsInt32(), 23).AsSingle();
                    xmm1 = Sse2.ConvertToVector128Single(xmm1.AsInt32());
                    xmm2 = Sse2.Add(xmm14, xmm2.AsUInt32()).AsSingle();
                    xmm2 = Sse.Subtract(xmm2, xmm14.AsSingle());
                    xmm3 = Sse2.Add(xmm14, xmm3.AsUInt32()).AsSingle();
                    xmm3 = Sse.Subtract(xmm3, xmm14.AsSingle());
                    var xmm4 = xmm13;
                    var xmm5 = xmm13;
                    xmm4 = Sse.Multiply(xmm4, xmm2);
                    xmm5 = Sse.Multiply(xmm5, xmm3);
                    xmm4 = Sse.Add(xmm4, xmm12);
                    xmm5 = Sse.Add(xmm5, xmm12);
                    xmm4 = Sse.Multiply(xmm4, xmm2);
                    xmm5 = Sse.Multiply(xmm5, xmm3);
                    xmm4 = Sse.Add(xmm4, xmm11);
                    xmm5 = Sse.Add(xmm5, xmm11);
                    xmm4 = Sse.Multiply(xmm4, xmm2);
                    xmm5 = Sse.Multiply(xmm5, xmm3);
                    xmm4 = Sse.Add(xmm4, xmm10);
                    xmm5 = Sse.Add(xmm5, xmm10);
                    xmm4 = Sse.Multiply(xmm4, xmm2);
                    xmm5 = Sse.Multiply(xmm5, xmm3);
                    xmm4 = Sse.Add(xmm4, xmm9);
                    xmm5 = Sse.Add(xmm5, xmm9);
                    xmm4 = Sse.Multiply(xmm4, xmm2);
                    xmm5 = Sse.Multiply(xmm5, xmm3);
                    xmm4 = Sse.Add(xmm4, xmm0);
                    xmm5 = Sse.Add(xmm5, xmm1);
                    Unsafe.As<float, Vector128<float>>(ref r8) = xmm4;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r8, Vector128<float>.Count)) = xmm5;
                }
                unchecked
                {
                    olen = length - 3;
                    for (; i < olen; i += 4)
                    {
                        ref var r8 = ref Unsafe.Add(ref rdi, i);
                        var xmm0 = Sse2.And(xmm8, Unsafe.As<float, Vector128<uint>>(ref Unsafe.Add(ref rsi, i))).AsSingle();
                        var xmm2 = Sse2.AndNot(xmm7, xmm0.AsUInt32()).AsSingle();
                        xmm0 = Sse2.Subtract(xmm0.AsUInt32(), xmm14).AsSingle();
                        xmm0 = Sse2.ShiftRightArithmetic(xmm0.AsInt32(), 23).AsSingle();
                        xmm0 = Sse2.ConvertToVector128Single(xmm0.AsInt32());
                        xmm2 = Sse2.Add(xmm14, xmm2.AsUInt32()).AsSingle();
                        xmm2 = Sse.Subtract(xmm2, xmm14.AsSingle());
                        var xmm4 = xmm13;
                        xmm4 = Sse.Multiply(xmm4, xmm2);
                        xmm4 = Sse.Add(xmm4, xmm12);
                        xmm4 = Sse.Multiply(xmm4, xmm2);
                        xmm4 = Sse.Add(xmm4, xmm11);
                        xmm4 = Sse.Multiply(xmm4, xmm2);
                        xmm4 = Sse.Add(xmm4, xmm10);
                        xmm4 = Sse.Multiply(xmm4, xmm2);
                        xmm4 = Sse.Add(xmm4, xmm9);
                        xmm4 = Sse.Multiply(xmm4, xmm2);
                        xmm4 = Sse.Add(xmm4, xmm0);
                        Unsafe.As<float, Vector128<float>>(ref r8) = xmm4;
                    }
                    for (; i < length; i++)
                    {
                        ref var r8 = ref Unsafe.Add(ref rdi, i);
                        var eax = Unsafe.As<float, uint>(ref Unsafe.Add(ref rsi, i)) << 1;
                        var edx = eax << 8;
                        eax -= 0x7f00_0000u;
                        edx >>= 9;
                        eax >>= 24;
                        var xmm0 = (float)eax;
                        edx = 0x3f80_0000u + edx;
                        var xmm1 = BinaryExtensions.UInt32BitsToSingle(edx) - 1.0f;
                        var xmm2 = xmm13.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += xmm12.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += xmm11.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += xmm10.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += xmm9.GetElement(0);
                        xmm2 *= xmm1;
                        xmm2 += xmm0;
                        r8 = xmm2;
                    }
                }
            }

            #endregion
        }
    }
}

#endif
