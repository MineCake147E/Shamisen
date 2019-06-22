using System;
using MonoAudio.Conversion.Resampling;
using MonoAudio.Formats;
using MonoAudio.MathUtils;

namespace MonoAudio.Conversion.Resampling.SampleResampler
{
    /// <summary>
    /// The base of resampler.
    /// </summary>
    /// <seealso cref="MonoAudio.Conversion.Resampling.IResampler{TSample, TFormat}" />
    public abstract class ResamplerBase : IResampler<float, SampleFormat>
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
            if (destinationSampleRate < 1) throw new ArgumentOutOfRangeException(nameof(destinationSampleRate), $"{nameof(destinationSampleRate)} must be grater than 0!");
            (RateMul, RateDiv) = MathHelper.MinimizeDivisor(destinationSampleRate, source.Format.SampleRate);
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

        private long RateMul { get; }

        private long RateDiv { get; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public virtual long Position { get => Source.Position * RateMul / RateDiv; set => Source.Position = value * RateDiv / RateMul; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length => Source.Length * RateMul / RateDiv;

        /// <summary>
        /// Reads the specified buffer.
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
