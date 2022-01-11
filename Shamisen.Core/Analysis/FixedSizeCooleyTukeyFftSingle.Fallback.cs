using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Analysis
{
    public sealed partial class FixedSizeCooleyTukeyFftSingle
    {
        internal static partial class Fallback
        {
            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void PerformLargeFallback(Span<ComplexF> span, ReadOnlySpan<ComplexF> cache)
            {
                var omegas = cache;
                var length = span.Length;
                ref var shead = ref MemoryMarshal.GetReference(span);
                var index = 3;
                var offset = 0;
                for (var m = 8; m <= length; m <<= 1)
                {
                    nint mHalf = m >> 1;
                    var omorder = omegas.Slice(offset, (int)mHalf);
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
                            CooleyTukeyFft.PerformSingleOperation(ref pA, ref pB, Unsafe.Add(ref omH, j));
                        }
                        rA = ref Unsafe.Add(ref rA, m);
                        rB = ref Unsafe.Add(ref rB, m);
                    }
                    index++;
                    offset += (int)mHalf;
                }
            }
        }
    }
}
