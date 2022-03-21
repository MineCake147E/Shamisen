#if NETCOREAPP3_1_OR_GREATER
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
    /// Contains some utility functions for <see cref="Sse2"/>.
    /// </summary>
    public static class Sse2Utils
    {
        /// <summary>
        /// Gets the value which indicates whether the <see cref="Sse2Utils"/> can be used in this machine.
        /// </summary>
        public static bool IsSupported => Sse2.IsSupported;
        /// <inheritdoc cref="Sse2.ShiftRightLogical128BitLane(Vector128{byte}, byte)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector128<float> ShiftRightLogical128BitLane(Vector128<float> value, byte numBytes) => Sse2.ShiftRightLogical128BitLane(value.AsByte(), numBytes).AsSingle();
        /// <inheritdoc cref="Sse2.ShiftLeftLogical128BitLane(Vector128{byte}, byte)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector128<float> ShiftLeftLogical128BitLane(Vector128<float> value, byte numBytes) => Sse2.ShiftLeftLogical128BitLane(value.AsByte(), numBytes).AsSingle();

        /// <inheritdoc cref="Sse3.MoveAndDuplicate(Vector128{double})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector128<double> MoveAndDuplicate(Vector128<double> source) => Sse3.IsSupported ? Sse3.MoveAndDuplicate(source) : Sse2.Shuffle(source, source, 0);
    }
}

#endif