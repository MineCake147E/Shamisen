using System;

using MonoAudio.Synthesis;

namespace MonoAudio.Benchmarks
{
    /*
    public sealed class DummySource<TSample, TFormat> : IReadableAudioSource<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue = false;

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample, TFormat}" /> supports seeking or not.
        /// </summary>
        public bool CanSeek => true;

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format { get; }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}" /> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        public long Length => -1;

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}" /> is.
        /// Some implementation could not support this property.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SilenceSource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        public DummySource(TFormat format) => Format = format;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public ReadResult Read(Span<TSample> buffer)
        {
            return buffer.Length;
        }

        #region IDisposable Support

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        #endregion IDisposable Support
    }
    */
}
