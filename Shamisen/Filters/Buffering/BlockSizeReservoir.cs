using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

namespace Shamisen.Filters.Buffering
{
    /// <summary>
    /// Aligns the result of <see cref="IReadableAudioSource{TSample, TFormat}"/> with <see cref="IInterleavedAudioFormat{TSample}.BlockSize"/>.
    /// </summary>
    public sealed class BlockSizeReservoir<TSample, TFormat> : IReadableAudioSource<TSample, TFormat>
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
            Source = source ?? throw new ArgumentNullException(nameof(source));
            blockSizeDivisor = new UInt32Divisor((uint)source.Format.BlockSize);
            buffer = new TSample[source.Format.BlockSize];
            written = default;
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<TSample, TFormat> Source { get; }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format => Source.Format;

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Length => Source.Length;

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength => Source.TotalLength;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}" /> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Position => Source.Position;

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample, TFormat}" />.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => null;

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample, TFormat}" />.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => null;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
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
                    written = written = this.buffer.AsMemory(0, w);
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
