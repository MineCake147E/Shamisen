using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Codecs.Waveform.Parsing.Chunks
{
    /// <summary>
    /// Parses "JUNK", "PAD ", and non-supported chunks.
    /// </summary>
    /// <seealso cref="IChunkParser" />
    public sealed class JunkChunkParser : IChunkParser
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="JunkChunkParser"/> class.
        /// </summary>
        /// <param name="chunkId">The chunk identifier.</param>
        /// <param name="chunkLength">Length of the chunk.</param>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public JunkChunkParser(ChunkId chunkId, ulong chunkLength, IChunkReader source)
        {
            ChunkId = chunkId;
            ChunkLength = chunkLength;
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            Source.Skip(source.RemainingBytes);
        }

        /// <summary>
        /// Gets the chunk identifier.
        /// </summary>
        /// <value>
        /// The chunk identifier.
        /// </value>
        public ChunkId ChunkId { get; }

        /// <summary>
        /// Gets the length of this chunk excluding header 8 bytes.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public ulong ChunkLength { get; }

        /// <summary>
        /// Gets the source chunk reader.
        /// </summary>
        /// <value>
        /// The source chunk reader.
        /// </value>
        public IChunkReader Source { get; private set; }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Source?.Dispose();
                }
                //Source = null;
                //
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
