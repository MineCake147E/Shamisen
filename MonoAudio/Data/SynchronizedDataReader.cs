using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Data
{
    /// <summary>
    /// Provides a synchronization for <see cref="DataReader{TSample}"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <seealso cref="ISynchronizedDataReader{TSample}" />
    /// <seealso cref="IDisposable" />
    public sealed partial class SynchronizedDataReader<TSample> : ISynchronizedDataReader<TSample>, IDisposable
    {
        private bool disposedValue = false;

        private DataReader<TSample> reader;

        private ISynchronizedDataReader<TSample> syncReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedDataReader{TSample}"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <exception cref="ArgumentNullException">reader</exception>
        public SynchronizedDataReader(DataReader<TSample> reader)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            syncReader = reader is ISynchronizedDataReader<TSample> synchronized ? synchronized : new AsyncStreamSynchronizer(reader);
        }

        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public ReadResult Read(Span<TSample> buffer) => syncReader.Read(buffer);

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
                reader.Dispose();
                reader = null;
                if (syncReader is IDisposable disposable) disposable.Dispose();
                syncReader = null;

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
