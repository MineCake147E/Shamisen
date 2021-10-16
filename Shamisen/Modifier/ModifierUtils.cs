using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Modifier;

namespace Shamisen
{
    /// <summary>
    /// Contains some utility functions for <see cref="IAudioSource{TSample, TFormat}"/>'s modifiers.
    /// </summary>
    public static class ModifierUtils
    {
        /// <summary>
        /// Returns the new <see cref="ISkipSupport"/> with specified fraction.
        /// </summary>
        /// <param name="skipSupport">The skip support.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns></returns>
        public static ISkipSupport WithFraction(this ISkipSupport skipSupport, ulong divisor, ulong multiplier)
        => new FractionalSkipSupport(skipSupport, divisor, multiplier);

        /// <summary>
        /// Returns the new <see cref="ISkipSupport"/> with specified fraction.
        /// </summary>
        /// <param name="skipSupport">The skip support.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns></returns>
        public static ISkipSupport WithFraction(this ISkipSupport skipSupport, UInt64Divisor divisor, ulong multiplier)
        => new FractionalSkipSupport(skipSupport, divisor, multiplier);

        /// <summary>
        /// Returns the new <see cref="ISeekSupport"/> with specified fraction.
        /// </summary>
        /// <param name="seekSupport">The seek support.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns></returns>
        public static ISeekSupport WithFraction(this ISeekSupport seekSupport, ulong divisor, ulong multiplier)
        => new FractionalSeekSupport(seekSupport, divisor, multiplier);

        /// <summary>
        /// Returns <c>null</c> if either or both of <paramref name="lengthA"/> and <paramref name="lengthB"/> are null, otherwise the maximum value of <paramref name="lengthA"/> and <paramref name="lengthB"/>.
        /// </summary>
        /// <param name="lengthA">The length a.</param>
        /// <param name="lengthB">The length b.</param>
        /// <returns><c>null</c> if either or both of <paramref name="lengthA"/> and <paramref name="lengthB"/> are null, otherwise the maximum value of <paramref name="lengthA"/> and <paramref name="lengthB"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong? NullOrMax(ulong? lengthA, ulong? lengthB)
            => lengthA is null || lengthB is null ? null : Math.Max(lengthA.Value, lengthB.Value);
    }
}
