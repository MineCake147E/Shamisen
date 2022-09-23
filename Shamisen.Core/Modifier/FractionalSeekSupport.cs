using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Mathematics;
using Shamisen.Utils;

namespace Shamisen.Modifier
{
    /// <summary>
    /// Implements an <see cref="ISkipSupport"/> with fraction and source.
    /// </summary>
    /// <seealso cref="ISkipSupport" />
    public sealed class FractionalSeekSupport : ISeekSupport
    {
        private readonly ISeekSupport source;
        private readonly UInt64FractionMultiplier fractionMultiplier;

        /// <summary>
        /// Initializes a new instance of the <see cref="FractionalSeekSupport"/> struct.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public FractionalSeekSupport(ISeekSupport source, ulong divisor, ulong multiplier)
        {
            ArgumentNullException.ThrowIfNull(source);
            this.source = source;
            (var m, var d) = MathHelper.MinimizeDivisor(multiplier, divisor);
            fractionMultiplier = new(d, m);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FractionalSeekSupport"/> struct.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public FractionalSeekSupport(ISeekSupport source, UInt64Divisor divisor, ulong multiplier)
        {
            ArgumentNullException.ThrowIfNull(source);
            this.source = source;
            fractionMultiplier = new(divisor, multiplier);
        }

        /// <inheritdoc/>
        public void SeekLast(ulong offset)
        {
            source.SeekLast(0);
            StepBack(offset);
        }

        /// <inheritdoc/>
        public void SeekTo(ulong index)
        {
            source.SeekTo(0);
            Skip(index);
        }

        /// <inheritdoc/>
        public void Skip(ulong step)
        {
            var fm = fractionMultiplier;
            var maxNoOverflowInput = fm.MaxNoOverflowInput;
            if (maxNoOverflowInput < ulong.MaxValue)
            {
                var maxNoOverflowOutput = fm * maxNoOverflowInput;
                while (step > maxNoOverflowInput)
                {
                    source.Skip(maxNoOverflowOutput);
                    step -= maxNoOverflowInput;
                }
            }
            source.Skip(fm * step);
        }

        /// <inheritdoc/>
        public void StepBack(ulong step)
        {
            var fm = fractionMultiplier;
            var maxNoOverflowInput = fm.MaxNoOverflowInput;
            if (maxNoOverflowInput < ulong.MaxValue)
            {
                var maxNoOverflowOutput = fm * maxNoOverflowInput;
                while (step > maxNoOverflowInput)
                {
                    source.StepBack(maxNoOverflowOutput);
                    step -= maxNoOverflowInput;
                }
            }
            source.StepBack(fm * step);
        }
    }
}
