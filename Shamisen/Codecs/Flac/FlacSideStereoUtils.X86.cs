#if NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac
{
    public static partial class FlacSideStereoUtils
    {
        internal static class X86
        {
            internal static bool IsSupported =>
#if NET5_0_OR_GREATER
                X86Base.IsSupported;

#else
                Sse.IsSupported;
#endif

            #region LeftSide

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveLeftSideStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                var min = MathI.Min(MathI.Min(left.Length, right.Length), buffer.Length / 2);
                buffer = buffer.SliceWhileIfLongerThan(min * 2);
                left = left.SliceWhileIfLongerThan(min);
                right = right.SliceWhileIfLongerThan(min);
                if (min > 32 && Avx2.IsSupported)
                {
                    DecodeAndInterleaveLeftSideStereoInt32Avx2(buffer, left, right);
                    return;
                }
                if (Sse2.IsSupported)
                {
                    DecodeAndInterleaveLeftSideStereoInt32Sse2(buffer, left, right);
                    return;
                }
                Fallback.DecodeAndInterleaveLeftSideStereoInt32(buffer, left, right);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveLeftSideStereoInt32Avx2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                ref var rL = ref MemoryMarshal.GetReference(left);
                ref var rR = ref MemoryMarshal.GetReference(right);
                ref var rB = ref MemoryMarshal.GetReference(buffer);
                nint i = 0, length = left.Length;
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    var ymm0 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i));
                    var ymm2 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i + 8));
                    var ymm1 = Avx2.Subtract(ymm0, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i)));
                    var ymm3 = Avx2.Subtract(ymm2, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i + 8)));
                    var ymm4 = Avx2.UnpackHigh(ymm0, ymm1);
                    var ymm5 = Avx2.UnpackHigh(ymm2, ymm3);
                    ymm0 = Avx2.UnpackLow(ymm0, ymm1);
                    ymm2 = Avx2.UnpackLow(ymm2, ymm3);
                    ymm1 = Avx2.Permute2x128(ymm0, ymm4, 0x20);
                    ymm0 = Avx2.Permute2x128(ymm0, ymm4, 0x31);
                    ymm3 = Avx2.Permute2x128(ymm2, ymm5, 0x20);
                    ymm2 = Avx2.Permute2x128(ymm2, ymm5, 0x31);
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 0)) = ymm1;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 8)) = ymm0;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 16)) = ymm3;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 24)) = ymm2;
                    ymm0 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i + 16));
                    ymm2 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i + 24));
                    ymm1 = Avx2.Subtract(ymm0, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i + 16)));
                    ymm3 = Avx2.Subtract(ymm2, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i + 24)));
                    ymm4 = Avx2.UnpackHigh(ymm0, ymm1);
                    ymm5 = Avx2.UnpackHigh(ymm2, ymm3);
                    ymm0 = Avx2.UnpackLow(ymm0, ymm1);
                    ymm2 = Avx2.UnpackLow(ymm2, ymm3);
                    ymm1 = Avx2.Permute2x128(ymm0, ymm4, 0x20);
                    ymm0 = Avx2.Permute2x128(ymm0, ymm4, 0x31);
                    ymm3 = Avx2.Permute2x128(ymm2, ymm5, 0x20);
                    ymm2 = Avx2.Permute2x128(ymm2, ymm5, 0x31);
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 32)) = ymm1;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 40)) = ymm0;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 48)) = ymm3;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 56)) = ymm2;
                }
                olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i))).AsInt32();
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 2))).AsInt32();
                    var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i))).AsInt32();
                    var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 2))).AsInt32();
                    xmm2 = Sse2.Subtract(xmm0, xmm2);
                    xmm3 = Sse2.Subtract(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 4)) = xmm1;
                    xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 4))).AsInt32();
                    xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 6))).AsInt32();
                    xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 4))).AsInt32();
                    xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 6))).AsInt32();
                    xmm2 = Sse2.Subtract(xmm0, xmm2);
                    xmm3 = Sse2.Subtract(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 8)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 12)) = xmm1;

                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i))).AsInt32();
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 2))).AsInt32();
                    var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i))).AsInt32();
                    var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 2))).AsInt32();
                    xmm2 = Sse2.Subtract(xmm0, xmm2);
                    xmm3 = Sse2.Subtract(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 4)) = xmm1;
                }
                for (; i < length; i += 1)
                {
                    var a = Unsafe.Add(ref rL, i);
                    Unsafe.Add(ref rB, i * 2) = a;
                    var b = Unsafe.Add(ref rR, i);
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 1) = a - b;
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveLeftSideStereoInt32Sse2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                ref var rL = ref MemoryMarshal.GetReference(left);
                ref var rR = ref MemoryMarshal.GetReference(right);
                ref var rB = ref MemoryMarshal.GetReference(buffer);
                nint i = 0, length = left.Length;
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i))).AsInt32();
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 2))).AsInt32();
                    var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i))).AsInt32();
                    var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 2))).AsInt32();
                    xmm2 = Sse2.Subtract(xmm0, xmm2);
                    xmm3 = Sse2.Subtract(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 4)) = xmm1;
                    xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 4))).AsInt32();
                    xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 6))).AsInt32();
                    xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 4))).AsInt32();
                    xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 6))).AsInt32();
                    xmm2 = Sse2.Subtract(xmm0, xmm2);
                    xmm3 = Sse2.Subtract(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 8)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 12)) = xmm1;
                }
                olen = length - 1;
                for (; i < olen; i += 2)
                {
                    var a = Unsafe.Add(ref rL, i);
                    Unsafe.Add(ref rB, i * 2) = a;
                    var b = Unsafe.Add(ref rR, i);
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 1) = a - b;
                    a = Unsafe.Add(ref Unsafe.Add(ref rL, i), 1);
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 2) = a;
                    b = Unsafe.Add(ref Unsafe.Add(ref rR, i), 1);
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 3) = a - b;
                }
                for (; i < length; i += 1)
                {
                    var a = Unsafe.Add(ref rL, i);
                    Unsafe.Add(ref rB, i * 2) = a;
                    var b = Unsafe.Add(ref rR, i);
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 1) = a - b;
                }
            }
            #endregion LeftSide

            #region RightSide

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveRightSideStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                var min = MathI.Min(MathI.Min(left.Length, right.Length), buffer.Length / 2);
                buffer = buffer.SliceWhileIfLongerThan(min * 2);
                left = left.SliceWhileIfLongerThan(min);
                right = right.SliceWhileIfLongerThan(min);
                if (left.Length > 32 && Avx2.IsSupported)
                {
                    DecodeAndInterleaveRightSideStereoInt32Avx2(buffer, left, right);
                    return;
                }
                if (Sse2.IsSupported)
                {
                    DecodeAndInterleaveRightSideStereoInt32Sse2(buffer, left, right);
                    return;
                }
                Fallback.DecodeAndInterleaveRightSideStereoInt32(buffer, left, right);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveRightSideStereoInt32Avx2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                ref var rL = ref MemoryMarshal.GetReference(left);
                ref var rR = ref MemoryMarshal.GetReference(right);
                ref var rB = ref MemoryMarshal.GetReference(buffer);
                nint i = 0, length = left.Length;
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    var ymm1 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i));
                    var ymm3 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i + 8));
                    var ymm0 = Avx2.Add(ymm1, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i)));
                    var ymm2 = Avx2.Add(ymm3, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i + 8)));
                    var ymm4 = Avx2.UnpackHigh(ymm0, ymm1);
                    var ymm5 = Avx2.UnpackHigh(ymm2, ymm3);
                    ymm0 = Avx2.UnpackLow(ymm0, ymm1);
                    ymm2 = Avx2.UnpackLow(ymm2, ymm3);
                    ymm1 = Avx2.Permute2x128(ymm0, ymm4, 0x20);
                    ymm0 = Avx2.Permute2x128(ymm0, ymm4, 0x31);
                    ymm3 = Avx2.Permute2x128(ymm2, ymm5, 0x20);
                    ymm2 = Avx2.Permute2x128(ymm2, ymm5, 0x31);
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 0)) = ymm1;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 8)) = ymm0;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 16)) = ymm3;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 24)) = ymm2;
                    ymm1 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i + 16));
                    ymm3 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i + 24));
                    ymm0 = Avx2.Add(ymm1, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i + 16)));
                    ymm2 = Avx2.Add(ymm3, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i + 24)));
                    ymm4 = Avx2.UnpackHigh(ymm0, ymm1);
                    ymm5 = Avx2.UnpackHigh(ymm2, ymm3);
                    ymm0 = Avx2.UnpackLow(ymm0, ymm1);
                    ymm2 = Avx2.UnpackLow(ymm2, ymm3);
                    ymm1 = Avx2.Permute2x128(ymm0, ymm4, 0x20);
                    ymm0 = Avx2.Permute2x128(ymm0, ymm4, 0x31);
                    ymm3 = Avx2.Permute2x128(ymm2, ymm5, 0x20);
                    ymm2 = Avx2.Permute2x128(ymm2, ymm5, 0x31);
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 32)) = ymm1;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 40)) = ymm0;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 48)) = ymm3;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 56)) = ymm2;
                }
                olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i))).AsInt32();
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 2))).AsInt32();
                    var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i))).AsInt32();
                    var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 2))).AsInt32();
                    xmm0 = Sse2.Add(xmm2, xmm0);
                    xmm1 = Sse2.Add(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 4)) = xmm1;
                    xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 4))).AsInt32();
                    xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 6))).AsInt32();
                    xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 4))).AsInt32();
                    xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 6))).AsInt32();
                    xmm0 = Sse2.Add(xmm2, xmm0);
                    xmm1 = Sse2.Add(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 8)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 12)) = xmm1;

                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i))).AsInt32();
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 2))).AsInt32();
                    var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i))).AsInt32();
                    var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 2))).AsInt32();
                    xmm0 = Sse2.Add(xmm2, xmm0);
                    xmm1 = Sse2.Add(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 4)) = xmm1;
                }
                for (; i < length; i += 1)
                {
                    var a = Unsafe.Add(ref rL, i);
                    var b = Unsafe.Add(ref rR, i);
                    Unsafe.Add(ref rB, i * 2) = a + b;
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 1) = b;
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveRightSideStereoInt32Sse2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                ref var rL = ref MemoryMarshal.GetReference(left);
                ref var rR = ref MemoryMarshal.GetReference(right);
                ref var rB = ref MemoryMarshal.GetReference(buffer);
                nint i = 0, length = left.Length;
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i))).AsInt32();
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 2))).AsInt32();
                    var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i))).AsInt32();
                    var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 2))).AsInt32();
                    xmm0 = Sse2.Add(xmm2, xmm0);
                    xmm1 = Sse2.Add(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 4)) = xmm1;
                    xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 4))).AsInt32();
                    xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rL, i + 6))).AsInt32();
                    xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 4))).AsInt32();
                    xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<int, ulong>(ref Unsafe.Add(ref rR, i + 6))).AsInt32();
                    xmm0 = Sse2.Add(xmm2, xmm0);
                    xmm1 = Sse2.Add(xmm1, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm2);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 8)) = xmm0;
                    xmm1 = Sse2.UnpackLow(xmm1, xmm3);
                    Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rB, i * 2 + 12)) = xmm1;
                }
                olen = length - 1;
                for (; i < olen; i += 2)
                {
                    var a = Unsafe.Add(ref rL, i);
                    var b = Unsafe.Add(ref rR, i);
                    Unsafe.Add(ref rB, i * 2) = a + b;
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 1) = b;
                    a = Unsafe.Add(ref Unsafe.Add(ref rL, i), 1);
                    b = Unsafe.Add(ref Unsafe.Add(ref rR, i), 1);
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 2) = a + b;
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 3) = b;
                }
                for (; i < length; i += 1)
                {
                    var a = Unsafe.Add(ref rL, i);
                    var b = Unsafe.Add(ref rR, i);
                    Unsafe.Add(ref rB, i * 2) = a + b;
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 1) = b;
                }
            }

            #endregion RightSide

            #region MidSide

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveMidSideStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                var min = MathI.Min(MathI.Min(left.Length, right.Length), buffer.Length / 2);
                buffer = buffer.SliceWhileIfLongerThan(min * 2);
                left = left.SliceWhileIfLongerThan(min);
                right = right.SliceWhileIfLongerThan(min);
                if (Avx2.IsSupported)
                {
                    DecodeAndInterleaveMidSideStereoInt32Avx2(buffer, left, right);
                    return;
                }
                Fallback.DecodeAndInterleaveMidSideStereoInt32(buffer, left, right);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveMidSideStereoInt32Avx2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                ref var rL = ref MemoryMarshal.GetReference(left);
                ref var rR = ref MemoryMarshal.GetReference(right);
                ref var rB = ref MemoryMarshal.GetReference(buffer);
                var ymm0 = Vector256.Create(1);
                nint i = 0, length = left.Length;
                var olen = length - 15;
                for (; i < olen; i += 16)
                {
                    var ymm1 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i));
                    var ymm2 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i));
                    ymm1 = Avx2.Add(ymm1, ymm1);
                    var ymm3 = Avx2.And(ymm2, ymm0);
                    ymm1 = Avx2.Or(ymm3, ymm1);
                    ymm3 = Avx2.Add(ymm1, ymm2);
                    ymm3 = Avx2.ShiftRightArithmetic(ymm3, 1);
                    ymm1 = Avx2.Subtract(ymm1, ymm2);
                    ymm1 = Avx2.ShiftRightArithmetic(ymm1, 1);
                    ymm2 = Avx2.UnpackLow(ymm3, ymm1);
                    ymm1 = Avx2.UnpackHigh(ymm3, ymm1);
                    ymm3 = Avx2.Permute2x128(ymm2, ymm1, 0x20);
                    ymm1 = Avx2.Permute2x128(ymm2, ymm1, 0x31);
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2)) = ymm3;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 8)) = ymm1;
                    ymm1 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rL, i + 8));
                    ymm2 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rR, i + 8));
                    ymm1 = Avx2.Add(ymm1, ymm1);
                    ymm3 = Avx2.And(ymm2, ymm0);
                    ymm1 = Avx2.Or(ymm3, ymm1);
                    ymm3 = Avx2.Add(ymm1, ymm2);
                    ymm3 = Avx2.ShiftRightArithmetic(ymm3, 1);
                    ymm1 = Avx2.Subtract(ymm1, ymm2);
                    ymm1 = Avx2.ShiftRightArithmetic(ymm1, 1);
                    ymm2 = Avx2.UnpackLow(ymm3, ymm1);
                    ymm1 = Avx2.UnpackHigh(ymm3, ymm1);
                    ymm3 = Avx2.Permute2x128(ymm2, ymm1, 0x20);
                    ymm1 = Avx2.Permute2x128(ymm2, ymm1, 0x31);
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 16)) = ymm3;
                    Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rB, i * 2 + 24)) = ymm1;
                }
                olen = length - 1;
                for (; i < olen; i += 2)
                {
                    var a = Unsafe.Add(ref rL, i);
                    var b = Unsafe.Add(ref rR, i);
                    a <<= 1;
                    a |= b & 1;
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 0) = (a + b) >> 1;
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 1) = (a - b) >> 1;
                    a = Unsafe.Add(ref Unsafe.Add(ref rL, i), 1);
                    b = Unsafe.Add(ref Unsafe.Add(ref rR, i), 1);
                    a <<= 1;
                    a |= b & 1;
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 2) = (a + b) >> 1;
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 3) = (a - b) >> 1;
                }
                for (; i < length; i += 1)
                {
                    var a = Unsafe.Add(ref rL, i);
                    var b = Unsafe.Add(ref rR, i);
                    a <<= 1;
                    a |= b & 1;
                    Unsafe.Add(ref rB, i * 2) = (a + b) >> 1;
                    Unsafe.Add(ref Unsafe.Add(ref rB, i * 2), 1) = (a - b) >> 1;
                }
            }

            #endregion MidSide
        }
    }
}

#endif
