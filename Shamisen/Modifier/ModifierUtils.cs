using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
