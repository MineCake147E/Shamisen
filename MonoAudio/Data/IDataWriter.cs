using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Data
{
    /// <summary>
    /// Defines a base infrastructure for writing data.
    /// </summary>
    /// <typeparam name="TSample">The type of writing data.</typeparam>
    /// <seealso cref="IDisposable" />
    public interface IDataSink<TSample> : IDisposable where TSample : unmanaged
    {
        /// <summary>
        /// Writes the specified <paramref name="data"/> asynchronously.
        /// </summary>
        /// <param name="data">The writing data.</param>
        /// <returns></returns>
        ValueTask WriteAsync(ReadOnlyMemory<TSample> data);

        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The writing data.</param>
        void Write(ReadOnlySpan<TSample> data);
    }

    /// <summary>
    /// Defines a base infrastructure for writing data, with seek functionality.
    /// </summary>
    public interface IDataWriter<TSample> : IDataSink<TSample> where TSample : unmanaged
    {
        /// <summary>
        /// Seeks this data writer to the specified index.
        /// </summary>
        /// <param name="position">The position to seek to, from the very first written <typeparamref name="TSample"/>.</param>
        void SeekTo(ulong position);

        /// <summary>
        /// Seeks this data writer with the specified offset and origin.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="origin">The origin.</param>
        void Seek(long offset, SeekOrigin origin);
    }
}
