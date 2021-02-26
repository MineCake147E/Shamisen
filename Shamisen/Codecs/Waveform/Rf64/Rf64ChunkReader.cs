using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Waveform.Parsing;
using Shamisen.Data;
using Shamisen.Data.Binary;

namespace Shamisen.Codecs.Waveform.Rf64
{
    /// <summary>
    /// Parses and reads RF64 Chunks.
    /// </summary>
    public sealed class Rf64ChunkReader : IChunkReader
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

        private IDataSource<byte> DataSource { get; set; }

        /// <summary>
        /// Gets the parser.
        /// </summary>
        /// <value>
        /// The parser.
        /// </value>
        public IRf64Parser Parser { get; }

        private bool HasParent => !(Parent is null);

        private Rf64ChunkReader? Parent { get; set; }

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource{TSample}" />.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong Position => TotalSize - RemainingBytes;

        /// <summary>
        /// Gets the total size of this chunk.
        /// </summary>
        /// <value>
        /// The total size.
        /// </value>
        public ulong TotalSize { get; private set; }

        private static readonly string ExceptionMessageOnIllegalRead = $"The {nameof(Rf64ChunkReader)} is occupied by sub chunk reader!";

        /// <summary>
        /// Initializes a new instance of the <see cref="Rf64ChunkReader" /> class that reads <see cref="ChunkId.Riff" /> and <see cref="ChunkId.Rf64" /> chunk.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="parser">The RF64 parser instance.</param>
        /// <param name="totalSizeSetter">A method to set the <see cref="TotalSize"/>.</param>
        /// <exception cref="ArgumentNullException">dataSource</exception>
        public Rf64ChunkReader(IDataSource<byte> dataSource, IRf64Parser parser, out StackOnlyActionContainer<ulong> totalSizeSetter)
        {
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
            Span<byte> buffer = stackalloc byte[8];
            dataSource.CheckRead(buffer);
            ChunkId = (ChunkId)BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            TotalSize = RemainingBytes = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(4));
            totalSizeSetter = TotalSize == uint.MaxValue
                ? new StackOnlyActionContainer<ulong>((size) =>
                {
                    TotalSize = size;
                    var read = RemainingBytes - uint.MaxValue;
                    RemainingBytes = read + TotalSize;
                })
                : new StackOnlyActionContainer<ulong>((size) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rf64ChunkReader" /> class that reads <see cref="ChunkId.Riff" /> and <see cref="ChunkId.Rf64" /> chunk.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="parser">The RF64 parser instance.</param>
        /// <exception cref="ArgumentNullException">dataSource</exception>
        public Rf64ChunkReader(IDataSource<byte> dataSource, IRf64Parser parser)
        {
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
            Span<byte> buffer = stackalloc byte[8];
            dataSource.CheckRead(buffer);
            ChunkId = (ChunkId)BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            TotalSize = RemainingBytes = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(4));
        }

        private Rf64ChunkReader(Rf64ChunkReader parent, IRf64Parser parser)
        {
            DataSource = Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
            Span<byte> buffer = stackalloc byte[8];
            parent.CheckRead(buffer);
            var chunkId = (ChunkId)BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            ChunkId = chunkId;
            ulong size = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(4));
            if (size == uint.MaxValue)
            {
                size = parser.GetSizeForNextChunk(chunkId);
            }
            TotalSize = RemainingBytes = size;
        }

        /// <summary>
        /// Reads the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public ReadResult Read(Span<byte> destination)
            => CanPubliclyRead ? ReadInternal(destination)
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
            var g = new Rf64ChunkReader(this, Parser);
            CurrentSubChunk = g;
            return g;
        }

        private ReadResult ReadFromSource(Span<byte> destination)
            => !(Parent is null) ? Parent.ReadInternal(destination) : DataSource.Read(destination);

        private async ValueTask<ReadResult> ReadFromSourceAsync(Memory<byte> destination)
            => !(Parent is null) ? await Parent.ReadInternalAsync(destination) : await DataSource.ReadAsync(destination);

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
            RemainingBytes -= numberOfElementsToSkip;
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
#pragma warning disable S1066 // Collapsible "if" statements should be merged
                    if (RemainingBytes > 0 && !(Parent is null))
#pragma warning restore S1066 // Collapsible "if" statements should be merged
                    {
                        Parent.Skip(RemainingBytes);
                    }
                }

                //DataSource = null;
                //Parent = null;
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
