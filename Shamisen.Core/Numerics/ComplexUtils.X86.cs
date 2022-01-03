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
using System.Threading;
using System.Threading.Tasks;

using Shamisen.Utils;

namespace Shamisen.Numerics
{
    public static partial class ComplexUtils
    {
        internal static class X86
        {
            internal static bool IsSupported => AudioUtils.X86.IsSupported;
            #region MultiplyAll

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void MultiplyAllX86(Span<ComplexF> destination, ReadOnlySpan<ComplexF> source, ComplexF value)
            {
                if (Avx.IsSupported)
                {
                    MultiplyAllAvx(destination, source, value);
                    return;
                }
                if (Sse3.IsSupported)
                {
                    MultiplyAllSse3(destination, source, value);
                    return;
                }
                Fallback.MultiplyAllFallback(destination, source, value);
            }
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void MultiplyAllX86(Span<Complex> destination, ReadOnlySpan<Complex> source, Complex value)
            {
                if (Avx.IsSupported)
                {
                    MultiplyAllAvx(destination, source, value);
                    return;
                }
                if (Sse3.IsSupported)
                {
                    MultiplyAllSse3(destination, source, value);
                    return;
                }
                Fallback.MultiplyAllFallback(destination, source, value);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void MultiplyAllAvx(Span<ComplexF> destination, ReadOnlySpan<ComplexF> source, ComplexF value)
            {
                ref var rsi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(source));
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(destination));
                nint i, length = MathI.Min(destination.Length, source.Length);
                var ymm15 = Vector256.Create(value.Real);
                var ymm14 = Vector256.Create(value.Imaginary);
                var olen = length - 15;
                for (i = 0; i < olen; i += 16)
                {
                    ref var dhead = ref Unsafe.Add(ref rdi, i);
                    var ymm0 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector256<double>.Count));
                    var ymm1 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector256<double>.Count));
                    var ymm2 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 2 * Vector256<double>.Count));
                    var ymm3 = Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 3 * Vector256<double>.Count));
                    var ymm4 = Avx.Permute(ymm0, 0xb1);
                    ymm0 = Avx.Multiply(ymm0, ymm15);
                    var ymm5 = Avx.Permute(ymm1, 0xb1);
                    ymm1 = Avx.Multiply(ymm1, ymm15);
                    var ymm6 = Avx.Permute(ymm2, 0xb1);
                    ymm2 = Avx.Multiply(ymm2, ymm15);
                    var ymm7 = Avx.Permute(ymm3, 0xb1);
                    ymm3 = Avx.Multiply(ymm3, ymm15);
                    ymm4 = Avx.Multiply(ymm4, ymm14);
                    ymm0 = Avx.AddSubtract(ymm0, ymm4);
                    ymm5 = Avx.Multiply(ymm5, ymm14);
                    ymm1 = Avx.AddSubtract(ymm1, ymm5);
                    ymm6 = Avx.Multiply(ymm6, ymm14);
                    ymm2 = Avx.AddSubtract(ymm2, ymm6);
                    ymm7 = Avx.Multiply(ymm7, ymm14);
                    ymm3 = Avx.AddSubtract(ymm3, ymm7);
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref dhead, 0 * Vector256<double>.Count)) = ymm0;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref dhead, 1 * Vector256<double>.Count)) = ymm1;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref dhead, 2 * Vector256<double>.Count)) = ymm2;
                    Unsafe.As<double, Vector256<float>>(ref Unsafe.Add(ref dhead, 3 * Vector256<double>.Count)) = ymm3;
                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var dhead = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i));
                    var xmm1 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 2));
                    var xmm4 = Avx.Permute(xmm0, 0xb1);
                    var xmm5 = Avx.Permute(xmm1, 0xb1);
                    xmm0 = Sse.Multiply(xmm0, ymm15.GetLower());
                    xmm1 = Sse.Multiply(xmm1, ymm15.GetLower());
                    xmm4 = Sse.Multiply(xmm4, ymm14.GetLower());
                    xmm0 = Sse3.AddSubtract(xmm0, xmm4);
                    xmm5 = Sse.Multiply(xmm5, ymm14.GetLower());
                    xmm1 = Sse3.AddSubtract(xmm1, xmm5);
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref dhead, 0)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref dhead, 2)) = xmm1;
                }
                for (; i < length; i++)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i)).AsSingle();
                    var xmm4 = Avx.Permute(xmm0, 0xb1);
                    xmm0 = Sse.Multiply(xmm0, ymm15.GetLower());
                    xmm4 = Sse.Multiply(xmm4, ymm14.GetLower());
                    xmm0 = Sse3.AddSubtract(xmm0, xmm4);
                    Unsafe.Add(ref rdi, i) = xmm0.AsDouble().GetElement(0);
                }
            }
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void MultiplyAllAvx(Span<Complex> destination, ReadOnlySpan<Complex> source, Complex value)
            {
                ref var rsi = ref Unsafe.As<Complex, double>(ref MemoryMarshal.GetReference(source));
                ref var rdi = ref Unsafe.As<Complex, double>(ref MemoryMarshal.GetReference(destination));
                nint i, length = MathI.Min(destination.Length, source.Length) * 2;
                var ymm15 = Vector256.Create(value.Real);
                var ymm14 = Vector256.Create(value.Imaginary);
                var olen = length - 15;
                for (i = 0; i < olen; i += 16)
                {
                    ref var dhead = ref Unsafe.Add(ref rdi, i);
                    var ymm0 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rsi, i + 0 * Vector256<double>.Count));
                    var ymm1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rsi, i + 1 * Vector256<double>.Count));
                    var ymm2 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rsi, i + 2 * Vector256<double>.Count));
                    var ymm3 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rsi, i + 3 * Vector256<double>.Count));
                    var ymm4 = Avx.Permute(ymm0, 0x55);
                    ymm0 = Avx.Multiply(ymm0, ymm15);
                    var ymm5 = Avx.Permute(ymm1, 0x55);
                    ymm1 = Avx.Multiply(ymm1, ymm15);
                    var ymm6 = Avx.Permute(ymm2, 0x55);
                    ymm2 = Avx.Multiply(ymm2, ymm15);
                    var ymm7 = Avx.Permute(ymm3, 0x55);
                    ymm3 = Avx.Multiply(ymm3, ymm15);
                    ymm4 = Avx.Multiply(ymm4, ymm14);
                    ymm0 = Avx.AddSubtract(ymm0, ymm4);
                    ymm5 = Avx.Multiply(ymm5, ymm14);
                    ymm1 = Avx.AddSubtract(ymm1, ymm5);
                    ymm6 = Avx.Multiply(ymm6, ymm14);
                    ymm2 = Avx.AddSubtract(ymm2, ymm6);
                    ymm7 = Avx.Multiply(ymm7, ymm14);
                    ymm3 = Avx.AddSubtract(ymm3, ymm7);
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref dhead, 0 * Vector256<double>.Count)) = ymm0;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref dhead, 1 * Vector256<double>.Count)) = ymm1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref dhead, 2 * Vector256<double>.Count)) = ymm2;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref dhead, 3 * Vector256<double>.Count)) = ymm3;
                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var dhead = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref rsi, i));
                    var xmm1 = Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref rsi, i + 2));
                    var xmm4 = Avx.Permute(xmm0, 0x55);
                    var xmm5 = Avx.Permute(xmm1, 0x55);
                    xmm0 = Sse2.Multiply(xmm0, ymm15.GetLower());
                    xmm1 = Sse2.Multiply(xmm1, ymm15.GetLower());
                    xmm4 = Sse2.Multiply(xmm4, ymm14.GetLower());
                    xmm0 = Sse3.AddSubtract(xmm0, xmm4);
                    xmm5 = Sse2.Multiply(xmm5, ymm14.GetLower());
                    xmm1 = Sse3.AddSubtract(xmm1, xmm5);
                    Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref dhead, 0)) = xmm0;
                    Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref dhead, 2)) = xmm1;
                }
                olen = length - 1;
                for (; i < olen; i += 2)
                {
                    var xmm0 = Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref rsi, i));
                    var xmm4 = Avx.Permute(xmm0, 0x55);
                    xmm0 = Sse2.Multiply(xmm0, ymm15.GetLower());
                    xmm4 = Sse2.Multiply(xmm4, ymm14.GetLower());
                    xmm0 = Sse3.AddSubtract(xmm0, xmm4);
                    Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref rdi, i)) = xmm0;
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void MultiplyAllSse3(Span<ComplexF> destination, ReadOnlySpan<ComplexF> source, ComplexF value)
            {
                ref var rsi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(source));
                ref var rdi = ref Unsafe.As<ComplexF, double>(ref MemoryMarshal.GetReference(destination));
                nint i, length = MathI.Min(destination.Length, source.Length);
                var xmm15 = Vector128.Create(value.Real);
                var xmm14 = Vector128.Create(value.Imaginary);
                var olen = length - 3;
                for (i = 0; i < olen; i += 4)
                {
                    ref var dhead = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i));
                    var xmm1 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 2));
                    var xmm4 = Sse.Shuffle(xmm0, xmm0, 0xb1);
                    var xmm5 = Sse.Shuffle(xmm1, xmm1, 0xb1);
                    xmm0 = Sse.Multiply(xmm0, xmm15);
                    xmm1 = Sse.Multiply(xmm1, xmm15);
                    xmm4 = Sse.Multiply(xmm4, xmm14);
                    xmm0 = Sse3.AddSubtract(xmm0, xmm4);
                    xmm5 = Sse.Multiply(xmm5, xmm14);
                    xmm1 = Sse3.AddSubtract(xmm1, xmm5);
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref dhead, 0)) = xmm0;
                    Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref dhead, 2)) = xmm1;
                }
                for (; i < length; i++)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i)).AsSingle();
                    var xmm4 = Sse.Shuffle(xmm0, xmm0, 0xb1);
                    xmm0 = Sse.Multiply(xmm0, xmm15);
                    xmm4 = Sse.Multiply(xmm4, xmm14);
                    xmm0 = Sse3.AddSubtract(xmm0, xmm4);
                    Unsafe.Add(ref rdi, i) = xmm0.AsDouble().GetElement(0);
                }
            }
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void MultiplyAllSse3(Span<Complex> destination, ReadOnlySpan<Complex> source, Complex value)
            {
                ref var rsi = ref Unsafe.As<Complex, double>(ref MemoryMarshal.GetReference(source));
                ref var rdi = ref Unsafe.As<Complex, double>(ref MemoryMarshal.GetReference(destination));
                nint i, length = MathI.Min(destination.Length, source.Length) * 2;
                var xmm15 = Vector128.Create(value.Real);
                var xmm14 = Vector128.Create(value.Imaginary);
                var olen = length - 3;
                for (i = 0; i < olen; i += 4)
                {
                    ref var dhead = ref Unsafe.Add(ref rdi, i);
                    var xmm0 = Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref rsi, i));
                    var xmm1 = Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref rsi, i + 2));
                    var xmm4 = Sse2.Shuffle(xmm0, xmm0, 1);
                    var xmm5 = Sse2.Shuffle(xmm1, xmm1, 1);
                    xmm0 = Sse2.Multiply(xmm0, xmm15);
                    xmm1 = Sse2.Multiply(xmm1, xmm15);
                    xmm4 = Sse2.Multiply(xmm4, xmm14);
                    xmm0 = Sse3.AddSubtract(xmm0, xmm4);
                    xmm5 = Sse2.Multiply(xmm5, xmm14);
                    xmm1 = Sse3.AddSubtract(xmm1, xmm5);
                    Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref dhead, 0)) = xmm0;
                    Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref dhead, 2)) = xmm1;
                }
                olen = length - 1;
                for (; i < olen; i += 2)
                {
                    var xmm0 = Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref rsi, i));
                    var xmm4 = Sse2.Shuffle(xmm0, xmm0, 1);
                    xmm0 = Sse2.Multiply(xmm0, xmm15);
                    xmm4 = Sse2.Multiply(xmm4, xmm14);
                    xmm0 = Sse3.AddSubtract(xmm0, xmm4);
                    Unsafe.As<double, Vector128<double>>(ref Unsafe.Add(ref rdi, i)) = xmm0;
                }
            }
            #endregion

            #region ConvertRealToComplex

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ConvertRealToComplexX86(Span<ComplexF> destination, ReadOnlySpan<float> source)
            {
                if (Avx2.IsSupported)
                {
                    ConvertRealToComplexAvx2(destination, source);
                    return;
                }
                Fallback.ConvertRealToComplexFallback(destination, source);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ConvertRealToComplexAvx2(Span<ComplexF> destination, ReadOnlySpan<float> source)
            {
                ref var rsi = ref MemoryMarshal.GetReference(source);
                ref var rdi = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(destination.Length, source.Length);
                var ymm0 = Vector256.Create(0, 0, 1, 0, 2, 0, 3, 0);
                var ymm1 = Vector256.Create(4, 0, 5, 0, 6, 0, 7, 0);
                Unsafe.SkipInit(out Vector256<float> ymm2);
                ymm2 = Avx.Xor(ymm2, ymm2);
                var olen = length - 63;
                for (; i < olen; i += 64)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm4 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector256<float>.Count));
                    var ymm5 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector256<float>.Count));
                    var ymm6 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 2 * Vector256<float>.Count));
                    var ymm7 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 3 * Vector256<float>.Count));
                    var ymm3 = Avx2.PermuteVar8x32(ymm4, ymm0);
                    ymm3 = Avx.Blend(ymm3, ymm2, 0b1010_1010);
                    ymm4 = Avx2.PermuteVar8x32(ymm4, ymm1);
                    ymm4 = Avx.Blend(ymm4, ymm2, 0b1010_1010);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 0 * Vector256<double>.Count)) = ymm3;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 1 * Vector256<double>.Count)) = ymm4;
                    ymm3 = Avx2.PermuteVar8x32(ymm5, ymm0);
                    ymm3 = Avx.Blend(ymm3, ymm2, 0b1010_1010);
                    ymm4 = Avx2.PermuteVar8x32(ymm5, ymm1);
                    ymm4 = Avx.Blend(ymm4, ymm2, 0b1010_1010);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 2 * Vector256<double>.Count)) = ymm3;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 3 * Vector256<double>.Count)) = ymm4;
                    ymm3 = Avx2.PermuteVar8x32(ymm6, ymm0);
                    ymm3 = Avx.Blend(ymm3, ymm2, 0b1010_1010);
                    ymm4 = Avx2.PermuteVar8x32(ymm6, ymm1);
                    ymm4 = Avx.Blend(ymm4, ymm2, 0b1010_1010);
                    ymm5 = Avx2.PermuteVar8x32(ymm7, ymm0);
                    ymm5 = Avx.Blend(ymm5, ymm2, 0b1010_1010);
                    ymm6 = Avx2.PermuteVar8x32(ymm7, ymm1);
                    ymm6 = Avx.Blend(ymm6, ymm2, 0b1010_1010);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 4 * Vector256<double>.Count)) = ymm3;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 5 * Vector256<double>.Count)) = ymm4;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 6 * Vector256<double>.Count)) = ymm5;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 7 * Vector256<double>.Count)) = ymm6;
                    ymm4 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 4 * Vector256<float>.Count));
                    ymm5 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 5 * Vector256<float>.Count));
                    ymm6 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 6 * Vector256<float>.Count));
                    ymm7 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 7 * Vector256<float>.Count));
                    ymm3 = Avx2.PermuteVar8x32(ymm4, ymm0);
                    ymm3 = Avx.Blend(ymm3, ymm2, 0b1010_1010);
                    ymm4 = Avx2.PermuteVar8x32(ymm4, ymm1);
                    ymm4 = Avx.Blend(ymm4, ymm2, 0b1010_1010);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 8 * Vector256<double>.Count)) = ymm3;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 9 * Vector256<double>.Count)) = ymm4;
                    ymm3 = Avx2.PermuteVar8x32(ymm5, ymm0);
                    ymm3 = Avx.Blend(ymm3, ymm2, 0b1010_1010);
                    ymm4 = Avx2.PermuteVar8x32(ymm5, ymm1);
                    ymm4 = Avx.Blend(ymm4, ymm2, 0b1010_1010);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 10 * Vector256<double>.Count)) = ymm3;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 11 * Vector256<double>.Count)) = ymm4;
                    ymm3 = Avx2.PermuteVar8x32(ymm6, ymm0);
                    ymm3 = Avx.Blend(ymm3, ymm2, 0b1010_1010);
                    ymm4 = Avx2.PermuteVar8x32(ymm6, ymm1);
                    ymm4 = Avx.Blend(ymm4, ymm2, 0b1010_1010);
                    ymm5 = Avx2.PermuteVar8x32(ymm7, ymm0);
                    ymm5 = Avx.Blend(ymm5, ymm2, 0b1010_1010);
                    ymm6 = Avx2.PermuteVar8x32(ymm7, ymm1);
                    ymm6 = Avx.Blend(ymm6, ymm2, 0b1010_1010);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 12 * Vector256<double>.Count)) = ymm3;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 13 * Vector256<double>.Count)) = ymm4;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 14 * Vector256<double>.Count)) = ymm5;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 15 * Vector256<double>.Count)) = ymm6;
                }
                olen = length - 15;
                for (; i < olen; i += 16)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm4 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector256<float>.Count));
                    var ymm5 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector256<float>.Count));
                    var ymm3 = Avx2.PermuteVar8x32(ymm4, ymm0);
                    ymm3 = Avx.Blend(ymm3, ymm2, 0b1010_1010);
                    ymm4 = Avx2.PermuteVar8x32(ymm4, ymm1);
                    ymm4 = Avx.Blend(ymm4, ymm2, 0b1010_1010);
                    var ymm6 = Avx2.PermuteVar8x32(ymm5, ymm0);
                    ymm6 = Avx.Blend(ymm6, ymm2, 0b1010_1010);
                    var ymm7 = Avx2.PermuteVar8x32(ymm5, ymm1);
                    ymm7 = Avx.Blend(ymm7, ymm2, 0b1010_1010);
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 0 * Vector256<double>.Count)) = ymm3;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 1 * Vector256<double>.Count)) = ymm4;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 2 * Vector256<double>.Count)) = ymm6;
                    Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref r8, 3 * Vector256<double>.Count)) = ymm7;
                }
                for (; i < length; i++)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    Unsafe.As<ComplexF, float>(ref r8) = Unsafe.Add(ref rsi, i);
                    Unsafe.As<ComplexF, int>(ref Unsafe.AddByteOffset(ref r8, 4)) = 0;
                }
            }
            #endregion

            #region ExtractMagnitudeSquared
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ExtractMagnitudeSquaredX86(Span<float> destination, ReadOnlySpan<ComplexF> source)
            {
                if (Avx2.IsSupported)
                {
                    ExtractMagnitudeSquaredAvx2(destination, source);
                    return;
                }
                Fallback.ExtractMagnitudeSquaredFallback(destination, source);
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ExtractMagnitudeSquaredAvx2(Span<float> destination, ReadOnlySpan<ComplexF> source)
            {
                ref var rsi = ref MemoryMarshal.GetReference(source);
                ref var rdi = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(destination.Length, source.Length);
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i);
                    var ymm0 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 0));
                    var ymm1 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 4));
                    var ymm2 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 8));
                    var ymm3 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 12));
                    var ymm4 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 16));
                    var ymm5 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 20));
                    var ymm6 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 24));
                    var ymm7 = Unsafe.As<ComplexF, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 28));
                    var ymm8 = Avx.Shuffle(ymm0, ymm1, 136);
                    ymm8 = Avx2.Permute4x64(ymm8.AsDouble(), 216).AsSingle();
                    ymm8 = Avx.Multiply(ymm8, ymm8);
                    var ymm9 = Avx.Shuffle(ymm2, ymm3, 136);
                    ymm9 = Avx2.Permute4x64(ymm9.AsDouble(), 216).AsSingle();
                    ymm9 = Avx.Multiply(ymm9, ymm9);
                    var ymm10 = Avx.Shuffle(ymm4, ymm5, 136);
                    ymm10 = Avx2.Permute4x64(ymm10.AsDouble(), 216).AsSingle();
                    ymm10 = Avx.Multiply(ymm10, ymm10);
                    var ymm11 = Avx.Shuffle(ymm6, ymm7, 136);
                    ymm11 = Avx2.Permute4x64(ymm11.AsDouble(), 216).AsSingle();
                    ymm11 = Avx.Multiply(ymm11, ymm11);
                    ymm0 = Avx.Shuffle(ymm0, ymm1, 221);
                    ymm0 = Avx2.Permute4x64(ymm0.AsDouble(), 216).AsSingle();
                    ymm0 = Avx.Multiply(ymm0, ymm0);
                    ymm0 = Avx.Add(ymm8, ymm0);
                    ymm1 = Avx.Shuffle(ymm2, ymm3, 221);
                    ymm1 = Avx2.Permute4x64(ymm1.AsDouble(), 216).AsSingle();
                    ymm1 = Avx.Multiply(ymm1, ymm1);
                    ymm1 = Avx.Add(ymm9, ymm1);
                    ymm2 = Avx.Shuffle(ymm4, ymm5, 221);
                    ymm2 = Avx2.Permute4x64(ymm2.AsDouble(), 216).AsSingle();
                    ymm2 = Avx.Multiply(ymm2, ymm2);
                    ymm2 = Avx.Add(ymm10, ymm2);
                    ymm3 = Avx.Shuffle(ymm6, ymm7, 221);
                    ymm3 = Avx2.Permute4x64(ymm3.AsDouble(), 216).AsSingle();
                    ymm3 = Avx.Multiply(ymm3, ymm3);
                    ymm3 = Avx.Add(ymm11, ymm3);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, 0)) = ymm0;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, 8)) = ymm1;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, 16)) = ymm2;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, 24)) = ymm3;
                }
                for (; i < length; i++)
                {
                    var xmm0 = Unsafe.Add(ref rsi, i).AsVector128();
                    xmm0 = Sse41.DotProduct(xmm0, xmm0, 0x31);
                    Unsafe.Add(ref rdi, i) = xmm0.GetElement(0);
                }
            }
            #endregion
        }
    }
}

#endif