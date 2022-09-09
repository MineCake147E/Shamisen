using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Conversion;

namespace Shamisen.Filters.Buffering
{
    /// <summary>
    /// Aligns the result of <see cref="IReadableAudioSource{TSample, TFormat}"/> with <see cref="IInterleavedAudioFormat{TSample}.BlockSize"/>.
    /// </summary>
    public sealed class BlockSizeReservoir<TSample, TFormat> : IAudioFilter<TSample, TFormat>
         where TSample : unmanaged
        where TFormat : IInterleavedAudioFormat<TSample>
    {
        private bool disposedValue;

        private readonly TSample[] buffer;
        private readonly UInt32Divisor blockSizeDivisor;
        private Memory<TSample> written;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockSizeReservoir{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public BlockSizeReservoir(IReadableAudioSource<TSample, TFormat> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            blockSizeDivisor = new UInt32Divisor((uint)source.Format.BlockSize);
            buffer = new TSample[source.Format.BlockSize];
            written = default;
        }

        /// <inheritdoc/>
        public IReadableAudioSource<TSample, TFormat> Source { get; }

        /// <inheritdoc/>
        public TFormat Format => Source.Format;

        /// <inheritdoc/>
        public ulong? Length => Source.Length;

        /// <inheritdoc/>
        public ulong? TotalLength => Source.TotalLength;

        /// <inheritdoc/>
        public ulong? Position => Source.Position;

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => null;

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => null;

        /// <inheritdoc/>
        public ReadResult Read(Span<TSample> buffer)
        {
            unchecked
            {
                var rem = buffer;
                var w = 0;
                if (!written.IsEmpty)
                {
                    written.CopyTo(buffer);
                    rem = buffer.Slice(written.Length);
                    w += written.Length;
                    written = default;
                }
                rem = rem.SliceAlign(blockSizeDivisor);
                var rr = Source.Read(rem);
                if (rr.HasNoData)
                {
                    if (w == 0)
                    {
                        return rr;
                    }
                    written = this.buffer.AsMemory(0, w);
                    return ReadResult.WaitingForSource;
                }
                w += rr.Length;
                var r = blockSizeDivisor.FloorRem((uint)w, out var len);
                if (r > 0)
                {
                    rem.Slice((int)len, (int)r).CopyTo(this.buffer.AsSpan());
                    written = this.buffer.AsMemory(0, (int)r);
                }

                return (int)len;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
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
