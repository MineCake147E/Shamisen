using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure of a source of binary data.
    /// </summary>
    public interface IDataSource<TSample> : IDisposable where TSample : unmanaged
    {
        /// <summary>
        /// Gets the read support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        IReadSupport<TSample>? ReadSupport { get; }

        /// <summary>
        /// Gets the asynchronous read support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        IAsyncReadSupport<TSample>? AsyncReadSupport { get; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.
        /// </value>
        ulong? Length { get; }

        /// <summary>
        /// Gets the total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        ulong? TotalLength { get; }

        /// <summary>
        /// Gets the position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        ulong? Position { get; }

        /// <summary>
        /// Gets the skip support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        ISkipSupport? SkipSupport { get; }

        /// <summary>
        /// Gets the seek support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        ISeekSupport? SeekSupport { get; }
    }

    /// <summary>
    /// Defines a base infrastructure of a source of binary data, which supports skipping.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <seealso cref="IDataSource{TSample}" />
    [Obsolete("Moving to ISkipSupport")]
    public interface ISkippableDataSource<TSample> : IDataSource<TSample>, ISkipSupport where TSample : unmanaged
    {
    }

    /// <summary>
    /// Defines a base infrastructure of a source of binary data, which supports seeking.
    /// </summary>
    /// <seealso cref="IDataSource{TSample}" />
    [Obsolete("Moving to ISeekSupport")]
    public interface ISeekableDataSource<TSample> : ISkippableDataSource<TSample>, ISeekSupport where TSample : unmanaged
    {
    }
}
