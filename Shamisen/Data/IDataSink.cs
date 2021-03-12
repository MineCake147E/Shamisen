using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure for writing data.
    /// </summary>
    /// <typeparam name="TSample">The type of writing data.</typeparam>
    /// <seealso cref="IDisposable" />
    public interface IDataSink<TSample> : IDisposable, IWriteSupport<TSample> where TSample : unmanaged
    {
        /// <summary>
        /// Gets the asynchronous write support.
        /// </summary>
        /// <value>
        /// The asynchronous write support.
        /// </value>
        IAsyncWriteSupport<TSample>? AsyncWriteSupport { get; }

        /// <summary>
        /// Gets the current size of written data.
        /// </summary>
        /// <value>
        /// The current size of written data.
        /// </value>
        ulong? CurrentSize { get; }

        /// <summary>
        /// Gets the remaining space of this <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <value>
        /// The remaining space.
        /// </value>
        ulong? RemainingSpace { get; }

        /// <summary>
        /// Gets the maximum size of this <see cref="IDataSink{TSample}"/>.<br/>
        /// The <see langword="null"/> means that maximum size is either not available, beyond <see cref="ulong.MaxValue"/>, or infinity.
        /// </summary>
        /// <value>
        /// The maximum size.
        /// </value>
        ulong? MaxSize { get; }

        /// <summary>
        /// Gets the current position of this <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        ulong? Position { get; }

        /// <summary>
        /// Gets the skip support of this <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        ISkipSupport? SkipSupport { get; }

        /// <summary>
        /// Gets the seek support of this <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        ISeekSupport? SeekSupport { get; }
    }
}
