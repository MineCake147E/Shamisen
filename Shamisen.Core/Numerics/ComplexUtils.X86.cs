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

namespace Shamisen.Utils.Numerics
{
    public static partial class ComplexUtils
    {
        internal static class X86
        {
            internal static bool IsSupported => AudioUtils.X86.IsSupported;

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
        }
    }
}

#endif