using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts wave data to samples.
    /// </summary>
    public abstract class WaveToSampleConverterBase : IAudioConverter<byte, float>, ISampleSource
    {
        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<byte> Source { get; }

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource" /> supports seeking or not.
        /// </summary>
        public bool CanSeek => Source.CanSeek;

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public WaveFormat Format { get; }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected abstract int BytesPerSample { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource" /> is.
        /// Some implementation could not support this property.
        /// </summary>
        public long Position { get => Source.Position / BytesPerSample; set => Source.Position = value * BytesPerSample; }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource" /> lasts in samples.
        /// -1 Means Infinity.
        /// </summary>
        public long Length => Source.Length / BytesPerSample;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public abstract int Read(Span<float> buffer);

        #region IDisposable Support

        /// <summary>
        /// The disposed value
        /// </summary>
        protected bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveToSampleConverterBase"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="format">The format.</param>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// format
        /// </exception>
        protected WaveToSampleConverterBase(IReadableAudioSource<byte> source, WaveFormat format)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Format = format ?? throw new ArgumentNullException(nameof(format));
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
