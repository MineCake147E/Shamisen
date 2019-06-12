using System;
using MonoAudio.Conversion.Resampling;
using MonoAudio.Math;

namespace MonoAudio.Conversion.Resampling.SampleResampler
{
    public abstract class ResamplerBase : IResampler<float>
    {
        protected ResamplerBase(IReadableAudioSource<float> source, int destinationSampleRate)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (destinationSampleRate < 1) throw new ArgumentOutOfRangeException(nameof(destinationSampleRate), $"{nameof(destinationSampleRate)} must be grater than 0!");
            (RateMul, RateDiv) = MathHelper.SanitizeFraction(destinationSampleRate, source.Format.SampleRate);
        }

        public IReadableAudioSource<float> Source { get; }

        public virtual bool CanSeek => Source.CanSeek;

        public WaveFormat Format { get; }

        private long RateMul { get; }
        private long RateDiv { get; }

        public virtual long Position { get => Source.Position * RateMul / RateDiv; set => Source.Position = value * RateDiv / RateMul; }

        public long Length => Source.Length * RateMul / RateDiv;

        public abstract int Read(Span<float> buffer);

        #region IDisposable Support
        private bool disposedValue = false;

        //TODO: XML Comment
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                ActualDispose(disposing);

                disposedValue = true;
            }
        }

        protected abstract void ActualDispose(bool disposing);

        /// <summary>
        /// Releases all resource used by the
        /// <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/>. The <see cref="Dispose"/>
        /// method leaves the <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/> in an
        /// unusable state. After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/> so the garbage collector can
        /// reclaim the memory that the <see cref="T:MonoAudio.Conversion.Resampling.SampleResampler.ResamplerBase"/>
        /// was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
