using System;
using System.Collections.Generic;
using System.Text;

using MonoAudio.Codecs.Waveform.Parsing.Chunks;

namespace MonoAudio.Codecs.Waveform.Formats.LinearPcm
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
        public ulong ChunkLength { get => Source.TotalSize; }

        /// <summary>
        /// Gets the source chunk reader.
        /// </summary>
        /// <value>
        /// The source chunk reader.
        /// </value>
        public IChunkReader Source { get; }

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        public bool CanSeek { get => false; }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public IWaveFormat Format { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}" /> is.
        /// Some implementation could not support this property.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        /// <exception cref="NotSupportedException">
        /// </exception>
        [Obsolete("Not Supported!", true)]
        public long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}"/> lasts in specific types.
        /// Negative value Means Infinity.
        /// </summary>
        public long Length { get => (long)ChunkLength; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
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
