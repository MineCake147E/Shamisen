using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Conversion;

namespace Shamisen.Filters
{
    /// <summary>
    /// Truncates some infinitely-long <see cref="IReadableAudioSource{TSample, TFormat}"/> with specified length.
    /// </summary>
    public sealed class LengthTruncationSource<TSample, TFormat> : IAudioFilter<TSample, TFormat>
        where TSample : unmanaged where TFormat : IInterleavedAudioFormat<TSample>
    {
        private bool disposedValue;
        private ulong position = 0;
        private UInt32Divisor blockSizeDivisorU32;

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthTruncationSource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="totalLength">The total length.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public LengthTruncationSource(IReadableAudioSource<TSample, TFormat> source, ulong totalLength)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            TotalLength = Math.Min(source.TotalLength ?? totalLength, totalLength);
            blockSizeDivisorU32 = new UInt32Divisor((uint)source.Format.BlockSize);
        }

        /// <inheritdoc cref="IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}.Source"/>
        public IReadableAudioSource<TSample, TFormat> Source { get; }

        /// <inheritdoc/>
        public TFormat Format => Source.Format;

        /// <inheritdoc/>
        public ulong? Length => TotalLength - Position;

        /// <inheritdoc/>
        public ulong? TotalLength { get; }

        /// <inheritdoc/>
        public ulong? Position => position;

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => Source.SkipSupport;

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => Source.SeekSupport;

        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<TSample> buffer)
        {
            if (position >= TotalLength) return ReadResult.EndOfStream;
            buffer = buffer.SliceAlign(blockSizeDivisorU32);
            var pinc = (uint)buffer.Length / blockSizeDivisorU32;
            if (Length < pinc)
            {
                buffer = buffer.SliceWhileIfLongerThan((int)Length * Format.BlockSize);
            }
            var rr = Source.Read(buffer);
            var ppnc = (uint)rr.Length / blockSizeDivisorU32;
            position += ppnc;
            return rr;
        }

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
