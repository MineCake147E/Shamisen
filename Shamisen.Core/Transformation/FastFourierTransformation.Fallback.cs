using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Transformation
{
    public static partial class FastFourierTransformation
    {
        internal static class Fallback
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Perform2Fallback(Span<ComplexF> span)
            {
                ref var rsi = ref MemoryMarshal.GetReference(span);
                nint i = 0, length = span.Length;
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    ref var r8 = ref Unsafe.Add(ref rsi, i);
                    var a = Unsafe.Add(ref r8, 0);
                    var b = Unsafe.Add(ref r8, 1);
                    var u = Unsafe.Add(ref r8, 2);
                    var t = Unsafe.Add(ref r8, 3);
                    var v = a + b;
                    a -= b;
                    b = u + t;
                    u -= t;
                    Unsafe.Add(ref rsi, i + 0) = v;
                    Unsafe.Add(ref rsi, i + 1) = a;
                    Unsafe.Add(ref rsi, i + 2) = b;
                    Unsafe.Add(ref rsi, i + 3) = u;
                    a = Unsafe.Add(ref r8, 4);
                    b = Unsafe.Add(ref r8, 5);
                    u = Unsafe.Add(ref r8, 6);
                    t = Unsafe.Add(ref r8, 7);
                    v = a + b;
                    a -= b;
                    b = u + t;
                    u -= t;
                    Unsafe.Add(ref rsi, i + 4) = v;
                    Unsafe.Add(ref rsi, i + 5) = a;
                    Unsafe.Add(ref rsi, i + 6) = b;
                    Unsafe.Add(ref rsi, i + 7) = u;
                }
                olen = length - 1;
                for (; i < olen; i += 2)
                {
                    var u = Unsafe.Add(ref rsi, i);
                    var t = Unsafe.Add(ref rsi, i + 1);
                    Unsafe.Add(ref rsi, i) = u + t;
                    Unsafe.Add(ref rsi, i + 1) = u - t;
                }
            }

            #region Perform4
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform4Fallback(Span<ComplexF> span, FftMode mode)
            {
                switch (mode)
                {
                    case FftMode.Forward:
                        Perform4ForwardFallback(span);
                        break;
                    case FftMode.Backward:
                        Perform4BackwardFallback(span);
                        break;
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform4BackwardFallback(Span<ComplexF> span)
            {
                var sQ = MemoryMarshal.Cast<ComplexF, (ComplexF, ComplexF, ComplexF, ComplexF)>(span);
                for (var k = 0; k < sQ.Length; k++)
                {
                    ref var sA = ref sQ[k];
                    var v = new ComplexF(-sA.Item4.Imaginary, sA.Item4.Real);
                    var t = sA.Item3;
                    var w = sA.Item2;
                    var u = sA.Item1;
                    sA.Item1 = u + t;
                    sA.Item2 = w + v;
                    sA.Item3 = u - t;
                    sA.Item4 = w - v;
                }
            }

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void Perform4ForwardFallback(Span<ComplexF> span)
            {
                var sQ = MemoryMarshal.Cast<ComplexF, (ComplexF, ComplexF, ComplexF, ComplexF)>(span);
                for (var k = 0; k < sQ.Length; k++)
                {
                    ref var sA = ref sQ[k];
                    var v = new ComplexF(sA.Item4.Imaginary, -sA.Item4.Real);
                    var t = sA.Item3;
                    var w = sA.Item2;
                    var u = sA.Item1;
                    sA.Item1 = u + t;
                    sA.Item2 = w + v;
                    sA.Item3 = u - t;
                    sA.Item4 = w - v;
                }
            }
            #endregion

            #region PerformSingleOperation
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void PerformSingleOperationFallback(ref ComplexF pA, ref ComplexF pB, in ComplexF om)
            {
                var t = om * pB;
                var u = pA;
                pA = u + t;
                pB = u - t;
            }
            #endregion

            #region ExpandCache
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ExpandCacheFallback(Span<ComplexF> span, FftMode mode)
            {
                var order = MathI.LogBase2((uint)span.Length) + 1;
                nint i = 0, length = span.Length >> 1;
                var v2m = (ComplexF)GetValueToMultiply(order);
                v2m = mode == FftMode.Forward ? ComplexF.Conjugate(v2m) : v2m;
                ref var rdi = ref MemoryMarshal.GetReference(span);
                ref var rsi = ref Unsafe.Add(ref rdi, length);
                var olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var v0 = Unsafe.Add(ref rsi, i + 0);
                    var v1 = Unsafe.Add(ref rsi, i + 1);
                    var v2 = Unsafe.Add(ref rsi, i + 2);
                    var v3 = Unsafe.Add(ref rsi, i + 3);
                    var v4 = v0 * v2m;
                    var v5 = v1 * v2m;
                    var v6 = v2 * v2m;
                    var v7 = v3 * v2m;
                    r8 = v0;
                    Unsafe.Add(ref r8, 1) = v4;
                    Unsafe.Add(ref r8, 2) = v1;
                    Unsafe.Add(ref r8, 3) = v5;
                    Unsafe.Add(ref r8, 4) = v2;
                    Unsafe.Add(ref r8, 5) = v6;
                    Unsafe.Add(ref r8, 6) = v3;
                    Unsafe.Add(ref r8, 7) = v7;
                }
                for (; i < length; i++)
                {
                    ref var r8 = ref Unsafe.Add(ref rdi, i * 2);
                    var v0 = Unsafe.Add(ref rsi, i + 0);
                    var v4 = v0 * v2m;
                    r8 = v0;
                    Unsafe.Add(ref r8, 1) = v4;
                }
            }
            #endregion

            #region PerformLarge
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void PerformLargeFallback(Span<ComplexF> span, FftMode mode)
            {
                var pool = ArrayPool<byte>.Shared;
                var length = span.Length;
                var region = pool.Rent(Unsafe.SizeOf<ComplexF>() * length / 2);
                var buffer = MemoryMarshal.Cast<byte, ComplexF>(region.AsSpan());
                var omegas = buffer.Slice(0, length / 2);
                CalculateCache(mode, MathI.LogBase2(4u), omegas.SliceFromEnd(2));
                ref var shead = ref MemoryMarshal.GetReference(span);
                var index = 3;
                for (var m = 8; m <= length; m <<= 1)
                {
                    nint mHalf = m >> 1;
                    var omorder = omegas.SliceFromEnd((int)mHalf);
                    ExpandCache(omorder, mode);
                    ref var omH = ref MemoryMarshal.GetReference(omorder);
                    ref var rA = ref shead;
                    ref var rB = ref Unsafe.Add(ref rA, mHalf);
                    for (var k = 0; k < length; k += m)
                    {
                        nint j = 0;
                        for (; j < mHalf; j++)
                        {
                            ref var pA = ref Unsafe.Add(ref rA, j);
                            ref var pB = ref Unsafe.Add(ref rB, j);
                            PerformSingleOperation(ref pA, ref pB, Unsafe.Add(ref omH, j));
                        }
                        rA = ref Unsafe.Add(ref rA, m);
                        rB = ref Unsafe.Add(ref rB, m);
                    }
                    index++;
                }
                pool.Return(region);
            }
            #endregion
        }
    }
}
