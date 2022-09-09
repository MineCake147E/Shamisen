using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

using Shamisen.Mathematics;

using static System.Runtime.InteropServices.MemoryMarshal;

namespace Shamisen.Synthesis
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly

    /// <summary>
    /// Defines a base infrastructure of the source generates a deterministic pseudo-monaural wave with specified frequency.
    /// </summary>
    /// <seealso cref="ISampleSource" />
    public abstract class PseudoMonauralSignalSourceBase : ISampleSource
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SinusoidSource"/> class.
        /// </summary>
        /// <param name="format">The output format.</param>
        protected PseudoMonauralSignalSourceBase(SampleFormat format)
        {
            SamplingFrequencyInverse = 1.0 / format.SampleRate;
            Format = format;
        }

        /// <inheritdoc/>
        public SampleFormat Format { get; }

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        public double Frequency { get; set; }

        private double SamplingFrequencyInverse { get; }

        private double Theta { get; set; } = 0;

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => throw new NotImplementedException();

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => throw new NotImplementedException();

        /// <inheritdoc/>
        public abstract ulong? Length { get; }

        /// <inheritdoc/>
        public abstract ulong? TotalLength { get; }

        /// <inheritdoc/>
        public abstract ulong? Position { get; }

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            var channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            var freq = Frequency;
            var omega = MathHelper.DoublePI * freq * SamplingFrequencyInverse;
            switch (channels)   //Unrolling assignations
            {
                case 1:
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = GenerateMonauralSample(Theta);
                        Theta = AppendTheta(omega);
                    }
                    return buffer.Length;
                case 2:
                    {
                        var m = Cast<float, Vector2>(buffer);
                        for (var i = 0; i < m.Length; i++)
                        {
                            m[i] = new Vector2(GenerateMonauralSample(Theta));
                            Theta = AppendTheta(omega);
                        }
                        return buffer.Length;
                    }
                case 3:
                    {
                        var m = Cast<float, Vector3>(buffer);
                        for (var i = 0; i < m.Length; i++)
                        {
                            m[i] = new Vector3(GenerateMonauralSample(Theta));
                            Theta = AppendTheta(omega);
                        }
                        return buffer.Length;
                    }
                case 4:
                    {
                        var m = Cast<float, Vector4>(buffer);
                        for (var i = 0; i < m.Length; i++)
                        {
                            m[i] = new Vector4(GenerateMonauralSample(Theta));
                            Theta = AppendTheta(omega);
                        }
                        return buffer.Length;
                    }
                default:
                    for (var i = 0; i < buffer.Length; i += channels)
                    {
                        var value = GenerateMonauralSample(Theta);
                        buffer.Slice(i, channels).FastFill(value);
                        Theta = AppendTheta(omega);
                    }
                    return buffer.Length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double AppendTheta(double omega) => ((Theta + omega + Math.PI) % MathHelper.DoublePI) - Math.PI;

        /// <summary>
        /// Generates the monaural sample.
        /// </summary>
        /// <param name="theta">The theta(from -pi to pi).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract float GenerateMonauralSample(double theta);

        #region IDisposable Support

        private bool disposedValue = false; //

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void DisposeInternal(bool disposing);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeInternal(disposing);

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
