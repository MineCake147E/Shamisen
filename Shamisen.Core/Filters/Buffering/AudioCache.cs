using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Filters.Buffering
{
    /// <summary>
    /// Caches some audio data like <see cref="DataCache{TSample}"/>.
    /// </summary>
    public sealed class AudioCache<TSample, TFormat> : IReadableAudioSource<TSample, TFormat> where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        private DataCache<TSample> cache;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioCache{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        public AudioCache(TFormat format)
        {
            Format = format;
            cache = new DataCache<TSample>();
        }

        /// <inheritdoc/>
        public TFormat Format { get; }

        /// <inheritdoc/>
        public ulong? Length { get; }

        /// <inheritdoc/>
        public ulong? TotalLength => cache.TotalLength is { } tlen ? tlen / (ulong)Format.Channels : null;

        /// <inheritdoc/>
        public ulong? Position { get; }

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<TSample> buffer) => cache.Read(buffer);

        /// <summary>
        /// Writes the data inside specified buffer to this instance.
        /// </summary>
        /// <param name="buffer">The data buffer.</param>
        public void Write(ReadOnlySpan<TSample> buffer) => cache.Write(buffer);

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cache.Dispose();
                }
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
