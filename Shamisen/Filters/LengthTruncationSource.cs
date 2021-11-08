using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

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
        public ulong? Length => TotalLength - Position;

        /// <summary>
        /// Gets the total length.
        /// </summary>
        /// <value>
        /// The total length.
        /// </value>
        public ulong? TotalLength { get; }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong? Position => position;

        /// <summary>
        /// Gets the skip support.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => Source.SkipSupport;

        /// <summary>
        /// Gets the seek support.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
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
