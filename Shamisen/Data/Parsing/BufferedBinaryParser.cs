using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data.Binary;

namespace Shamisen.Data.Parsing
{
    /// <summary>
    /// Parses binary data with internal buffer.<br/>
    /// It is more efficient than <see cref="DataReaderExtensions"/>'s methods.<br/>
    /// The <see cref="BufferedBinaryParser"/> won't <see cref="IDisposable.Dispose"/> the <see cref="Source"/> even if the parser gets disposed.
    /// </summary>
    /// <seealso cref="IDataSource{TSample}" />
    public sealed partial class BufferedBinaryParser : IReadableDataSource<byte>, IAsyncReadSupport<byte>
    {
        private bool disposedValue;

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableDataSource<byte> Source { get; private set; }

        /// <summary>
        /// The minimum buffer length
        /// </summary>
        public const int MinimumBufferLength = sizeof(ulong) * 8;

        private const int RefillThreshold = sizeof(ulong);

        private byte[] buffer;
        private Memory<byte> remainingData;
        private bool isEof = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedBinaryParser"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="bufferLength">The length of internal buffer, which must be larger than or equals to <see cref="MinimumBufferLength"/>.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public BufferedBinaryParser(IReadableDataSource<byte> source, int bufferLength = MinimumBufferLength)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (bufferLength < MinimumBufferLength)
                throw new ArgumentOutOfRangeException(nameof(bufferLength), $"The {nameof(bufferLength)} must be larger than or equals to {MinimumBufferLength}!");
            buffer = new byte[bufferLength];
            CheckRefill();
        }

        /// <summary>
        /// Gets the remaining length of the <see cref="IDataSource{TSample}"/> in bytes.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IDataSource{TSample}"/> in bytes.
        /// </value>
        public ulong? Length => TotalLength - Position;

        /// <summary>
        /// Gets the total length of the <see cref="IDataSource{TSample}" /> in bytes.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IDataSource{TSample}" /> in bytes.
        /// </value>
        public ulong? TotalLength => Source.TotalLength;

        /// <summary>
        /// Gets the position of the <see cref="IDataSource{TSample}" /> in bytes.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IDataSource{TSample}" /> in bytes.
        /// </value>
        public ulong? Position => Source.Position - (uint)remainingData.Length;

        /// <summary>
        /// Gets the skip support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => null;

        /// <summary>
        /// Gets the seek support of the <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => null;

        /// <summary>
        /// Gets the read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IReadSupport<byte>? ReadSupport => this;

        /// <summary>
        /// Gets the asynchronous read support of the <see cref="IDataSource{TSample}" />.
        /// </summary>
        public IAsyncReadSupport<byte>? AsyncReadSupport => this;

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="buffer">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte"/>s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
        public ReadResult Read(Span<byte> buffer)
        {
            if (isEof || disposedValue) return ReadResult.EndOfStream;
            if (!remainingData.IsEmpty)
            {
                if (remainingData.Length > buffer.Length)
                {
                    remainingData.Span.SliceWhile(buffer.Length).CopyTo(buffer);
                    remainingData = remainingData.Slice(buffer.Length);
                    CheckRefill();
                    return buffer.Length;
                }
                else if (remainingData.Length == buffer.Length)
                {
                    remainingData.Span.CopyTo(buffer);
                    remainingData = Memory<byte>.Empty;
                    CheckRefill();
                    return buffer.Length;
                }
                else
                {
                    remainingData.Span.CopyTo(buffer);
                    var nd = buffer.Slice(remainingData.Length);
                    var res = Source.Read(nd);
                    CheckRefill();
                    return res + remainingData.Length;
                }
            }
            else
            {
                ReadResult res = Source.Read(buffer);
                CheckRefill();
                return res;
            }
        }

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="buffer">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte"/>s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
        public async ValueTask<ReadResult> ReadAsync(Memory<byte> buffer)
        {
            if (Source.AsyncReadSupport is null) return Read(buffer.Span);
            if (isEof || disposedValue) return ReadResult.EndOfStream;
            if (!remainingData.IsEmpty)
            {
                if (remainingData.Length > buffer.Length)
                {
                    remainingData.SliceWhile(buffer.Length).CopyTo(buffer);
                    remainingData = remainingData.Slice(buffer.Length);
                    await CheckRefillAsync();
                    return buffer.Length;
                }
                else if (remainingData.Length == buffer.Length)
                {
                    remainingData.CopyTo(buffer);
                    remainingData = Memory<byte>.Empty;
                    await CheckRefillAsync();
                    return buffer.Length;
                }
                else
                {
                    remainingData.Span.CopyTo(buffer);
                    var nd = buffer.Slice(remainingData.Length);
                    var res = await Source.AsyncReadSupport.ReadAsync(nd);
                    await CheckRefillAsync();
                    return res + remainingData.Length;
                }
            }
            else
            {
                var res = await Source.AsyncReadSupport.ReadAsync(buffer);
                await CheckRefillAsync();
                return res;
            }
        }

        private void CheckRefill()
        {
            if (remainingData.Length > RefillThreshold) return;
            FillBuffer();
        }

        private async ValueTask CheckRefillAsync()
        {
            if (remainingData.Length > RefillThreshold) return;
            await FillBufferAsync();
        }

        private void FillBuffer()
        {
            if (remainingData.IsEmpty)
            {
                remainingData = buffer;
                var res = Source.Read(remainingData.Span);
                if (res.IsEndOfStream)
                {
                    isEof = true;
                    return;
                }
                remainingData = remainingData.SliceWhile(res.Length);
            }
            else
            {
                var M = buffer.AsMemory();
                remainingData.CopyTo(M);
                var h = M.Slice(remainingData.Length);
                var res = Source.Read(h.Span);
                if (res.IsEndOfStream)
                {
                    isEof = true;
                    return;
                }
                remainingData = M.SliceWhile(remainingData.Length + res.Length);
            }
        }

        private async ValueTask FillBufferAsync()
        {
            if (Source.AsyncReadSupport is null)
            {
                FillBuffer();
                return;
            }
            if (remainingData.IsEmpty)
            {
                remainingData = buffer;
                var res = await Source.AsyncReadSupport.ReadAsync(remainingData);
                if (res.IsEndOfStream)
                {
                    isEof = true;
                    return;
                }
                remainingData = remainingData.SliceWhile(res.Length);
            }
            else
            {
                var M = buffer.AsMemory();
                remainingData.CopyTo(M);
                var h = M.Slice(remainingData.Length);
                var res = await Source.AsyncReadSupport.ReadAsync(h);
                if (res.IsEndOfStream)
                {
                    isEof = true;
                    return;
                }
                remainingData = M.SliceWhile(remainingData.Length + res.Length);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                //buffer = null;
                remainingData = Memory<byte>.Empty;
                //Source = null;
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
