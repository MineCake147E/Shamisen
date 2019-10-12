using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using MonoAudio.Mathematics;
using static System.Runtime.InteropServices.MemoryMarshal;

namespace MonoAudio.Synthesis
{
    /// <summary>
    /// Generates a square wave with specified frequency.
    /// </summary>
    /// <seealso cref="MonoAudio.ISampleSource" />
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
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void DisposeInternal(bool disposing) { }

        /// <summary>
        /// Generates the monaural sample.
        /// </summary>
        /// <param name="theta">The theta(from 0 to 2*pi).</param>
        /// <returns></returns>
        protected override float GenerateMonauralSample(double theta) => Math.Sign(Math.Sin(theta));
    }
}
