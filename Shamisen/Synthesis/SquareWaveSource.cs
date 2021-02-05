using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using Shamisen.Mathematics;

using static System.Runtime.InteropServices.MemoryMarshal;

namespace Shamisen.Synthesis
{
    /// <summary>
    /// Generates a square wave with specified frequency.
    /// </summary>
    /// <seealso cref="Shamisen.ISampleSource" />
    public sealed class SquareWaveSource : PseudoMonauralSignalSourceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SquareWaveSource"/> class.
        /// </summary>
        /// <param name="format">The output format.</param>
        public SquareWaveSource(SampleFormat format) : base(format)
        {
        }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public override ulong? Length => null;

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public override ulong? TotalLength => null;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}" /> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public override ulong? Position => null;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void DisposeInternal(bool disposing) { }

        /// <summary>
        /// Generates the monaural sample.
        /// </summary>
        /// <param name="theta">The theta(from 0 to 2*pi).</param>
        /// <returns></returns>
        protected override float GenerateMonauralSample(double theta) => Math.Sign(Math.PI - theta);
    }
}
