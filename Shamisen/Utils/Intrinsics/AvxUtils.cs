﻿#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Intrinsics
{
    /// <summary>
    /// Contains some utility functions for <see cref="Avx"/>.
    /// </summary>
    public static class AvxUtils
    {
        /// <summary>
        /// Gets the value which indicates whether the <see cref="AvxUtils"/> can be used in this machine.
        /// </summary>
        public static bool IsSupported => Avx.IsSupported;

        /// <inheritdoc cref="Avx.BroadcastScalarToVector128(float*)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector128<float> BroadcastScalarToVector128(ref float source)
        {
            unsafe
            {
#if DEBUG
                return Vector128.Create(source);
#else
                return Avx.BroadcastScalarToVector128((float*)Unsafe.AsPointer(ref source));
#endif
            }
        }
    }
}
#endif