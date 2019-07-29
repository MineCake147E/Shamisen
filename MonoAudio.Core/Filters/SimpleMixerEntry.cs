using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Filters
{
    /// <summary>
    /// Represents a <see cref="SimpleMixer"/>'s entry.
    /// </summary>
    public sealed class MixerEntry : IDisposable
    {
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MixerEntry"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="volume">The volume.</param>
        /// <param name="disposeSource">if set to <c>true</c> [dispose source].</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public MixerEntry(IReadableAudioSource<float, SampleFormat> source, float volume, bool disposeSource)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Volume = volume;
            DisposeSource = disposeSource;
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<float, SampleFormat> Source { get; private set; }

        /// <summary>
        /// Gets or sets the volume.
        /// </summary>
        /// <value>
        /// The volume.
        /// </value>
        public float Volume { get; set; }

        /// <summary>
        /// Gets a value indicating whether to dispose source in <see cref="Dispose()"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> to dispose source in <see cref="Dispose()"/>; otherwise, <c>false</c>.
        /// </value>
        public bool DisposeSource { get; }

        #region IDisposable Support

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
                if (DisposeSource) Source?.Dispose();
                Source = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MixerEntry"/> class.
        /// </summary>
        ~MixerEntry()
        {
            Dispose(false);
        }

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
