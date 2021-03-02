using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure of a read support of <see cref="IDataSource{TSample}"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    public interface IReadSupport<TSample> where TSample : unmanaged
    {
        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        ReadResult Read(Span<TSample> buffer);
    }

    /// <summary>
    /// Defines a base infrastructure of an asynchronous read support of <see cref="IDataSource{TSample}"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    public interface IAsyncReadSupport<TSample> where TSample : unmanaged
    {
        /// <summary>
        /// Reads the data to the specified buffer asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        ValueTask<ReadResult> ReadAsync(Memory<TSample> buffer);
    }
}
