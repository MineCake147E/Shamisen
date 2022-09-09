using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Conversion;
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
            var allocationUnit = (int)Math.Min(tlen, 4096ul);
            cache = new(allocationUnit);
            var buffer = new TSample[allocationUnit];
            var span = buffer.AsSpan();
            while (source.Read(span) is { } rr && !rr.IsEndOfStream)
            {
                cache.Write(span);
            }
            cache.SeekTo(0);
        }

        /// <inheritdoc cref="IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}.Source"/>
        public IReadableAudioSource<TSample, TFormat> Source { get; }

        /// <inheritdoc/>
        public TFormat Format => Source.Format;

        /// <inheritdoc/>
        public ulong? Length => Source.TotalLength - Position;

        /// <inheritdoc/>
        public ulong? TotalLength => null;

        /// <inheritdoc/>
        public ulong? Position => cache.ReadPosition;

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <inheritdoc/>
        public ReadResult Read(Span<TSample> buffer)
        {
            var written = 0;
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
