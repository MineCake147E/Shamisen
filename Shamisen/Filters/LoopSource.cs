using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Filters
{
    /// <summary>
    /// Provides a looping source for <see cref="IReadableAudioSource{TSample, TFormat}"/> with non-<see langword="null"/> <see cref="IAudioSource{TSample, TFormat}.TotalLength"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    /// <seealso cref="Shamisen.Filters.IAudioFilter{TSample, TFormat}" />
    public sealed class LoopSource<TSample, TFormat> : IAudioFilter<TSample, TFormat>
        where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue;
        private DataCache<TSample> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoopSource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public LoopSource(IReadableAudioSource<TSample, TFormat> source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
#pragma warning disable IDE0083
            if (!(source.TotalLength is ulong tlen)) throw new ArgumentException($"{nameof(source.TotalLength)} must not be null!", nameof(source));
#pragma warning restore IDE0083
            int allocationUnit = (int)Math.Min(tlen, 4096ul);
            cache = new(allocationUnit);
            var buffer = new TSample[allocationUnit];
            var span = buffer.AsSpan();
            while (source.Read(span) is { } rr && !rr.IsEndOfStream)
            {
                cache.Write(span);
            }
            cache.SeekTo(0);
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<TSample, TFormat> Source { get; }

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public TFormat Format => Source.Format;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public ulong? Length => Source.TotalLength - Position;

        /// <summary>
        /// Gets the total length.
        /// </summary>
        /// <value>
        /// The total length.
        /// </value>
        public ulong? TotalLength => null;

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong? Position => cache.ReadPosition;

        /// <summary>
        /// Gets the skip support.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport { get; }

        /// <summary>
        /// Gets the seek support.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<TSample> buffer)
        {
            int written = 0;
            var rem = buffer;
            while (!rem.IsEmpty)
            {
                var rr = cache.Read(rem);
                if (rr.Length < rem.Length)
                {
                    cache.SeekTo(0);
                }
                written += rr.Length;
                rem = rem.Slice(rr.Length);
            }
            return written;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                cache.Dispose();
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
