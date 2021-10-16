using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

using Shamisen.Utils.Buffers;

namespace Shamisen.IO
{
    /// <summary>
    /// Provides <see cref="WaveStream"/>'s audio data to <see cref="Shamisen"/>-styled consumer.
    /// </summary>
    /// <seealso cref="IWaveSource" />
    public sealed class NAudioWaveStreamSource : IWaveSource
    {
        private bool disposedValue;
        private PooledArrayResizer<byte> internalBuffer;
        private WaveStream Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NAudioWaveStreamSource"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public NAudioWaveStreamSource(WaveStream source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            var sourceFormat = source.WaveFormat;
            internalBuffer = new(sourceFormat.BlockAlign * 2048);
            Format = sourceFormat.AsShamisenWaveFormat();
            SeekSupport = null;
            SkipSupport = null;
        }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public IWaveFormat Format { get; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </value>
        public ulong? Length { get; }

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength => (ulong)Source.Length;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Position => (ulong)Source.Position;

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
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<byte> buffer)
        {
            byte[]? ibuf = internalBuffer.Array ?? throw new ObjectDisposedException(nameof(NAudioWaveStreamSource));
            if (ibuf.Length < buffer.Length)
            {
                ibuf = ResizeBuffer(buffer.Length);
            }
            int i = Source.Read(ibuf, 0, ibuf.Length);
            if (i > 0)
            {
                ibuf.AsSpan(0, buffer.Length).CopyTo(buffer);
                return i;
            }
            else
            {
                return Source.Position < Source.Length ? ReadResult.WaitingForSource : ReadResult.EndOfStream;
            }
        }

        private byte[] ResizeBuffer(int newSize)
        {
            internalBuffer.Array.AsSpan().FastFill();
            return internalBuffer.Resize(newSize);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                internalBuffer.Dispose();
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
