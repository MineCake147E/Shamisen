#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Shamisen.Utils.Intrinsics
{
    /// <summary>
    /// Contains some utility functions for <see cref="Ssse3"/>.
    /// </summary>
    public static class Ssse3Utils
    {
        /// <summary>
        /// Gets the value which indicates whether the <see cref="Ssse3Utils"/> can be used in this machine.
        /// </summary>
        public static bool IsSupported => Ssse3.IsSupported;
        /// <inheritdoc cref="Ssse3.AlignRight(Vector128{byte}, Vector128{byte}, byte)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector128<float> AlignRight(Vector128<float> left, Vector128<float> right, byte mask) => Ssse3.AlignRight(left.AsByte(), right.AsByte(), mask).AsSingle();
    }
}

#endif