using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Mathematics;

namespace Shamisen.Modifier
{
    /// <summary>
    /// Implements an <see cref="ISkipSupport"/> with fraction and source.
    /// </summary>
    /// <seealso cref="ISkipSupport" />
    public sealed class FractionalSkipSupport : ISkipSupport
    {
        private readonly ISkipSupport source;
        private readonly UInt64Divisor divisor;
        private readonly ulong multiplier;

        /// <summary>
        /// Initializes a new instance of the <see cref="FractionalSkipSupport"/> struct.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public FractionalSkipSupport(ISkipSupport source, UInt64Divisor divisor, ulong multiplier)
        {
            ArgumentNullException.ThrowIfNull(source);
            this.source = source;
            this.divisor = divisor;
            this.multiplier = multiplier;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FractionalSkipSupport"/> struct.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public FractionalSkipSupport(ISkipSupport source, ulong divisor, ulong multiplier)
        {
            ArgumentNullException.ThrowIfNull(source);
            this.source = source;
            (var m, var d) = MathHelper.MinimizeDivisor(multiplier, divisor);
            this.divisor = new UInt64Divisor(d);
            this.multiplier = m;
        }

        /// <summary>
        /// Skips the source the specified step in frames.
        /// </summary>
        /// <param name="step">The number of frames to skip.</param>
        public void Skip(ulong step) => source.Skip(step * multiplier / divisor);
    }
}
