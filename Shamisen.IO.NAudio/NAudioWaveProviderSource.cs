using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

using Shamisen.Utils;
using Shamisen.Utils.Buffers;

namespace Shamisen.IO
{
    /// <summary>
    /// Provides <see cref="IWaveProvider"/>'s audio data to <see cref="Shamisen"/>-styled consumer.
    /// </summary>
    /// <seealso cref="IWaveSource" />
    public sealed class NAudioWaveProviderSource : IWaveSource
    {
        private bool disposedValue;
        private PooledArrayResizer<byte> internalBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NAudioWaveProviderSource"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public NAudioWaveProviderSource(IWaveProvider source)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            var sourceFormat = source.WaveFormat;
            internalBuffer = new(sourceFormat.BlockAlign * 2048);
            Format = sourceFormat.AsShamisenWaveFormat();
            SeekSupport = null;
            SkipSupport = null;
        }

        private IWaveProvider Source { get; }

        /// <inheritdoc/>
        public IWaveFormat Format { get; }

        /// <inheritdoc/>
        public ulong? Length { get; }

        /// <inheritdoc/>
        public ulong? TotalLength { get; }

        /// <inheritdoc/>
        public ulong? Position { get; }

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public ReadResult Read(Span<byte> buffer)
        {
            byte[]? ibuf = internalBuffer.Array ?? throw new ObjectDisposedException(nameof(NAudioWaveProviderSource));
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
                return ReadResult.WaitingForSource;
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
        /// Finalizes an instance of the <see cref="NAudioWaveProviderSource"/> class.
        /// </summary>
        ~NAudioWaveProviderSource()
        {
            Dispose(disposing: false);
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
