using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

using DivideSharp;

using Shamisen.Mathematics;

using static System.Runtime.InteropServices.MemoryMarshal;

namespace Shamisen.Synthesis
{
    /// <summary>
    /// Generates a square wave with specified frequency.
    /// </summary>
    /// <seealso cref="ISampleSource" />
    public sealed class SquareWaveSource : ISampleSource, IFrequencyGeneratorSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SquareWaveSource"/> class.
        /// </summary>
        /// <param name="format">The output format.</param>
        public SquareWaveSource(SampleFormat format)
        {
            SamplingFrequencyInverse = 1.0 / format.SampleRate;
            Format = format;
            ChannelsDivisor = new(format.Channels);
        }

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
                Omega = (Fixed64)Math.Abs(2 * value * SamplingFrequencyInverse);
                OmegaDivisor = new((ulong)Omega.Value);
            }
        }

        private double SamplingFrequencyInverse { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        [Obsolete("Not Supported", true)]
        public long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        private Fixed64 Theta { get; set; } = Fixed64.Zero;

        private Fixed64 Omega { get; set; }

        private UInt64Divisor OmegaDivisor { get; set; }

        private Int32Divisor ChannelsDivisor { get; set; }

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport { get => null; }

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport { get => null; }

        ulong? IAudioSource<float, SampleFormat>.Length => null;

        ulong? IAudioSource<float, SampleFormat>.TotalLength => null;

        ulong? IAudioSource<float, SampleFormat>.Position => null;

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
            var omegaDivisor = OmegaDivisor;
            return Process(buffer, omega, theta, omegaDivisor, channels, ChannelsDivisor);
        }

        private ReadResult Process(Span<float> buffer, Fixed64 omega, Fixed64 theta, UInt64Divisor omegaDivisor, int channels, Int32Divisor channelsDivisor)
        {
            var rem = buffer;
            while (!rem.IsEmpty)
            {
                ulong d = GetDurationOfSameValue(omegaDivisor, theta);
                var q = d * (uint)channels;
                if (q > (uint)rem.Length)
                {
                    rem.FastFill(GenerateMonauralSample(theta));
                    theta += new Fixed64(omega.Value * (rem.Length / channelsDivisor));
                    rem = default;
                }
                else
                {
                    int q1 = (int)q;
                    rem.SliceWhile(q1).FastFill(GenerateMonauralSample(theta));
                    theta += new Fixed64(omega.Value * (long)d);
                    rem = rem.Slice(q1);
                }
            }
            Theta = theta;
            return buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong GetDurationOfSameValue(UInt64Divisor omega, Fixed64 theta)
        {
            var r = omega.DivRem(0x8000_0000_0000_0000u - (ulong)theta.Value % 0x8000_0000_0000_0000u, out var q);
            return r > 0 ? q + 1 : q;
        }

        /// <summary>
        /// Generates the monaural sample.
        /// </summary>
        /// <param name="theta">The theta(from -pi to pi).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GenerateMonauralSample(Fixed64 theta) => (float)((theta.Value >> 63) * 2 + 1);

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
