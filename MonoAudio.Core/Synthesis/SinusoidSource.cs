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
        public double Frequency { get; set; }

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

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public int Read(Span<float> buffer)
        {
            var channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            var freq = Frequency;
            var omega = MathHelper.DoublePI * freq * SamplingFrequencyInverse;
            switch (channels)   //Unrolling assignations
            {
                case 1:
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = (float)Math.Sin(Theta);
                        Theta = (Theta + omega) % MathHelper.DoublePI;
                    }
                    return buffer.Length;
                case 2:
                    {
                        var m = Cast<float, Vector2>(buffer);
                        for (int i = 0; i < m.Length; i++)
                        {
                            m[i] = new Vector2((float)Math.Sin(Theta));
                            Theta = (Theta + omega) % MathHelper.DoublePI;
                        }
                        return buffer.Length;
                    }
                case 3:
                    {
                        var m = Cast<float, Vector3>(buffer);
                        for (int i = 0; i < m.Length; i++)
                        {
                            m[i] = new Vector3((float)Math.Sin(Theta));
                            Theta = (Theta + omega) % MathHelper.DoublePI;
                        }
                        return buffer.Length;
                    }
                case 4:
                    {
                        var m = Cast<float, Vector4>(buffer);
                        for (int i = 0; i < m.Length; i++)
                        {
                            m[i] = new Vector4((float)Math.Sin(Theta));
                            Theta = (Theta + omega) % MathHelper.DoublePI;
                        }
                        return buffer.Length;
                    }
                default:
                    for (int i = 0; i < buffer.Length; i += channels)
                    {
                        float value = (float)Math.Sin(Theta);
                        buffer.Slice(i, channels).FastFill(value);
                        Theta = (Theta + omega) % MathHelper.DoublePI;
                    }
                    return buffer.Length;
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; //

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
