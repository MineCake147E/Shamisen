using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Waveform.Parsing;
using Shamisen.Codecs.Waveform.Rf64;
using Shamisen.Data;
using Shamisen.Data.Binary;

namespace Shamisen.Codecs.Waveform
{
    /// <summary>
    /// Parses and reads RIFF Chunks.
    /// </summary>
    public sealed class RiffChunkReader : IChunkReader
    {
        private bool disposedValue = false;

        /// <summary>
        /// Gets a value indicating whether this instance can be read by <see cref="Read(Span{byte})"/> and <see cref="ReadAsync(Memory{byte})"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can publicly read; otherwise, <c>false</c>.
        /// </value>
        public bool CanPubliclyRead => CurrentSubChunk is null || CurrentSubChunk.RemainingBytes < 1;

        /// <summary>
        /// Gets the current chunk identifier.
        /// </summary>
        /// <value>
        /// The chunk identifier.
        /// </value>
        public ChunkId ChunkId { get; }

        /// <summary>
        /// Gets the current sub chunk.
        /// </summary>
        /// <value>
        /// The current sub chunk.
        /// </value>
        public IChunkReader? CurrentSubChunk { get; private set; }

        /// <summary>
        /// Gets the length of remaining data in bytes.
        /// </summary>
        /// <value>
        /// The remaining bytes.
        /// </value>
        public ulong RemainingBytes { get; private set; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IDataSource{TSample}"/> in bytes.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IDataSource{TSample}"/> in bytes.
        /// </value>
        public ulong? Length => RemainingBytes;

        /// <summary>
        /// Gets the total length of the <see cref="IDataSource{TSample}" /> in bytes.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IDataSource{TSample}" /> in bytes.
        /// </value>
        public ulong? TotalLength => TotalSize;

        /// <summary>
        /// Gets the position of the <see cref="IDataSource{TSample}" /> in bytes.<br/>
        /// The <c>null</c> means that the <see cref="IDataSource{TSample}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IDataSource{TSample}" /> in bytes.
        /// </value>
        public ulong? Position => TotalSize - RemainingBytes;

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
        public IAsyncReadSupport<byte>? AsyncReadSupport => null;

        private IReadableDataSource<byte> DataSource { get; set; }

        private bool HasParent => !(Parent is null);

        private RiffChunkReader? Parent { get; }

        /// <summary>
        /// Gets the total size of this chunk.
        /// </summary>
        /// <value>
        /// The total size.
        /// </value>
        public ulong TotalSize { get; }

        private static readonly string ExceptionMessageOnIllegalRead = $"The {nameof(RiffChunkReader)} is occupied by subchunk reader!";

        /// <summary>
        /// Initializes a new instance of the <see cref="RiffChunkReader"/> class that reads <see cref="ChunkId.Riff"/> chunk.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <exception cref="ArgumentNullException">dataSource</exception>
        public RiffChunkReader(IReadableDataSource<byte> dataSource)
        {
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            ChunkId = (ChunkId)DataSource.ReadUInt32LittleEndian();
            TotalSize = RemainingBytes = ChunkId switch
            {
                ChunkId.Riff => dataSource.ReadUInt32LittleEndian(),
                ChunkId.Rf64 => throw new ArgumentException($"Use {nameof(Rf64ChunkReader)} instead for decoding RF64 \"WAVE\" streams!"),
                _ => throw new ArgumentException("The specified dataSource has invalid WAVE data!"),
            };
        }

        private RiffChunkReader(RiffChunkReader parent)
        {
            DataSource = Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            ChunkId = (ChunkId)parent.ReadUInt32LittleEndian();
            TotalSize = RemainingBytes = parent.ReadUInt32LittleEndian();
        }

        /// <summary>
        /// Reads the specified destination.
        /// </summary>
        /// <param name="buffer">The destination.</param>
        /// <returns></returns>
        public ReadResult Read(Span<byte> buffer)
            => CanPubliclyRead ? ReadInternal(buffer)
            : throw new InvalidOperationException(ExceptionMessageOnIllegalRead);

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <see cref="byte" />s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
        public async ValueTask<ReadResult> ReadAsync(Memory<byte> destination)
            => CanPubliclyRead ? await ReadInternalAsync(destination)
            : throw new InvalidOperationException(ExceptionMessageOnIllegalRead);

        /// <summary>
        /// Opens the stream for sub chunk.
        /// </summary>
        /// <returns></returns>
        public IChunkReader ReadSubChunk()
        {
            if (!CanPubliclyRead) throw new InvalidOperationException(ExceptionMessageOnIllegalRead);
            var g = new RiffChunkReader(this);
            CurrentSubChunk = g;
            return g;
        }

        private ReadResult ReadFromSource(Span<byte> destination)
            => !(Parent is null) ? Parent.ReadInternal(destination) : DataSource.Read(destination);

        private async ValueTask<ReadResult> ReadFromSourceAsync(Memory<byte> destination)
            => !(Parent is null) ? await Parent.ReadInternalAsync(destination) : DataSource.AsyncReadSupport is { } s ? await s.ReadAsync(destination) : DataSource.Read(destination.Span);

        private ReadResult ReadInternal(Span<byte> destination)
        {
            if (RemainingBytes == 0) return ReadResult.EndOfStream;
            if (RemainingBytes > int.MaxValue)
            {
                var read = ReadFromSource(destination);
                if (read > 0)
                {
                    RemainingBytes -= (uint)read;
                    return read;
                }
                else
                {
                    return read;
                }
            }
            else
            {
                int rBytes = (int)RemainingBytes;
                if (destination.Length > rBytes)
                {
                    destination = destination.Slice(0, rBytes);
                }
                var read = ReadFromSource(destination);
                if (read > 0)
                {
                    RemainingBytes -= (uint)read;
                    return read;
                }
                else
                {
                    return read;
                }
            }
        }

        private async ValueTask<ReadResult> ReadInternalAsync(Memory<byte> destination)
        {
            if (RemainingBytes == 0) return ReadResult.EndOfStream;
            if (RemainingBytes > int.MaxValue)
            {
                var read = await ReadFromSourceAsync(destination);
                if (read.HasData)
                {
                    RemainingBytes -= (uint)read;
                    return read;
                }
                else
                {
                    return read;
                }
            }
            else
            {
                int rBytes = (int)RemainingBytes;
                if (destination.Length > rBytes)
                {
                    destination = destination.Slice(0, rBytes);
                }
                var read = await ReadFromSourceAsync(destination);
                if (read.HasData)
                {
                    RemainingBytes -= (uint)read;
                    return read;
                }
                else
                {
                    return read;
                }
            }
        }

        /// <summary>
        /// Skips this data source the specified number of elements to skip.
        /// </summary>
        /// <param name="numberOfElementsToSkip">The number of elements to skip.</param>
        public void Skip(ulong numberOfElementsToSkip)
        {
            if (!(Parent is null))
            {
                Parent.Skip(numberOfElementsToSkip);
            }
            else
            {
                DataSource.SkipWithFallback(numberOfElementsToSkip);
            }
        }

        #region IDisposable Support

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

                //DataSource = null;
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
