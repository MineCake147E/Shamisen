using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoAudio.Data.Binary;

namespace MonoAudio.Data.Parsing
{
    /// <summary>
    /// Parses binary data with internal buffer.<br/>
    /// It is more efficient than <see cref="DataReaderExtensions"/>'s methods.<br/>
    /// The <see cref="BufferedBinaryParser"/> won't <see cref="IDisposable.Dispose"/> the <see cref="Source"/> even if the parser gets disposed.
    /// </summary>
    /// <seealso cref="IDataSource{TSample}" />
    public sealed partial class BufferedBinaryParser : IDataSource<byte>
    {
        private bool disposedValue;

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource{TSample}" />.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong Position { get; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IDataSource<byte> Source { get; private set; }

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
        public BufferedBinaryParser(IDataSource<byte> source, int bufferLength = MinimumBufferLength)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (bufferLength < MinimumBufferLength)
                throw new ArgumentOutOfRangeException(nameof(bufferLength), $"The {nameof(bufferLength)} must be larger than or equals to {MinimumBufferLength}!");
            buffer = new byte[bufferLength];
            CheckRefill();
        }

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte"/>s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
        public ReadResult Read(Span<byte> destination)
        {
            if (isEof || disposedValue) return ReadResult.EndOfStream;
            if (!remainingData.IsEmpty)
            {
                if (remainingData.Length > destination.Length)
                {
                    remainingData.Span.SliceWhile(destination.Length).CopyTo(destination);
                    remainingData = remainingData.Slice(destination.Length);
                    CheckRefill();
                    return destination.Length;
                }
                else if (remainingData.Length == destination.Length)
                {
                    remainingData.Span.CopyTo(destination);
                    remainingData = Memory<byte>.Empty;
                    CheckRefill();
                    return destination.Length;
                }
                else
                {
                    remainingData.Span.CopyTo(destination);
                    var nd = destination.Slice(remainingData.Length);
                    var res = Source.Read(nd);
                    CheckRefill();
                    return res + remainingData.Length;
                }
            }
            else
            {
                ReadResult res = Source.Read(destination);
                CheckRefill();
                return res;
            }
        }

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte"/>s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
        public async ValueTask<ReadResult> ReadAsync(Memory<byte> destination)
        {
            if (isEof || disposedValue) return ReadResult.EndOfStream;
            if (!remainingData.IsEmpty)
            {
                if (remainingData.Length > destination.Length)
                {
                    remainingData.SliceWhile(destination.Length).CopyTo(destination);
                    remainingData = remainingData.Slice(destination.Length);
                    await CheckRefillAsync();
                    return destination.Length;
                }
                else if (remainingData.Length == destination.Length)
                {
                    remainingData.CopyTo(destination);
                    remainingData = Memory<byte>.Empty;
                    await CheckRefillAsync();
                    return destination.Length;
                }
                else
                {
                    remainingData.Span.CopyTo(destination);
                    var nd = destination.Slice(remainingData.Length);
                    var res = await Source.ReadAsync(nd);
                    await CheckRefillAsync();
                    return res + remainingData.Length;
                }
            }
            else
            {
                ReadResult res = await Source.ReadAsync(destination);
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
            if (remainingData.IsEmpty)
            {
                remainingData = buffer;
                var res = await Source.ReadAsync(remainingData);
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
                var res = await Source.ReadAsync(h);
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
