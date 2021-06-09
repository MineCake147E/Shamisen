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

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format { get; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </value>
        public ulong? Length { get; }

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength { get; }

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Position { get; }

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
