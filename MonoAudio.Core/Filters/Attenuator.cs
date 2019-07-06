using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Formats;

namespace MonoAudio.Filters
{
    /// <summary>
    /// Modifies the velocity of <see cref="Source"/>
    /// </summary>
    /// <seealso cref="Filters.IAudioFilter{TSample, TFormat}" />
    public sealed class Attenuator : IAudioFilter<float, SampleFormat>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Attenuator"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public Attenuator(IReadableAudioSource<float, SampleFormat> source) => Source = source ?? throw new ArgumentNullException(nameof(source));

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<float, SampleFormat> Source { get; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public float Scale { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance can seek.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can seek; otherwise, <c>false</c>.
        /// </value>
        public bool CanSeek => Source.CanSeek;

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public SampleFormat Format => Source.Format;

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public long Position { get => Source.Position; set => Source.Position = value; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length => Source.Length;

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public int Read(Span<float> buffer)
        {
            var r = Source.Read(buffer);
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
