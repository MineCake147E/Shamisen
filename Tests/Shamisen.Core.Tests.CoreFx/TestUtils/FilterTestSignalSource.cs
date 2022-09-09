using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Mathematics;

namespace Shamisen.Core.Tests.CoreFx.TestUtils
{
    public sealed class FilterTestSignalSource : ISampleSource
    {
        private bool disposedValue;

        /// <inheritdoc/>
        public SampleFormat Format { get; }

        /// <inheritdoc/>
        public ulong? Length { get; }

        /// <inheritdoc/>
        public ulong? TotalLength { get; }

        /// <inheritdoc/>
        public ulong? Position { get; }

        /// <inheritdoc/>
        public ISkipSupport SkipSupport { get; }

        /// <inheritdoc/>
        public ISeekSupport SeekSupport { get; }

        /// <summary>
        /// The theta in range: -pi to pi
        /// </summary>
        private double[] theta;

        private double Omega { get; set; }

        private double SamplingFrequencyInverse { get; }

        private double frequency;

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        public double Frequency
        {
            get => frequency;
            set
            {
                frequency = value;
                Omega = Math.Abs(value * SamplingFrequencyInverse);
            }
        }

        public FilterTestSignalSource(SampleFormat format)
        {
            Format = format;
            theta = new double[format.Channels];
            SamplingFrequencyInverse = 1.0 / format.SampleRate;
            var y = 0.0;
            var q = Math.Tau / format.Channels;
            for (var i = 0; i < theta.Length; i++)
            {
                theta[i] = y;
                y = AppendTheta(y, q);
            }
        }

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public ReadResult Read(Span<float> buffer)
        {
            var channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            var omega = Omega;
            var t = theta;
            for (var i = 0; i < buffer.Length; i += channels)
            {
                for (var ch = 0; ch < channels; ch++)
                {
                    buffer[i + ch] = (float)(t[ch] * (1 / Math.PI));
                    t[ch] = AppendTheta(t[ch], omega);
                }
            }
            return buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double AppendTheta(double theta, double omega)
        {
            theta += omega;
            if (theta > Math.PI)
            {
                theta += Math.PI;
                theta %= MathHelper.DoublePI;
                theta -= Math.PI;
            }
            return theta;
        }

        #region IDisposableSupport

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposableSupport
    }
}
