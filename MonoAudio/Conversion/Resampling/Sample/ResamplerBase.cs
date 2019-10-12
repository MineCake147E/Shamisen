using System;
using System.Runtime.CompilerServices;
using MonoAudio.Filters;
using MonoAudio.Mathematics;

namespace MonoAudio.Conversion.Resampling.Sample
{
    /// <summary>
    /// The base of resampler.
    /// </summary>
    /// <seealso cref="Filters.IAudioFilter{TSample, TFormat}" />
    public abstract class ResamplerBase : IAudioFilter<float, SampleFormat>, ISampleSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResamplerBase"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destinationSampleRate">The destination sample rate.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        /// <exception cref="ArgumentOutOfRangeException">destinationSampleRate</exception>
        protected ResamplerBase(IReadableAudioSource<float, SampleFormat> source, int destinationSampleRate)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Format = new SampleFormat(source.Format.Channels, destinationSampleRate);
            if (destinationSampleRate < 1) throw new ArgumentOutOfRangeException(nameof(destinationSampleRate), $"{nameof(destinationSampleRate)} must be grater than 0!");
            (RateMul, RateDiv) = MathHelper.MinimizeDivisor(destinationSampleRate, source.Format.SampleRate);
            RateMulInverse = 1.0f / RateMul;
            RateDivInverse = 1.0f / RateDiv;
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<float, SampleFormat> Source { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can seek.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can seek; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CanSeek => Source.CanSeek;

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public SampleFormat Format { get; }

        /// <summary>
        /// Gets the rate destination sample rate.
        /// </summary>
        /// <value>
        /// The number to multiply.
        /// </value>
        private int RateMul { get; }

        /// <summary>
        /// Gets the rate source sample rate.
        /// </summary>
        /// <value>
        /// The number to divide with.
        /// </value>
        private int RateDiv { get; }

        /// <summary>
        /// Gets the divisor destination sample rate.
        /// </summary>
        /// <value>
        /// The number to multiply.
        /// </value>
        private float RateMulInverse { get; }

        /// <summary>
        /// Gets the divisor source sample rate.
        /// </summary>
        /// <value>
        /// The number to multiply.
        /// </value>
        private float RateDivInverse { get; }

        /// <summary>
        /// Gets the channels of output.
        /// </summary>
        /// <value>
        /// The channels of output.
        /// </value>
        protected int Channels => Format.Channels;

        #region Rate Conversion Utilities

        /// <summary>
        /// Gets the output buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="inputPosition">The input position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetOutputPosition(int inputPosition) => inputPosition * RateMul / RateDiv;

        /// <summary>
        /// Gets the input buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="outputPosition">The output position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetInputPosition(int outputPosition) => outputPosition * RateDiv / RateMul;

        /// <summary>
        /// Gets the output buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="inputPosition">The input position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long GetOutputPosition(long inputPosition) => inputPosition * RateMul / RateDiv;

        /// <summary>
        /// Gets the input buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="outputPosition">The output position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long GetInputPosition(long outputPosition) => outputPosition * RateDiv / RateMul;

        /// <summary>
        /// Gets the output buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="inputPosition">The input position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetOutputPosition(float inputPosition) => inputPosition * RateMul * RateDivInverse;

        /// <summary>
        /// Gets the input buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="outputPosition">The output position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetInputPosition(float outputPosition) => outputPosition * RateDiv * RateMulInverse;

        /// <summary>
        /// Calculates the conversion gradient a little precisely.
        /// </summary>
        /// <param name="outputPosition">The output position. Must not be multiplied by <see cref="Channels"/>.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected (int position, float amount) GetConversionGradient(int outputPosition)
        {
            var mul = outputPosition * RateDiv;
            var diff = mul % RateMul;
            mul -= diff;
            return (mul / RateMul, diff * RateMulInverse);
        }

        /// <summary>
        /// Gets the ceilinged input position.
        /// </summary>
        /// <param name="outputPosition">The output position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetCeiledInputPosition(int outputPosition)
        {
            var a = Math.DivRem(outputPosition * RateDiv, RateMul, out int m);
            return m > 0 ? a + 1 : a;
        }

        #endregion Rate Conversion Utilities

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public virtual long Position { get => GetOutputPosition(Source.Position); set => Source.Position = value * RateDiv / RateMul; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length => GetOutputPosition(Source.Length);

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public abstract int Read(Span<float> buffer);

        #region IDisposable Support

        private bool disposedValue = false;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                ActualDispose(disposing);

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void ActualDispose(bool disposing);

        /// <summary>
        /// Releases all resource used by the
        /// <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose()"/> when you are finished using the
        /// <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/>. The <see cref="Dispose()"/>
        /// method leaves the <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/> in an
        /// unusable state. After calling <see cref="Dispose()"/>, you must release all references to the
        /// <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/> so the garbage collector can
        /// reclaim the memory that the <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/>
        /// was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
