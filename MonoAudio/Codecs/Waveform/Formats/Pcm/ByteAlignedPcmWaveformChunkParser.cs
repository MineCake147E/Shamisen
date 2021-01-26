using System;
using System.Collections.Generic;
using System.Text;

using DivideSharp;

using MonoAudio.Codecs.Waveform.Parsing.Chunks;
using MonoAudio.Data;

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
        public ulong ChunkLength { get => Source.TotalSize; }

        /// <summary>
        /// Gets the source chunk reader.
        /// </summary>
        /// <value>
        /// The source chunk reader.
        /// </value>
        public IChunkReader Source { get; }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public IWaveFormat Format { get; }

        private UInt64Divisor BlockSizeDivisor { get; }

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport { get; }

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </value>
        public ulong? Length => TotalLength - Position;

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength => ChunkLength / BlockSizeDivisor;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Position => Source.Position / BlockSizeDivisor;

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
