using System;
using System.Threading.Tasks;

namespace Shamisen.Data
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly

    /// <summary>
    /// Abstractions for the delegates below:<br/>
    /// <see cref="ReadFunc{TSample}"/>,
    /// <see cref="ReadAsyncFunc{TSample}"/>,
    /// <see cref="ReadWithParameterFunc{TSample, TParam}"/>, and
    /// <see cref="ReadWithParameterAsyncFunc{TSample, TParam}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    [Obsolete("Moving to IDataSource")]
    public abstract class DataReader<TSample> : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        private bool disposedValue = false;

        /// <summary>
        /// Reads the data asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length read.</returns>
        public abstract Task<int> ReadAsync(Memory<TSample> buffer);

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void DisposeInternal(bool disposing);

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeInternal(disposing);
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
