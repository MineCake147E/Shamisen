using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonoAudio.Data;
using MonoAudio.Data.Binary;

namespace MonoAudio.Codecs.Waveform
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
        public bool CanPubliclyRead => CurrentSubChunk is null;

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
        public IChunkReader CurrentSubChunk { get; private set; }

        /// <summary>
        /// Gets the length of remaining data in bytes.
        /// </summary>
        /// <value>
        /// The remaining bytes.
        /// </value>
        public ulong RemainingBytes { get; private set; }

        private IDataSource DataSource { get; set; }

        private bool HasParent => !(Parent is null);

        private RiffChunkReader Parent { get; }

        /// <summary>
        /// Gets the total size of this chunk.
        /// </summary>
        /// <value>
        /// The total size.
        /// </value>
        public ulong TotalSize { get; }

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource" />.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong Position => TotalSize - RemainingBytes;

        private static readonly string ExceptionMessageOnIllegalRead = $"The {nameof(RiffChunkReader)} is occupied by subchunk reader!";

        /// <summary>
        /// Initializes a new instance of the <see cref="RiffChunkReader"/> class that reads <see cref="ChunkId.Riff"/> chunk.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <exception cref="ArgumentNullException">dataSource</exception>
        public RiffChunkReader(IDataSource dataSource)
        {
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            ChunkId = (ChunkId)DataSource.ReadUInt32LittleEndian();
            TotalSize = RemainingBytes = ChunkId switch
            {
                ChunkId.Riff => dataSource.ReadUInt32LittleEndian(),
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
        /// The number of <see cref="byte" />s read from this <see cref="IDataSource" />.
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
            => HasParent ? Parent.ReadInternal(destination) : DataSource.Read(destination);

        private async ValueTask<ReadResult> ReadFromSourceAsync(Memory<byte> destination)
            => HasParent ? await Parent.ReadInternalAsync(destination) : await DataSource.ReadAsync(destination);

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

                DataSource = null;
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
