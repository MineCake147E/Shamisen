using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Filters.Mixing
{
    /// <summary>
    /// Mixes down two signal into one signal.
    /// </summary>
    /// <seealso cref="ISampleSource" />
    [Obsolete("Undone!", true)]
    public sealed class SimpleMixer : ISampleSource
    {
        private bool disposedValue = false;

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        public bool CanSeek { get; }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public SampleFormat Format { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}"/> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Gets the item A.
        /// </summary>
        /// <value>
        /// The item A.
        /// </value>
        public MixerItem ItemA { get; }

        /// <summary>
        /// Gets the item B.
        /// </summary>
        /// <value>
        /// The item B.
        /// </value>
        public MixerItem ItemB { get; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<float> buffer)
        {
            return ReadResult.WaitingForSource;
        }

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
                    //
                }

                //
                disposedValue = true;
            }
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
