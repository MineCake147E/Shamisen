using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using MonoAudio.Mathematics;
using static System.Runtime.InteropServices.MemoryMarshal;

namespace MonoAudio.Synthesis
{
    /// <summary>
    /// Generates a sinusoid wave with specified frequency.
    /// </summary>
    public sealed class SinusoidSource : ISampleSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SinusoidSource"/> class.
        /// </summary>
        /// <param name="format">The output format.</param>
        public SinusoidSource(SampleFormat format)
        {
            SamplingFrequencyInverse = 1.0 / format.SampleRate;
            Format = format;
        }

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        public bool CanSeek => false;

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public SampleFormat Format { get; }

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
                Omega = Math.Abs(MathHelper.DoublePI * value * SamplingFrequencyInverse);
            }
        }

        private double SamplingFrequencyInverse { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        [Obsolete("Not Supported", true)]
        public long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}"/> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        public long Length => -1;

        private double Theta { get; set; } = 0;

        private double Omega { get; set; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<float> buffer)
        {
            var channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            var omega = Omega;
            var theta = Theta;
            switch (channels)   //Unrolling assignations
            {
                case 1:
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = GenerateMonauralSample(theta);
                        theta = AppendTheta(theta, omega);
                    }
                    Theta = theta;
                    return buffer.Length;
                case 2:
                    {
                        var m = Cast<float, Vector2>(buffer);
                        for (int i = 0; i < m.Length; i++)
                        {
                            m[i] = new Vector2(GenerateMonauralSample(theta));
                            theta = AppendTheta(theta, omega);
                        }
                        Theta = theta;
                        return buffer.Length;
                    }
                case 3:
                    {
                        var m = Cast<float, Vector3>(buffer);
                        for (int i = 0; i < m.Length; i++)
                        {
                            m[i] = new Vector3(GenerateMonauralSample(theta));
                            theta = AppendTheta(theta, omega);
                        }
                        Theta = theta;
                        return buffer.Length;
                    }
                case 4:
                    {
                        var m = Cast<float, Vector4>(buffer);
                        for (int i = 0; i < m.Length; i++)
                        {
                            m[i] = new Vector4(GenerateMonauralSample(theta));
                            theta = AppendTheta(theta, omega);
                        }
                        Theta = theta;
                        return buffer.Length;
                    }
                default:
                    for (int i = 0; i < buffer.Length; i += channels)
                    {
                        float value = GenerateMonauralSample(theta);
                        buffer.Slice(i, channels).FastFill(value);
                        theta = AppendTheta(theta, omega);
                    }
                    Theta = theta;
                    return buffer.Length;
            }
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

        /// <summary>
        /// Generates the monaural sample.
        /// </summary>
        /// <param name="theta">The theta(from -pi to pi).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GenerateMonauralSample(double theta) => (float)Math.Sin(theta);

        #region IDisposable Support

        private bool disposedValue = false; //
        private double frequency;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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
