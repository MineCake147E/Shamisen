using System;
using System.Collections.Generic;
using System.IO;
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
    public sealed class FractionalSeekSupport : ISeekSupport
    {
        private readonly ISeekSupport source;
        private readonly UInt64Divisor divisor;
        private readonly Int64Divisor signedDivisor;
        private readonly ulong multiplier;

        /// <summary>
        /// Initializes a new instance of the <see cref="FractionalSeekSupport"/> struct.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public FractionalSeekSupport(ISeekSupport source, ulong divisor, ulong multiplier)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            (ulong m, ulong d) = MathHelper.MinimizeDivisor(multiplier, divisor);
            this.divisor = new UInt64Divisor(d);
            signedDivisor = new Int64Divisor((long)d);
            this.multiplier = m;
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
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.divisor = divisor;
            signedDivisor = new Int64Divisor((long)divisor.Divisor);
            this.multiplier = multiplier;
        }

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}" /> with the specified offset in frames.
        /// </summary>
        /// <param name="offset">The offset in frames.</param>
        /// <param name="origin">The origin.</param>
        public void Seek(long offset, SeekOrigin origin) => source.Seek(offset * (long)multiplier / signedDivisor, origin);

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}" /> to the specified index in frames from the end of stream.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public void SeekLast(ulong offset) => source.SeekLast(offset * multiplier / divisor);

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}" /> to the specified index in frames.
        /// </summary>
        /// <param name="index">The index in frames.</param>
        public void SeekTo(ulong index) => source.SeekTo(index * multiplier / divisor);

        /// <summary>
        /// Skips the source the specified step in frames.
        /// </summary>
        /// <param name="step">The number of frames to skip.</param>
        public void Skip(ulong step) => source.Skip(step * multiplier / divisor);

        /// <summary>
        /// Steps this data source the specified step back in frames.
        /// </summary>
        /// <param name="step">The number of frames to step back.</param>
        public void StepBack(ulong step) => source.StepBack(step * multiplier / divisor);
    }
}
