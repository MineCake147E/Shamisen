using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif
#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif

namespace Shamisen.Utils
{
    /// <summary>
    /// Contains some mathematical functions for Floating-Point numbers.
    /// </summary>
    public static class FastMath
    {
        /// <returns>
        /// Parameter x or y, whichever is larger.
        /// If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="float.NaN"/>,
        /// the result might depend on CPUs.
        /// </returns>
        /// <inheritdoc cref="Math.Max(float, float)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Max(float x, float y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = Vector64.CreateScalarUnsafe(x);
                    var s1 = Vector64.CreateScalarUnsafe(y);
                    return AdvSimd.MaxNumberScalar(s0, s1).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    return Sse.MaxScalar(xmm0, xmm1).GetElement(0);
                }
#endif
                return Math.Max(x, y);
            }
        }
        /// <returns>
        /// Parameter x or y, whichever is smaller.
        /// If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="float.NaN"/>,
        /// the result might depend on CPUs.
        /// </returns>
        /// <inheritdoc cref="Math.Min(float, float)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Min(float x, float y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = Vector64.CreateScalarUnsafe(x);
                    var s1 = Vector64.CreateScalarUnsafe(y);
                    return AdvSimd.MinNumberScalar(s0, s1).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    return Sse.MinScalar(xmm0, xmm1).GetElement(0);
                }
#endif
                return Math.Min(x, y);
            }
        }
    }
}
