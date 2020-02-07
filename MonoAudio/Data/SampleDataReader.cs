using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Data
{
    /// <summary>
    ///
    /// </summary>
    public sealed class SampleDataReader<TSample, TFormat> : ISynchronizedDataReader<TSample>, IDisposable
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue = false;

        private IReadableAudioSource<TSample, TFormat> source;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleDataReader{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public SampleDataReader(IReadableAudioSource<TSample, TFormat> source) => this.source = source ?? throw new ArgumentNullException(nameof(source));

        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public ReadResult Read(Span<TSample> buffer) => source.Read(buffer);

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
                source.Dispose();
                //

                disposedValue = true;
            }
        }

        // ~SampleDataReader()
        // {
        //   Dispose(false);
        // }

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
