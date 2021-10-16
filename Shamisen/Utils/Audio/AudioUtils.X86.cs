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
            internal static bool IsSupported =>
#if NET5_0_OR_GREATER
                X86Base.IsSupported;

#else
                Sse.IsSupported;
#endif
            #region Interleave

            #region Stereo

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                if (Avx.IsSupported)
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
                ref int rdi = ref MemoryMarshal.GetReference(buffer);
                ref int r8 = ref MemoryMarshal.GetReference(left);
                ref int r9 = ref MemoryMarshal.GetReference(right);
                nint i = 0, j = 0, length = MathI.Min(buffer.Length / 2, MathI.Min(left.Length, right.Length));
                nint olen = length - 31;
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
                    int a = Unsafe.Add(ref r8, i);
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
                ref int rdi = ref MemoryMarshal.GetReference(buffer);
                ref int r8 = ref MemoryMarshal.GetReference(left);
                ref int r9 = ref MemoryMarshal.GetReference(right);
                nint i = 0, j = 0, length = MathI.Min(buffer.Length / 2, MathI.Min(left.Length, right.Length));
                nint olen = length - 7;
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
                    int a = Unsafe.Add(ref r8, i);
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
                    ref int rL = ref MemoryMarshal.GetReference(left);
                    ref int rR = ref MemoryMarshal.GetReference(right);
                    ref int rC = ref MemoryMarshal.GetReference(center);
                    ref int rB = ref MemoryMarshal.GetReference(buffer);
                    var ymm13 = Vector256.Create(0, 0, 0, 1, 1, 1, 2, 2);
                    var ymm14 = Vector256.Create(2, 3, 3, 3, 4, 4, 4, 5);
                    var ymm15 = Vector256.Create(5, 5, 6, 6, 6, 7, 7, 7);
                    nint olen = length - 31;
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
                        int a = Unsafe.Add(ref rL, i);
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
                    ref int rL = ref MemoryMarshal.GetReference(left);
                    ref int rR = ref MemoryMarshal.GetReference(right);
                    ref int rC = ref MemoryMarshal.GetReference(center);
                    ref int rB = ref MemoryMarshal.GetReference(buffer);
                    nint olen = length - 7;
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
                        int a = Unsafe.Add(ref rL, i);
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
                    ref int r8 = ref MemoryMarshal.GetReference(frontLeft);
                    ref int r9 = ref MemoryMarshal.GetReference(frontRight);
                    ref int r10 = ref MemoryMarshal.GetReference(rearLeft);
                    ref int r15 = ref MemoryMarshal.GetReference(rearRight);
                    ref int rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = MathI.Min(buffer.Length / 4, MathI.Min(MathI.Min(frontLeft.Length, frontRight.Length), MathI.Min(rearLeft.Length, rearRight.Length))); ;
                    nint i = 0, j = 0;
                    nint olen = length - 31;
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
                        int a = Unsafe.Add(ref r8, i);
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

            #endregion

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
                ref float src = ref MemoryMarshal.GetReference(source);
                ref float dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = Math.Min(source.Length, destination.Length / 2);
                nint olen = length - 31;
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
                    float h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 2 + 0) = h;
                    Unsafe.Add(ref dst, i * 2 + 1) = h;
                }
            }
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralToStereoAvx(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref float src = ref MemoryMarshal.GetReference(source);
                ref float dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = Math.Min(source.Length, destination.Length / 2);
                nint olen = length - 7;
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
                    float h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 2 + 0) = h;
                    Unsafe.Add(ref dst, i * 2 + 1) = h;
                }
            }
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralToStereoSse(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref float src = ref MemoryMarshal.GetReference(source);
                ref float dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = Math.Min(source.Length, destination.Length / 2);
                nint olen = length - 7;
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
                    float h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 2 + 0) = h;
                    Unsafe.Add(ref dst, i * 2 + 1) = h;
                }
            }
            #endregion

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
                ref float src = ref MemoryMarshal.GetReference(source);
                ref float dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(source.Length, destination.Length / 3);
                nint olen = length - 63;
                var ymm15 = Vector256.Create(0, 0, 0, 1, 1, 1, 2, 2);
                var ymm14 = Vector256.Create(2, 3, 3, 3, 4, 4, 4, 5);
                var ymm13 = Vector256.Create(5, 5, 6, 6, 6, 7, 7, 7);
                for (; i < olen; i += 64)
                {
                    nint v = 2 * i + i;
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
                    nint v = 3 * i;
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
                    float h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 3 + 0) = h;
                    Unsafe.Add(ref dst, i * 3 + 1) = h;
                    Unsafe.Add(ref dst, i * 3 + 2) = h;
                }
            }
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo3ChannelsSse2(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref float src = ref MemoryMarshal.GetReference(source);
                ref float dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(source.Length, destination.Length / 3);
                nint olen = length - 15;
                for (; i < olen; i += 16)
                {
                    nint v = 2 * i + i;
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
                    nint v = 2 * i + i;
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
                    float h = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 3 + 0) = h;
                    Unsafe.Add(ref dst, i * 3 + 1) = h;
                    Unsafe.Add(ref dst, i * 3 + 2) = h;
                }
            }
            #endregion

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
                    ref float rsi = ref MemoryMarshal.GetReference(source);
                    ref float rdi = ref MemoryMarshal.GetReference(destination);
                    nint i = 0, length = MathI.Min(source.Length * 4, destination.Length);
                    nint olen = length - 3 * 8;
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
                    ref float rsi = ref MemoryMarshal.GetReference(source);
                    ref float rdi = ref MemoryMarshal.GetReference(destination);
                    nint i = 0, length = MathI.Min(source.Length * 4, destination.Length);
                    nint olen = length - 3 * 4;
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
            #endregion
            #endregion
        }
    }
}

#endif
