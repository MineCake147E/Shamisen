using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Shamisen.Data
{
    /// <summary>
    /// Reads data from <see cref="Stream"/>
    /// </summary>
    public sealed class StreamDataSource : IReadableDataSource<byte>, IAsyncReadSupport<byte>
    {
        private Stream source;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDataSource"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <exception cref="ArgumentNullException">stream</exception>
        public StreamDataSource(Stream stream)
        {
            source = stream ?? throw new ArgumentNullException(nameof(stream));
            if (stream.CanSeek) SeekSupport = new StreamSeekSupport(stream);
        }

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong PositionOld => (ulong)source.Position;

        /// <summary>
        /// Gets the remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IDataSource{TSample}"/> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? Length => !source.CanSeek ? null : TotalLength - Position;

        /// <summary>
        /// Gets the total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? TotalLength => !source.CanSeek ? null : (ulong)source.Length;

        /// <summary>
        /// Gets the position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IDataSource{TSample}" /> in number of <typeparamref name="TSample"/>.
        /// </value>
        public ulong? Position => !source.CanSeek ? null : (ulong)source.Position;

        /// <summary>
        /// Gets the skip support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => SeekSupport;

        /// <summary>
        /// Gets the seek support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Gets the read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IReadSupport<byte>? ReadSupport => this;

        /// <summary>
        /// Gets the asynchronous read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IAsyncReadSupport<byte>? AsyncReadSupport => this;

#if !NETSTANDARD2_0

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="buffer">The destination.</param>
        /// <returns>The number of <see cref="byte"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        public ReadResult Read(Span<byte> buffer)
        {
            var rr = source.Read(buffer);
            return rr < 1 ? ReadResult.EndOfStream : rr;
        }

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="buffer">The destination.</param>
        /// <returns>The number of <see cref="byte"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        public async ValueTask<ReadResult> ReadAsync(Memory<byte> buffer) => new ReadResult(await source.ReadAsync(buffer));

#else
        private byte[] buffer = new byte[2048];

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <typeparamref name="TSample"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        public ReadResult Read(Span<byte> destination)
        {
            if (destination.Length < buffer.Length)
            {
                var res = source.Read(buffer, 0, destination.Length);
                switch (res)
                {
                    case > 0:
                        buffer.AsSpan().SliceWhile(res).CopyTo(destination);
                        return res;
                    default:
                        return ReadResult.EndOfStream;
                }
            }
            else
            {
                var d = destination;
                int res = 0;
                while (!d.IsEmpty)
                {
                    var rres = source.Read(buffer, 0, Math.Min(d.Length, buffer.Length));
                    buffer.AsSpan().SliceWhile(rres).CopyTo(d);
                    d = d.Slice(rres);
                    res += rres;
                    if (rres == 0)
                    {
                        return res == 0 ? ReadResult.EndOfStream : res;
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <typeparamref name="TSample"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        public async ValueTask<ReadResult> ReadAsync(Memory<byte> destination) => Read(destination.Span);

#endif

        #region SeekSupport

        private sealed class StreamSeekSupport : ISeekSupport, IDisposable
        {
            private Stream source;
            private bool disposedValue;

            public StreamSeekSupport(Stream source) => this.source = source ?? throw new ArgumentNullException(nameof(source));

            /// <summary>
            /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> with the specified offset in frames.
            /// </summary>
            /// <param name="offset">The offset in frames.</param>
            /// <param name="origin">The origin.</param>
            public void Seek(long offset, SeekOrigin origin) => source.Seek(offset, origin);

            /// <summary>
            /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> to the specified index in frames from the end of stream.
            /// </summary>
            /// <param name="offset">The offset.</param>
            public void SeekLast(ulong offset) => source.Seek((long)offset, SeekOrigin.End);

            /// <summary>
            /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> to the specified index in frames.
            /// </summary>
            /// <param name="index">The index in frames.</param>
            public void SeekTo(ulong index) => source.Seek((long)index, SeekOrigin.Begin);

            /// <summary>
            /// Skips the source the specified step in frames.
            /// </summary>
            /// <param name="step">The number of frames to skip.</param>
            public void Skip(ulong step) => source.Seek((long)step, SeekOrigin.Current);

            /// <summary>
            /// Steps this data source the specified step back in frames.
            /// </summary>
            /// <param name="step">The number of frames to step back.</param>
            public void StepBack(ulong step) => source.Seek(-(long)step, SeekOrigin.Current);

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        //
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    source.Dispose();
                    (SeekSupport as IDisposable)?.Dispose();
                }
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

        #endregion SeekSupport
    }
}
