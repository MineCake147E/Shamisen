using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Numerics
{
    public static partial class ComplexUtils
    {
        internal static class Fallback
        {
            #region MultiplyAll
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void MultiplyAllFallback(Span<ComplexF> destination, ReadOnlySpan<ComplexF> source, ComplexF value)
            {
                ref var x9 = ref Unsafe.As<ComplexF, Vector2>(ref MemoryMarshal.GetReference(source));
                ref var x10 = ref Unsafe.As<ComplexF, Vector2>(ref MemoryMarshal.GetReference(destination));
                nint i, length = MathI.Min(destination.Length, source.Length);
                //Standard Vectors are slightly better at broadcasting, rather than shuffling.
                var v15_2s = value.Value;
                var v14_2s = new Vector2(-v15_2s.Y, v15_2s.X);
                var olen = length - 3;
                for (i = 0; i < olen; i += 4)
                {
                    ref var x11 = ref Unsafe.Add(ref x10, i);
                    var v0_2s = Unsafe.Add(ref x9, i + 0);
                    var v1_2s = Unsafe.Add(ref x9, i + 1);
                    var v2_2s = Unsafe.Add(ref x9, i + 2);
                    var v3_2s = Unsafe.Add(ref x9, i + 3);
                    var v4_2s = new Vector2(v0_2s.Y);
                    v0_2s = new(v0_2s.X);
                    var v5_2s = new Vector2(v1_2s.Y);
                    v1_2s = new(v1_2s.X);
                    var v6_2s = new Vector2(v2_2s.Y);
                    v2_2s = new(v2_2s.X);
                    var v7_2s = new Vector2(v3_2s.Y);
                    v3_2s = new(v3_2s.X);
                    v0_2s *= v15_2s;
                    v1_2s *= v15_2s;
                    v2_2s *= v15_2s;
                    v3_2s *= v15_2s;
                    v4_2s *= v14_2s;
                    v0_2s += v4_2s;
                    v5_2s *= v14_2s;
                    v1_2s += v5_2s;
                    v6_2s *= v14_2s;
                    v2_2s += v6_2s;
                    v7_2s *= v14_2s;
                    v3_2s += v7_2s;
                    Unsafe.Add(ref x11, 0) = v0_2s;
                    Unsafe.Add(ref x11, 1) = v1_2s;
                    Unsafe.Add(ref x11, 2) = v2_2s;
                    Unsafe.Add(ref x11, 3) = v3_2s;
                }
                for (; i < length; i++)
                {
                    ref var x11 = ref Unsafe.Add(ref x10, i);
                    var v0_2s = Unsafe.Add(ref x9, i + 0);
                    var v4_2s = new Vector2(v0_2s.Y);
                    v0_2s = new(v0_2s.X);
                    v0_2s *= v15_2s;
                    v4_2s *= v14_2s;
                    v0_2s += v4_2s;
                    x11 = v0_2s;
                }
            }
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void MultiplyAllFallback(Span<Complex> destination, ReadOnlySpan<Complex> source, Complex value)
            {
                ref var x9 = ref MemoryMarshal.GetReference(source);
                ref var x10 = ref MemoryMarshal.GetReference(destination);
                nint i, length = MathI.Min(destination.Length, source.Length);
                var q15 = value;
                for (i = 0; i < length; i++)
                {
                    ref var x11 = ref Unsafe.Add(ref x10, i);
                    var q0 = Unsafe.Add(ref x9, i + 0);
                    q0 *= q15;
                    x11 = q0;
                }
            }
            #endregion

            #region ConvertRealToComplex

            internal static void ConvertRealToComplexFallback(Span<ComplexF> destination, ReadOnlySpan<float> source)
            {
                ref var x9 = ref MemoryMarshal.GetReference(source);
                ref var x10 = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(destination.Length, source.Length);
                var olen = length - 4 * Vector<float>.Count + 1;
                for (; i < olen; i += 4 * Vector<float>.Count)
                {
                    ref var x11 = ref Unsafe.Add(ref x10, i);
                    var v0_ns = Unsafe.As<float, Vector<uint>>(ref Unsafe.Add(ref x9, i + 0 * Vector<float>.Count));
                    var v1_ns = Unsafe.As<float, Vector<uint>>(ref Unsafe.Add(ref x9, i + 1 * Vector<float>.Count));
                    var v2_ns = Unsafe.As<float, Vector<uint>>(ref Unsafe.Add(ref x9, i + 2 * Vector<float>.Count));
                    var v3_ns = Unsafe.As<float, Vector<uint>>(ref Unsafe.Add(ref x9, i + 3 * Vector<float>.Count));
                    Vector.Widen(v0_ns, out var v0_nd, out var v1_nd);
                    Unsafe.As<ComplexF, Vector<ulong>>(ref Unsafe.Add(ref x11, 0 * Vector<double>.Count)) = v0_nd;
                    Unsafe.As<ComplexF, Vector<ulong>>(ref Unsafe.Add(ref x11, 1 * Vector<double>.Count)) = v1_nd;
                    Vector.Widen(v1_ns, out v0_nd, out v1_nd);
                    Unsafe.As<ComplexF, Vector<ulong>>(ref Unsafe.Add(ref x11, 2 * Vector<double>.Count)) = v0_nd;
                    Unsafe.As<ComplexF, Vector<ulong>>(ref Unsafe.Add(ref x11, 3 * Vector<double>.Count)) = v1_nd;
                    Vector.Widen(v2_ns, out v0_nd, out v1_nd);
                    Unsafe.As<ComplexF, Vector<ulong>>(ref Unsafe.Add(ref x11, 4 * Vector<double>.Count)) = v0_nd;
                    Unsafe.As<ComplexF, Vector<ulong>>(ref Unsafe.Add(ref x11, 5 * Vector<double>.Count)) = v1_nd;
                    Vector.Widen(v3_ns, out v0_nd, out v1_nd);
                    Unsafe.As<ComplexF, Vector<ulong>>(ref Unsafe.Add(ref x11, 6 * Vector<double>.Count)) = v0_nd;
                    Unsafe.As<ComplexF, Vector<ulong>>(ref Unsafe.Add(ref x11, 7 * Vector<double>.Count)) = v1_nd;
                }
                for (; i < length; i++)
                {
                    Unsafe.Add(ref x10, i) = Unsafe.Add(ref x9, i);
                }
            }
            #endregion

            #region ExtractMagnitudeSquared
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void ExtractMagnitudeSquaredFallback(Span<float> destination, ReadOnlySpan<ComplexF> source)
            {
                ref var x9 = ref MemoryMarshal.GetReference(source);
                ref var x10 = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(destination.Length, source.Length);
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    ref var x11 = ref Unsafe.Add(ref x10, i);
                    var v0_4s = Unsafe.As<ComplexF, Vector4>(ref Unsafe.Add(ref x9, i + 0));
                    var v1_4s = Unsafe.As<ComplexF, Vector4>(ref Unsafe.Add(ref x9, i + 2));
                    var v2_4s = Unsafe.As<ComplexF, Vector4>(ref Unsafe.Add(ref x9, i + 4));
                    var v3_4s = Unsafe.As<ComplexF, Vector4>(ref Unsafe.Add(ref x9, i + 6));
                    v0_4s *= v0_4s;
                    v1_4s *= v1_4s;
                    v2_4s *= v2_4s;
                    v3_4s *= v3_4s;
                    var s0 = v0_4s.X + v0_4s.Y;
                    var s1 = v0_4s.Z + v0_4s.W;
                    var s2 = v1_4s.X + v1_4s.Y;
                    var s3 = v1_4s.Z + v1_4s.W;
                    var s4 = v2_4s.X + v2_4s.Y;
                    var s5 = v2_4s.Z + v2_4s.W;
                    var s6 = v3_4s.X + v3_4s.Y;
                    var s7 = v3_4s.Z + v3_4s.W;
                    Unsafe.Add(ref x11, 0) = s0;
                    Unsafe.Add(ref x11, 1) = s1;
                    Unsafe.Add(ref x11, 2) = s2;
                    Unsafe.Add(ref x11, 3) = s3;
                    Unsafe.Add(ref x11, 4) = s4;
                    Unsafe.Add(ref x11, 5) = s5;
                    Unsafe.Add(ref x11, 6) = s6;
                    Unsafe.Add(ref x11, 7) = s7;
                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    ref var x11 = ref Unsafe.Add(ref x10, i);
                    var v0_2s = Unsafe.As<ComplexF, Vector2>(ref Unsafe.Add(ref x9, i + 0));
                    var v1_2s = Unsafe.As<ComplexF, Vector2>(ref Unsafe.Add(ref x9, i + 1));
                    var v2_2s = Unsafe.As<ComplexF, Vector2>(ref Unsafe.Add(ref x9, i + 2));
                    var v3_2s = Unsafe.As<ComplexF, Vector2>(ref Unsafe.Add(ref x9, i + 3));
                    var s0 = v0_2s.LengthSquared();
                    var s1 = v1_2s.LengthSquared();
                    var s2 = v2_2s.LengthSquared();
                    var s3 = v3_2s.LengthSquared();
                    Unsafe.Add(ref x11, 0) = s0;
                    Unsafe.Add(ref x11, 1) = s1;
                    Unsafe.Add(ref x11, 2) = s2;
                    Unsafe.Add(ref x11, 3) = s3;
                }
                for (; i < length; i++)
                {
                    ref var x11 = ref Unsafe.Add(ref x10, i);
                    var v0_2s = Unsafe.As<ComplexF, Vector2>(ref Unsafe.Add(ref x9, i + 0));
                    var s0 = v0_2s.LengthSquared();
                    Unsafe.Add(ref x11, 0) = s0;
                }
            }
            #endregion
        }
    }
}
