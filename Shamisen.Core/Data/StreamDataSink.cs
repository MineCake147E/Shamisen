using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data
{
    /// <summary>
    /// Writes data to <see cref="Stream"/>.
    /// </summary>
    /// <seealso cref="IDataSink{TSample}" />
    public sealed class StreamDataSink : IDataSink<byte>
    {
        private bool disposedValue;

        /// <summary>
        /// Gets a value indicating whether the instance have to propagate <see cref="IDisposable.Dispose"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if  the instance have to propagate <see cref="IDisposable.Dispose"/>; otherwise, <c>false</c>.
        /// </value>
        public bool PropagateDispose { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDataSink"/> class.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="propagateDispose">The value indicating whether the instance have to propagate <see cref="IDisposable.Dispose"/>.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public StreamDataSink(Stream destination, bool propagateDispose = true)
        {
            this.destination = destination ?? throw new ArgumentNullException(nameof(destination));
            PropagateDispose = propagateDispose;
            if (!destination.CanWrite) throw new ArgumentException($"The {nameof(destination)} doesn't support writing!", nameof(destination));
            if (destination.CanSeek)
            {
                SeekSupport = new StreamSeekSupport(destination);
                CurrentSize = (ulong)destination.Length;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDataSink"/> class.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="propagateDispose">The value indicating whether the instance have to propagate <see cref="IDisposable.Dispose"/>.</param>
        /// <param name="disableSeek">The value which indicates whether the instance have to disable seek supports.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        internal StreamDataSink(Stream destination, bool propagateDispose, bool disableSeek)
        {
            this.destination = destination ?? throw new ArgumentNullException(nameof(destination));
            PropagateDispose = propagateDispose;
            if (!destination.CanWrite) throw new ArgumentException($"The {nameof(destination)} doesn't support writing!", nameof(destination));
            if (!disableSeek && destination.CanSeek)
            {
                SeekSupport = new StreamSeekSupport(destination);
                CurrentSize = (ulong)destination.Length;
            }
        }

        private Stream destination;

        /// <summary>
        /// Gets the asynchronous write support.
        /// </summary>
        /// <value>
        /// The asynchronous write support.
        /// </value>
        public IAsyncWriteSupport<byte>? AsyncWriteSupport { get; }

        /// <summary>
        /// Gets the position of the <see cref="IDataSink{TSample}" /> in number of <see cref="byte"/>s.<br/>
        /// The <c>null</c> means that the <see cref="IDataSink{TSample}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IDataSink{TSample}" /> in number of <see cref="byte"/>s.
        /// </value>
        public ulong? Position => !destination.CanSeek ? null : (ulong)destination.Position;

        /// <summary>
        /// Gets the skip support of the <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => SeekSupport;

        /// <summary>
        /// Gets the seek support of the <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Gets the current size of written data.
        /// </summary>
        /// <value>
        /// The current size of written data.
        /// </value>
        public ulong? CurrentSize { get; private set; }

        /// <summary>
        /// Gets the remaining space of this <see cref="IDataSink{TSample}" />.
        /// </summary>
        /// <value>
        /// The remaining space.
        /// </value>
        public ulong? RemainingSpace => MaxSize - CurrentSize;

        /// <summary>
        /// Gets the maximum size of this <see cref="IDataSink{TSample}" />.<br />
        /// The <see langword="null" /> means that maximum size is either not available, beyond <see cref="ulong.MaxValue" />, or infinity.
        /// </summary>
        /// <value>
        /// The maximum size.
        /// </value>
        public ulong? MaxSize { get; }

#if !NETSTANDARD2_0

        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The writing data.</param>
        public void Write(ReadOnlySpan<byte> data) => destination.Write(data);

#else
        private byte[] buffer = new byte[2048];
        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The writing data.</param>
        public void Write(ReadOnlySpan<byte> data)
        {
            var bufferSpan = buffer.AsSpan();
            if (data.Length <= bufferSpan.Length)
            {
                data.CopyTo(bufferSpan);
                destination.Write(buffer, 0, data.Length);
            }
            else
            {
                var d = data;
                while (!d.IsEmpty)
                {
                    if (d.Length <= bufferSpan.Length)
                    {
                        d.CopyTo(bufferSpan);
                        destination.Write(buffer, 0, d.Length);
                    }
                    else
                    {
                        d.SliceWhile(bufferSpan.Length).CopyTo(bufferSpan);
                        destination.Write(buffer, 0, buffer.Length);
                        d = d.Slice(bufferSpan.Length);
                    }
                }
            }
        }
#endif

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                if (PropagateDispose) destination.Dispose();
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
