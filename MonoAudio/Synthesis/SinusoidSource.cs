using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using MonoAudio.MathUtils;
using static System.Runtime.InteropServices.MemoryMarshal;

namespace MonoAudio.Synthesis
{
    /// <summary>
    /// Generates a sinusoid wave with specified frequency.
    /// </summary>
    public sealed class SinusoidSource : PseudoMonauralSignalSourceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SquareWaveSource"/> class.
        /// </summary>
        /// <param name="format">The output format.</param>
        public SinusoidSource(SampleFormat format) : base(format)
        {
        }

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
        protected override float GenerateMonauralSample(double theta) => (float)Math.Sin(theta);
    }
}
