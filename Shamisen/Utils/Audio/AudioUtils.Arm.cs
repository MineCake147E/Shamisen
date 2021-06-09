#if NET5_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace Shamisen.Utils
{
    public static partial class AudioUtils
    {
        internal static partial class Arm
        {
            internal static bool IsSupported => ArmBase.IsSupported;

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveThreeInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
            {
                if (AdvSimd.IsSupported)
                {
                    //InterleaveThreeInt32AdvSimd(buffer, left, right, center);
                    //return;
                }
                Fallback.InterleaveThreeInt32(buffer, left, right, center);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveThreeInt32AdvSimd(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
            {
                unsafe
                {
                    //
                }
                Fallback.InterleaveStereoInt32(buffer, left, right);
            }
        }
    }
}

#endif
