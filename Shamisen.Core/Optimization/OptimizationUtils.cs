using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Contains some utility for optimization.
    /// </summary>
    public static class OptimizationUtils
    {
#if NETCOREAPP3_1_OR_GREATER

        /// <summary>
        /// Returns 0 when the target framework doesn't support <see cref="MethodImplOptions.AggressiveOptimization"/>.
        /// </summary>
        public const MethodImplOptions AggressiveOptimizationIfPossible = MethodImplOptions.AggressiveOptimization;

#else
        /// <summary>
        /// Returns 0 when the target framework doesn't support <see cref="MethodImplOptions"/>.AggressiveOptimization.
        /// </summary>
        public const MethodImplOptions AggressiveOptimizationIfPossible = default;
#endif

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Returns <see cref="MethodImplOptions.AggressiveInlining"/> when the target framework doesn't support <see cref="MethodImplOptions.AggressiveOptimization"/>.
        /// </summary>
#else
        /// <summary>
        /// Returns <see cref="MethodImplOptions.AggressiveInlining"/> when the target framework doesn't support <see cref="MethodImplOptions"/>.AggressiveOptimization.
        /// </summary>
#endif
        public const MethodImplOptions InlineAndOptimizeIfPossible = MethodImplOptions.AggressiveInlining | AggressiveOptimizationIfPossible;
    }
}
