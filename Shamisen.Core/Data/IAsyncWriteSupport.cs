using System;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure of an asynchronous write support of <see cref="IDataSink{TSample}"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    public interface IAsyncWriteSupport<TSample> where TSample : unmanaged
    {
        /// <summary>
        /// Writes the specified data asynchronously.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ValueTask WriteAsync(ReadOnlyMemory<TSample> data);
    }
}
