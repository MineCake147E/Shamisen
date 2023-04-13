using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Utils;
using Shamisen.Utils.Intrinsics;

namespace Shamisen
{
    public static partial class SpanUtils
    {
        #region 64-bits-wide

        #region ARMv8

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAdvSimd(Span<ulong> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            var olen = length - 2 * Vector128<ulong>.Count + 1;
            for (; i < olen; i += 2 * Vector128<ulong>.Count)
            {
                var v0_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var v1_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                v0_2d = AdvSimd.ReverseElement8(v0_2d);
                v1_2d = AdvSimd.ReverseElement8(v1_2d);
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = v0_2d;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2)) = v1_2d;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAdvSimdArm64(Span<ulong> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            unsafe
            {
                //This is a loop for A64FX, probably not optimal in Cortex-A710.
                //The `ldp` instruction isn't available at all, so we use simpler load.
                var olen = length - 8 * Vector128<ulong>.Count + 1;
                for (; i < olen; i += 8 * Vector128<ulong>.Count)
                {
                    var v0_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                    var v1_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                    var v2_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 4));
                    var v3_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 6));
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i), v0_2d, v1_2d);
                    v0_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 8));
                    v1_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 10));
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 4), v2_2d, v3_2d);
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    v2_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 12));
                    v3_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 14));
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 8), v0_2d, v1_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 12), v2_2d, v3_2d);
                }
                olen = 4 * Vector128<ulong>.Count + 1;
                for (; i < olen; i += 4 * Vector128<ulong>.Count)
                {
                    var v0_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                    var v1_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    var v2_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 4));
                    var v3_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 6));
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i), v0_2d, v1_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 4), v2_2d, v3_2d);
                }
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        #endregion

        #region X86

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAvx2(Span<ulong> span)
        {
            //The Internal number gets deconstructed in little-endian so the values are written in BIG-ENDIAN.
            var mask = Vector128.Create(0x0001020304050607ul, 0x08090a0b0c0d0e0ful).AsByte();
            var mask256 = Vector256.Create(mask, mask);
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            var olen = length - 31;
            for (; i < olen; i += 32)
            {
                var ymm0 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var ymm1 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 4));
                var ymm2 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 8));
                var ymm3 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 12));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), mask256).AsUInt64();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), mask256).AsUInt64();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), mask256).AsUInt64();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), mask256).AsUInt64();
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 4)) = ymm1;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm2;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 12)) = ymm3;
                ymm0 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 16));
                ymm1 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 20));
                ymm2 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 24));
                ymm3 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 28));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), mask256).AsUInt64();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), mask256).AsUInt64();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), mask256).AsUInt64();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), mask256).AsUInt64();
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm0;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 20)) = ymm1;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm2;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 28)) = ymm3;
            }
            for (; i < length - 3; i += 4)
            {
                var xmm0 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask256.GetLower()).AsUInt64();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask256.GetLower()).AsUInt64();
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessSsse3(Span<ulong> span)
        {
            //The Internal number gets deconstructed in little-endian so the values are written in BIG-ENDIAN.
            var mask = Vector128.Create(0x0001020304050607ul, 0x08090a0b0c0d0e0ful).AsByte();
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            for (; i < length - 15; i += 16)
            {
                var xmm0 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                var xmm2 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 4));
                var xmm3 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 6));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsUInt64();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsUInt64();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), mask).AsUInt64();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), mask).AsUInt64();
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2)) = xmm1;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm2;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 6)) = xmm3;
                xmm0 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 8));
                xmm1 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 10));
                xmm2 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 12));
                xmm3 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 14));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsUInt64();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsUInt64();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), mask).AsUInt64();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), mask).AsUInt64();
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm0;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 10)) = xmm1;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm2;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 14)) = xmm3;
            }
            for (; i < length - 3; i += 4)
            {
                var xmm0 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsUInt64();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsUInt64();
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }
        #endregion

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessFallback(Span<ulong> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            for (; i < length - 7; i += 8)
            {
                var x0 = Unsafe.Add(ref rdi, i + 0);
                var x1 = Unsafe.Add(ref rdi, i + 1);
                Unsafe.Add(ref rdi, i + 0) = BinaryPrimitives.ReverseEndianness(x0);
                Unsafe.Add(ref rdi, i + 1) = BinaryPrimitives.ReverseEndianness(x1);
                x0 = Unsafe.Add(ref rdi, i + 2);
                Unsafe.Add(ref rdi, i + 2) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 3);
                Unsafe.Add(ref rdi, i + 3) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 4);
                Unsafe.Add(ref rdi, i + 4) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 5);
                Unsafe.Add(ref rdi, i + 5) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 6);
                Unsafe.Add(ref rdi, i + 6) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 7);
                Unsafe.Add(ref rdi, i + 7) = BinaryPrimitives.ReverseEndianness(x0);
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessSimple(Span<ulong> span)
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = BinaryPrimitives.ReverseEndianness(span[i]);
            }
        }

        #endregion

        #region 32-bits-wide
        #region ARMv8

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAdvSimd(Span<int> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            var olen = length - 2 * Vector128<int>.Count + 1;
            for (; i < olen; i += 2 * Vector128<int>.Count)
            {
                var v0_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0));
                var v1_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 2));
                v0_2d = AdvSimd.ReverseElement8(v0_2d);
                v1_2d = AdvSimd.ReverseElement8(v1_2d);
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0)) = v0_2d;
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 2)) = v1_2d;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAdvSimdArm64(Span<int> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            unsafe
            {
                //This is a loop for A64FX, probably not optimal in Cortex-A710.
                //The `ldp` instruction isn't available at all, so we use simpler load.
                var olen = length - 8 * Vector128<int>.Count + 1;
                for (; i < olen; i += 8 * Vector128<int>.Count)
                {
                    var v0_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0));
                    var v1_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 2));
                    var v2_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4));
                    var v3_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 6));
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i), v0_2d, v1_2d);
                    v0_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 8));
                    v1_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 10));
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 4), v2_2d, v3_2d);
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    v2_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 12));
                    v3_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 14));
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 8), v0_2d, v1_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 12), v2_2d, v3_2d);
                }
                olen = 4 * Vector128<int>.Count + 1;
                for (; i < olen; i += 4 * Vector128<int>.Count)
                {
                    var v0_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0));
                    var v1_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 2));
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    var v2_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4));
                    var v3_2d = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 6));
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i), v0_2d, v1_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 4), v2_2d, v3_2d);
                }
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        #endregion

        #region X86

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAvx2(Span<int> span)
        {
            var mask256 = Vector256.Create(3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12, 3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12).AsByte();
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            var olen = length - 63;
            for (; i < olen; i += 64)
            {
                var ymm0 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 0));
                var ymm1 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 8));
                var ymm2 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 16));
                var ymm3 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 24));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), mask256).AsInt32();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), mask256).AsInt32();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), mask256).AsInt32();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), mask256).AsInt32();
                Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0;
                Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1;
                Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2;
                Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3;
                ymm0 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 32));
                ymm1 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 40));
                ymm2 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 48));
                ymm3 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 56));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), mask256).AsInt32();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), mask256).AsInt32();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), mask256).AsInt32();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), mask256).AsInt32();
                Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 32)) = ymm0;
                Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 40)) = ymm1;
                Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 48)) = ymm2;
                Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 56)) = ymm3;
            }
            olen = length - 7;
            for (; i < olen; i += 8)
            {
                var xmm0 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask256.GetLower()).AsInt32();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask256.GetLower()).AsInt32();
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessSsse3(Span<int> span)
        {
            //The Internal number gets deconstructed in little-endian so the values are written in BIG-ENDIAN.
            var mask = Vector128.Create(3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12).AsByte();
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            var olen = length - 31;
            for (; i < olen; i += 32)
            {
                var xmm0 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4));
                var xmm2 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 8));
                var xmm3 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 12));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsInt32();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsInt32();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), mask).AsInt32();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), mask).AsInt32();
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm2;
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm3;
                xmm0 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 16));
                xmm1 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 20));
                xmm2 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 24));
                xmm3 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 28));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsInt32();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsInt32();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), mask).AsInt32();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), mask).AsInt32();
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 16)) = xmm0;
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 20)) = xmm1;
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 24)) = xmm2;
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 28)) = xmm3;
            }
            olen = length - 7;
            for (; i < olen; i += 8)
            {
                var xmm0 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsInt32();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsInt32();
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        #endregion

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessFallback(Span<int> span)
        {
            ref var x9 = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            for (; i < length - 7; i += 8)
            {
                var x0 = Unsafe.Add(ref x9, i + 0);
                var x1 = Unsafe.Add(ref x9, i + 1);
                Unsafe.Add(ref x9, i + 0) = BinaryPrimitives.ReverseEndianness(x0);
                Unsafe.Add(ref x9, i + 1) = BinaryPrimitives.ReverseEndianness(x1);
                x0 = Unsafe.Add(ref x9, i + 2);
                Unsafe.Add(ref x9, i + 2) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref x9, i + 3);
                Unsafe.Add(ref x9, i + 3) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref x9, i + 4);
                Unsafe.Add(ref x9, i + 4) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref x9, i + 5);
                Unsafe.Add(ref x9, i + 5) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref x9, i + 6);
                Unsafe.Add(ref x9, i + 6) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref x9, i + 7);
                Unsafe.Add(ref x9, i + 7) = BinaryPrimitives.ReverseEndianness(x0);
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref x9, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref x9, i));
            }
        }
        #endregion

        #region 24-bits-wide

        //AdvSimd version requires multi-register versions of VectorTableLookup.

        #region X86

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessSsse3(Span<Int24> span)
        {
            var mask0 = Vector128.Create(2, 1, 0, 5, 4, 3, 8, 7, 6, 11, 10, 9, 14, 13, 12, 15).AsByte();
            ref var rdi = ref Unsafe.As<Int24, byte>(ref MemoryMarshal.GetReference(span));
            nint i = 0, length = span.Length * 3 - 2;
            var olen = length - 16 * 3 + 1;
            for (; i < olen; i += 16 * 3)
            {
                var xmm0 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref rdi, i + 0 * 16));
                var xmm1 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref rdi, i + 1 * 16));
                var xmm2 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref rdi, i + 2 * 16));
                var xmm3 = Ssse3.AlignRight(xmm1, xmm0, 15);
                var xmm4 = Ssse3.AlignRight(xmm2, xmm1, 14);
                var xmm5 = Ssse3.AlignRight(xmm0, xmm2, 13);
                xmm0 = Ssse3.Shuffle(xmm0, mask0);
                xmm3 = Ssse3.Shuffle(xmm3, mask0);
                xmm4 = Ssse3.Shuffle(xmm4, mask0);
                xmm5 = Ssse3.Shuffle(xmm5, mask0);
                xmm0 = Ssse3.AlignRight(xmm0, xmm0, 15);
                xmm1 = Ssse3.AlignRight(xmm3, xmm3, 15);
                xmm2 = Ssse3.AlignRight(xmm4, xmm4, 15);
                xmm0 = Ssse3.AlignRight(xmm3, xmm0, 1);
                xmm3 = Ssse3.AlignRight(xmm4, xmm1, 2);
                xmm4 = Ssse3.AlignRight(xmm5, xmm2, 3);
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref rdi, i + 0 * 16)) = xmm0;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref rdi, i + 1 * 16)) = xmm3;
                Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref rdi, i + 2 * 16)) = xmm4;
            }
            for (; i < length; i += 3)
            {
                (Unsafe.Add(ref rdi, i + 2), Unsafe.Add(ref rdi, i)) = (Unsafe.Add(ref rdi, i), Unsafe.Add(ref rdi, i + 2));
            }
        }
        #endregion

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessFallback(Span<Int24> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            for (; i < length - 7; i += 8)
            {
                var x0 = Unsafe.Add(ref rdi, i + 0);
                var x1 = Unsafe.Add(ref rdi, i + 1);
                Unsafe.Add(ref rdi, i + 0) = Int24.ReverseEndianness(x0);
                Unsafe.Add(ref rdi, i + 1) = Int24.ReverseEndianness(x1);
                x0 = Unsafe.Add(ref rdi, i + 2);
                Unsafe.Add(ref rdi, i + 2) = Int24.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 3);
                Unsafe.Add(ref rdi, i + 3) = Int24.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 4);
                Unsafe.Add(ref rdi, i + 4) = Int24.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 5);
                Unsafe.Add(ref rdi, i + 5) = Int24.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 6);
                Unsafe.Add(ref rdi, i + 6) = Int24.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 7);
                Unsafe.Add(ref rdi, i + 7) = Int24.ReverseEndianness(x0);
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Int24.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }
        #endregion

        #region 16-bits-wide

        #region ARMv8

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAdvSimd(Span<short> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            var olen = length - 2 * Vector128<short>.Count + 1;
            for (; i < olen; i += 2 * Vector128<short>.Count)
            {
                var v0_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0));
                var v1_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 2));
                v0_2d = AdvSimd.ReverseElement8(v0_2d);
                v1_2d = AdvSimd.ReverseElement8(v1_2d);
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0)) = v0_2d;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 2)) = v1_2d;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAdvSimdArm64(Span<short> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            unsafe
            {
                //This is a loop for A64FX, probably not optimal in Cortex-A710.
                //The `ldp` instruction isn't available at all, so we use simpler load.
                var olen = length - 8 * Vector128<short>.Count + 1;
                for (; i < olen; i += 8 * Vector128<short>.Count)
                {
                    var v0_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0));
                    var v1_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 2));
                    var v2_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 4));
                    var v3_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 6));
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i), v0_2d, v1_2d);
                    v0_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 8));
                    v1_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 10));
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 4), v2_2d, v3_2d);
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    v2_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 12));
                    v3_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 14));
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 8), v0_2d, v1_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 12), v2_2d, v3_2d);
                }
                olen = 4 * Vector128<short>.Count + 1;
                for (; i < olen; i += 4 * Vector128<short>.Count)
                {
                    var v0_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0));
                    var v1_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 2));
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    var v2_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 4));
                    var v3_2d = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 6));
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i), v0_2d, v1_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 4), v2_2d, v3_2d);
                }
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        #endregion

        #region X86

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAvx2(Span<short> span)
        {
            var mask256 = Vector256.Create(1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14, 1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14).AsByte();
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            var olen = length - 8 * Vector256<short>.Count + 1;
            for (; i < olen; i += 8 * Vector256<short>.Count)
            {
                var ymm0 = Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 0 * Vector256<short>.Count));
                var ymm1 = Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 1 * Vector256<short>.Count));
                var ymm2 = Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 2 * Vector256<short>.Count));
                var ymm3 = Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 3 * Vector256<short>.Count));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), mask256).AsInt16();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), mask256).AsInt16();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), mask256).AsInt16();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), mask256).AsInt16();
                Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 0 * Vector256<short>.Count)) = ymm0;
                Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 1 * Vector256<short>.Count)) = ymm1;
                Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 2 * Vector256<short>.Count)) = ymm2;
                Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 3 * Vector256<short>.Count)) = ymm3;
                ymm0 = Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 4 * Vector256<short>.Count));
                ymm1 = Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 5 * Vector256<short>.Count));
                ymm2 = Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 6 * Vector256<short>.Count));
                ymm3 = Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 7 * Vector256<short>.Count));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), mask256).AsInt16();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), mask256).AsInt16();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), mask256).AsInt16();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), mask256).AsInt16();
                Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 4 * Vector256<short>.Count)) = ymm0;
                Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 5 * Vector256<short>.Count)) = ymm1;
                Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 6 * Vector256<short>.Count)) = ymm2;
                Unsafe.As<short, Vector256<short>>(ref Unsafe.Add(ref rdi, i + 7 * Vector256<short>.Count)) = ymm3;
            }
            olen = length - 2 * Vector128<short>.Count + 1;
            for (; i < olen; i += 2 * Vector128<short>.Count)
            {
                var xmm0 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0 * Vector128<short>.Count));
                var xmm1 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 1 * Vector128<short>.Count));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask256.GetLower()).AsInt16();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask256.GetLower()).AsInt16();
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0 * Vector128<short>.Count)) = xmm0;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 1 * Vector128<short>.Count)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessSsse3(Span<short> span)
        {
            var mask = Vector128.Create(1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14).AsByte();
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            var olen = length - 63;
            for (; i < olen; i += 64)
            {
                var xmm0 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 8));
                var xmm2 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 16));
                var xmm3 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 24));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsInt16();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsInt16();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), mask).AsInt16();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), mask).AsInt16();
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm1;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 16)) = xmm2;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 24)) = xmm3;
                xmm0 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 32));
                xmm1 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 40));
                xmm2 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 48));
                xmm3 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 56));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsInt16();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsInt16();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), mask).AsInt16();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), mask).AsInt16();
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 32)) = xmm0;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 40)) = xmm1;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 48)) = xmm2;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 56)) = xmm3;
            }
            olen = length - 15;
            for (; i < olen; i += 16)
            {
                var xmm0 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 8));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsInt16();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsInt16();
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        #endregion

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessFallback(Span<short> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            for (; i < length - 7; i += 8)
            {
                var x0 = Unsafe.Add(ref rdi, i + 0);
                var x1 = Unsafe.Add(ref rdi, i + 1);
                Unsafe.Add(ref rdi, i + 0) = BinaryPrimitives.ReverseEndianness(x0);
                Unsafe.Add(ref rdi, i + 1) = BinaryPrimitives.ReverseEndianness(x1);
                x0 = Unsafe.Add(ref rdi, i + 2);
                Unsafe.Add(ref rdi, i + 2) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 3);
                Unsafe.Add(ref rdi, i + 3) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 4);
                Unsafe.Add(ref rdi, i + 4) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 5);
                Unsafe.Add(ref rdi, i + 5) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 6);
                Unsafe.Add(ref rdi, i + 6) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 7);
                Unsafe.Add(ref rdi, i + 7) = BinaryPrimitives.ReverseEndianness(x0);
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }
        #endregion
    }
}
