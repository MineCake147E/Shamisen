using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data
{
    /// <summary>
    /// Translates calls between <see cref="IDataSource{TSample}"/> and <see cref="IReadableAudioSource{TSample, TFormat}"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public sealed class SampleDataSource<TSample, TFormat> : IDataSource<TSample>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue = false;

        private IReadableAudioSource<TSample, TFormat> source;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleDataSource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public SampleDataSource(IReadableAudioSource<TSample, TFormat> source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        /// Gets the remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? Length => source.Length;

        /// <summary>
        /// Gets the total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? TotalLength => source.TotalLength;

        /// <summary>
        /// Gets the position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? Position => source.Position;

        /// <summary>
        /// Gets the skip support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => source.SkipSupport;

        /// <summary>
        /// Gets the seek support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => source.SeekSupport;

        /// <summary>
        /// Gets the read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IReadSupport<TSample>? ReadSupport => source;

        /// <summary>
        /// Gets the asynchronous read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IAsyncReadSupport<TSample>? AsyncReadSupport => source as IAsyncReadSupport<TSample>;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    source.Dispose();
                }
                //source = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
