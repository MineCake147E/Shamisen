#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils.Intrinsics;

#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shamisen.Utils.Numerics
{
    /// <summary>
    /// Contains some utility functions to deal with <see cref="Complex"/>, <see cref="ComplexF"/>, and <see cref="ComplexD"/>.
    /// </summary>
    public static partial class ComplexUtils
    {
        #region MultiplyAll
        /// <summary>
        /// Multiplies all the contents of <paramref name="source"/> with <paramref name="value"/>, and stores to <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The place to store.</param>
        /// <param name="source">The multiplying values.</param>
        /// <param name="value">The multiplying constant value.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void MultiplyAll(Span<ComplexF> destination, ReadOnlySpan<ComplexF> source, ComplexF value)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.MultiplyAllX86(destination, source, value);
                    return;
                }
#endif
                Fallback.MultiplyAllFallback(destination, source, value);
            }
        }
        /// <summary>
        /// Multiplies all the contents of <paramref name="source"/> with <paramref name="value"/>, and stores to <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The place to store.</param>
        /// <param name="source">The multiplying values.</param>
        /// <param name="value">The multiplying constant value.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void MultiplyAll(Span<Complex> destination, ReadOnlySpan<Complex> source, Complex value)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.MultiplyAllX86(destination, source, value);
                    return;
                }
#endif
                Fallback.MultiplyAllFallback(destination, source, value);
            }
        }
        #endregion
    }
}
