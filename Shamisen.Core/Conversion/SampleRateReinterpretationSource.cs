using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Filters;

namespace Shamisen.Conversion
{
    /// <summary>
    /// Makes the <see cref="IAudioFormat{TSample}.SampleRate"/> appear to be specified sample rate.
    /// </summary>
    public sealed class SampleRateReinterpretationSource : IAudioFilter<float, SampleFormat>, ISampleSource
    {
        private bool disposedValue;

        /// <inheritdoc/>
        public IReadableAudioSource<float, SampleFormat>? Source { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SampleRateReinterpretationSource"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destinationSampleRate">The target sample rate.</param>
        public SampleRateReinterpretationSource(IReadableAudioSource<float, SampleFormat> source, int destinationSampleRate)
        {
            Source = source;
            Format = new(source.Format.Channels, destinationSampleRate);
        }
        /// <inheritdoc/>
        public SampleFormat Format { get; }
        /// <inheritdoc/>
        public ulong? Length => Source?.Length;
        /// <inheritdoc/>
        public ulong? TotalLength => Source?.TotalLength;
        /// <inheritdoc/>
        public ulong? Position => Source?.Position;
        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => Source?.SkipSupport;
        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => Source?.SeekSupport;

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer) => Source?.Read(buffer) ?? throw new ObjectDisposedException(nameof(SampleRateReinterpretationSource));

        /// <inheritdoc/>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Source?.Dispose();
                }
                Source = null;
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        ~SampleRateReinterpretationSource()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
