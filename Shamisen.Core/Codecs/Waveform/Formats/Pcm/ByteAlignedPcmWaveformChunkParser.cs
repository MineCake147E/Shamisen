using System;
using System.Collections.Generic;
using System.Text;

using DivideSharp;

using Shamisen.Codecs.Waveform.Parsing.Chunks;
using Shamisen.Data;

namespace Shamisen.Codecs.Waveform.Formats.LinearPcm
{
    /// <summary>
    /// Parses n-bit Linear and IEEE 754 floating point PCM data chunk.
    /// </summary>
    /// <seealso cref="IWaveformChunkParser" />
    public sealed class ByteAlignedPcmWaveformChunkParser : IWaveformChunkParser
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteAlignedPcmWaveformChunkParser"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="format">The format.</param>
        /// <exception cref="ArgumentNullException">
        /// source
        /// or
        /// format
        /// </exception>
        public ByteAlignedPcmWaveformChunkParser(IChunkReader source, IWaveFormat format)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Format = format ?? throw new ArgumentNullException(nameof(format));
            ChunkId = source.ChunkId;
            BlockSizeDivisor = new UInt64Divisor((ulong)format.BlockSize);
            SkipSupport = (Source as ISkipSupport).WithFraction(BlockSizeDivisor, 1);
            SeekSupport = (Source as ISeekSupport)?.WithFraction(BlockSizeDivisor.Divisor, 1);
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
        public ulong ChunkLength => Source.TotalSize;

        /// <summary>
        /// Gets the source chunk reader.
        /// </summary>
        /// <value>
        /// The source chunk reader.
        /// </value>
        public IChunkReader Source { get; }

        /// <inheritdoc/>
        public IWaveFormat Format { get; }

        private UInt64Divisor BlockSizeDivisor { get; }

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <inheritdoc/>
        public ulong? Length => TotalLength - Position;

        /// <inheritdoc/>
        public ulong? TotalLength => ChunkLength / BlockSizeDivisor;

        /// <inheritdoc/>
        public ulong? Position => Source.Position / BlockSizeDivisor;

        /// <inheritdoc/>
        public ReadResult Read(Span<byte> buffer) => Source.Read(buffer);

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                    Source.Dispose();
                }

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

        #endregion IDisposable Support
    }
}
