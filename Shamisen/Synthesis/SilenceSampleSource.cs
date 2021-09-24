using System;

namespace Shamisen.Synthesis
{
    /// <summary>
    /// Generates a silence or DC offset with <see cref="SampleFormat"/>.<br/>
    /// This variant is specialized for <see cref="float"/> samples for performance reason.<br/>
    /// If the type of sample is known to be <see cref="float"/>, use this instead of <see cref="SilenceSource{TSample, TFormat}"/>.
    /// </summary>
    public sealed class SilenceSampleSource : ISampleSource
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SilenceSampleSource"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="offset">The DC offset to generate.</param>
        public SilenceSampleSource(SampleFormat format, float offset = default)
        {
            Format = format;
            Offset = offset;
        }

        /// <inheritdoc/>
        public SampleFormat Format { get; }
        /// <inheritdoc/>
        ulong? IAudioSource<float, SampleFormat>.Length => null;

        /// <inheritdoc/>
        ulong? IAudioSource<float, SampleFormat>.TotalLength => null;

        /// <inheritdoc/>
        ulong? IAudioSource<float, SampleFormat>.Position => null;

        /// <summary>
        /// Gets and sets the DC offset that this <see cref="SilenceSampleSource"/> generates.
        /// </summary>
        public float Offset { get; set; }


        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }
        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            var offset = Offset;
            buffer.FastFill(offset);
            return buffer.Length;
        }

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

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
