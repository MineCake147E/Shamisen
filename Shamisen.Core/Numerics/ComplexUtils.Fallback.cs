using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Numerics
{
    public static partial class ComplexUtils
    {
        internal static class Fallback
        {
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
        }
    }
}
