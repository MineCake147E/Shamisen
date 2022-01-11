#if NETCOREAPP3_1_OR_GREATER
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
        internal static partial class X86
        {
            internal static bool IsSupported => CooleyTukeyFft.X86.IsSupported;

            [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
            internal static void PerformLargeX86(Span<ComplexF> span, ReadOnlySpan<ComplexF> cache)
            {
                var length = span.Length;
                var omegas = cache;
                CooleyTukeyFft.X86.Perform8X86(span, omegas);
                if (length < 16) return;
                CooleyTukeyFft.X86.Perform16X86(span, omegas.Slice(4));
                if (length < 32) return;
                nint o = 12;
                for (var m = 32; m <= length; m <<= 1)
                {
                    nint mHalf = m >> 1;
                    var omorder = omegas.Slice((int)o, (int)mHalf);
                    CooleyTukeyFft.X86.PerformLargerOrderX86(span, omorder);
                    o += mHalf;
                }
            }
        }
    }
}

#endif