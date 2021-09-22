using System;

using Shamisen.Utils;

namespace Shamisen.Filters
{
    /// <summary>
    /// Modifies the velocity of <see cref="Source"/>
    /// </summary>
    /// <seealso cref="IAudioFilter{TSample, TFormat}" />
    public sealed class Attenuator : IAudioFilter<float, SampleFormat>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Attenuator"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public Attenuator(IReadableAudioSource<float, SampleFormat> source) => Source = source ?? throw new ArgumentNullException(nameof(source));

        /// <inheritdoc/>
        public IReadableAudioSource<float, SampleFormat> Source { get; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public float Scale { get; set; }

        /// <inheritdoc/>
        public SampleFormat Format => Source.Format;

        /// <inheritdoc/>
        public ulong? Length => Source.Length;

        /// <inheritdoc/>
        public ulong? TotalLength => Source.TotalLength;

        /// <inheritdoc/>
        public ulong? Position => Source.Position;

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => Source.SkipSupport;

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => Source.SeekSupport;

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            var rr = Source.Read(buffer);
            if (rr.HasNoData) return rr;
            var r = rr.Length;
            buffer.Slice(0, r).FastScalarMultiply(Scale);
            return r;
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
                Source.Dispose();
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
