using System;
using System.Runtime.CompilerServices;

using DivideSharp;

using Shamisen.Filters;
using Shamisen.Mathematics;

namespace Shamisen.Conversion.Resampling.Sample
{
    /// <summary>
    /// The base of resampler.
    /// </summary>
    /// <seealso cref="IAudioFilter{TSample, TFormat}" />
    public abstract class ResamplerBase : IAudioFilter<float, SampleFormat>, ISampleSource
    {
        /// <inheritdoc/>
        public SampleFormat Format { get; }

        /// <inheritdoc cref="IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}.Source"/>
        public IReadableAudioSource<float, SampleFormat> Source { get; }

        /// <summary>
        /// Gets the channels of output.
        /// </summary>
        /// <value>
        /// The channels of output.
        /// </value>
        protected int Channels => Format.Channels;

        /// <summary>
        /// Gets the divisor for dividing a number by <see cref="Channels"/>.
        /// </summary>
        /// <value>
        /// The divisor object.
        /// </value>
        protected UInt32Divisor ChannelsDivisor { get; }

        /// <summary>
        /// Gets the divisor for converting source sample rate to destination sample rate.
        /// </summary>
        /// <value>
        /// The divisor object.
        /// </value>
        protected UInt32Divisor RateDivDivisor { get; }

        /// <summary>
        /// Gets the divisor for converting source sample rate to destination sample rate.
        /// </summary>
        /// <value>
        /// The divisor object.
        /// </value>
        protected UInt64Divisor RateDivDivisor64 { get; }

        /// <summary>
        /// Gets the rate source sample rate.
        /// </summary>
        /// <value>
        /// The number to divide with.
        /// </value>
        protected int RateDiv { get; }

        /// <summary>
        /// Gets the divisor source sample rate.
        /// </summary>
        /// <value>
        /// The number to multiply.
        /// </value>
        protected float RateDivInverse { get; }

        /// <summary>
        /// Gets the divisor for converting destination sample rate to source sample rate.
        /// </summary>
        /// <value>
        /// The divisor object.
        /// </value>
        protected UInt32Divisor RateMulDivisor { get; }

        /// <summary>
        /// Gets the rate destination sample rate.
        /// </summary>
        /// <value>
        /// The number to multiply.
        /// </value>
        protected int RateMul { get; }

        /// <summary>
        /// Gets the divisor destination sample rate.
        /// </summary>
        /// <value>
        /// The number to multiply.
        /// </value>
        protected float RateMulInverse { get; }

        /// <summary>
        /// Gets the number to increment conversion gradient.
        /// </summary>
        protected int GradientIncrement { get; }

        /// <summary>
        /// Gets the number to increment conversion source position.
        /// </summary>
        protected int IndexIncrement { get; }

        /// <inheritdoc/>
        public ulong? Length => Source.Length * (ulong)RateMul / RateDivDivisor64;

        /// <inheritdoc/>
        public ulong? TotalLength => Source.TotalLength * (ulong)RateMul / RateDivDivisor64;

        /// <inheritdoc/>
        public ulong? Position => Source.Position * (ulong)RateMul / RateDivDivisor64;

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResamplerBase"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destinationSampleRate">The destination sample rate.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        /// <exception cref="ArgumentOutOfRangeException">destinationSampleRate</exception>
        protected ResamplerBase(IReadableAudioSource<float, SampleFormat> source, int destinationSampleRate)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            Format = new SampleFormat(source.Format.Channels, destinationSampleRate);
            if (destinationSampleRate < 1) throw new ArgumentOutOfRangeException(nameof(destinationSampleRate), $"{nameof(destinationSampleRate)} must be grater than 0!");
            (RateMul, RateDiv) = MathHelper.MinimizeDivisor(destinationSampleRate, source.Format.SampleRate);
            RateMulInverse = 1.0f / RateMul;
            RateDivInverse = 1.0f / RateDiv;
            RateMulDivisor = new UInt32Divisor((uint)RateMul);
            RateDivDivisor = new UInt32Divisor((uint)RateDiv);
            RateDivDivisor64 = new UInt64Divisor((ulong)RateDiv);
            ChannelsDivisor = new UInt32Divisor((uint)Channels);
            GradientIncrement = (int)RateMulDivisor.DivRem((uint)RateDiv, out var indexIncrement);
            IndexIncrement = (int)indexIncrement;
            SkipSupport = source.SkipSupport?.WithFraction((ulong)RateDiv, (ulong)RateMul);
            SeekSupport = source.SeekSupport?.WithFraction((ulong)RateDiv, (ulong)RateMul);
        }

        #region Rate Conversion Utilities

        /// <summary>
        /// Gets the rounded input position further than -inf.
        /// </summary>
        /// <param name="outputPosition">The output position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetCeiledInputPosition(int outputPosition)
        {
            var m = (int)RateMulDivisor.DivRem((uint)(outputPosition * RateDiv), out var aa);
            var a = (int)aa;
            return m > 0 ? a + 1 : a;
        }

        /// <summary>
        /// Gets the rounded output position further than +inf.
        /// </summary>
        /// <param name="inputPosition">The input position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetFlooredOutputPosition(int inputPosition) => (int)RateDivDivisor.Divide((uint)(inputPosition * RateMul));

        /// <summary>
        /// Calculates the conversion gradient a little precisely.
        /// </summary>
        /// <param name="outputPosition">The output position that is not multiplied by <see cref="Channels"/>.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected (int position, float amount) GetConversionGradient(int outputPosition)
        {
            var mul = (uint)(outputPosition * RateDiv);
            var diff = RateMulDivisor.DivRem(mul, out mul);
            return ((int)mul, diff * RateMulInverse);
        }

        /// <summary>
        /// Calculates the conversion gradient.
        /// </summary>
        /// <param name="outputPosition">The output position that is not multiplied by <see cref="Channels"/>.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected (int position, int gradient) GetConversionPosition(int outputPosition)
        {
            var mul = (uint)(outputPosition * RateDiv);
            var diff = RateMulDivisor.DivRem(mul, out mul);
            return ((int)mul, (int)diff);
        }

        /// <summary>
        /// Gets the input buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="outputPosition">The output position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetInputPosition(int outputPosition) => (int)((uint)(outputPosition * RateDiv) / RateMulDivisor);

        /// <summary>
        /// Gets the input buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="outputPosition">The output position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long GetInputPosition(long outputPosition) => outputPosition * RateDiv / RateMul;

        /// <summary>
        /// Gets the input buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="outputPosition">The output position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetInputPosition(float outputPosition) => outputPosition * RateDiv * RateMulInverse;

        /// <summary>
        /// Gets the output buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="inputPosition">The input position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetOutputPosition(int inputPosition) => (int)((uint)(inputPosition * RateMul) / RateDivDivisor);

        /// <summary>
        /// Gets the output buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="inputPosition">The input position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long GetOutputPosition(long inputPosition) => inputPosition * RateMul / RateDiv;

        /// <summary>
        /// Gets the output buffer position.
        /// Supports Lighter rate conversion.
        /// </summary>
        /// <param name="inputPosition">The input position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetOutputPosition(float inputPosition) => inputPosition * RateMul * RateDivInverse;

        #endregion Rate Conversion Utilities

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public abstract ReadResult Read(Span<float> buffer);

        #region IDisposable Support

        private bool disposedValue = false;

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void ActualDispose(bool disposing);

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

        #endregion IDisposable Support
    }
}
