using System;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure of a write support of <see cref="IDataSink{TSample}"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    public interface IWriteSupport<TSample> where TSample : unmanaged
    {
        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The writing data.</param>
        void Write(ReadOnlySpan<TSample> data);
    }
}
