using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Conversion;
using Shamisen.Data;
using Shamisen.Formats;
using Shamisen.Utils;

namespace Shamisen.Filters
{
    /// <summary>
    /// Buffers the samples like YouTube does.<br/>
    /// It reads a little more than required, and prevents waiting for IOs / decoding.
    /// </summary>
    public sealed partial class StreamBuffer<TSample, TFormat> : IAudioFilter<TSample, TFormat>
        where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue = false;

        private PreloadDataBuffer<TSample> dataBuffer;

        /// <inheritdoc/>
        public TFormat Format => Source.Format;

        /// <inheritdoc cref="IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}.Source"/>
        public IReadableAudioSource<TSample, TFormat> Source { get; private set; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public ulong? Length => Source.Length;

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength => Source.TotalLength;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public ulong? Position => Source.Position;

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => null;

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => null;

        private UInt32Divisor blockSizeDivisor;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamBuffer{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="initialBlockSize">
        /// The size of initial buffer in Frames(independent on the number of channel and the type of sample).<br/>
        /// The buffer is automatically extended if the internal buffer is smaller than the size of reading buffers.
        /// </param>
        /// <param name="internalBufferNumber">The number of internal buffer.</param>
        /// <param name="allowWaitForRead">The value which indicates whether the <see cref="StreamBuffer{TSample, TFormat}"/> should wait for another sample block or not.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> should not be <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialBlockSize"/> should be larger than or equals to 2048.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="internalBufferNumber"/> should be larger than or equals to 16.</exception>
        public StreamBuffer(IReadableAudioSource<TSample, TFormat> source, int initialBlockSize, int internalBufferNumber = 16, bool allowWaitForRead = false)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (initialBlockSize < 2048) throw new ArgumentOutOfRangeException(nameof(initialBlockSize));
            if (internalBufferNumber < 16) throw new ArgumentOutOfRangeException(nameof(internalBufferNumber));
            dataBuffer = new PreloadDataBuffer<TSample>(new SampleDataSource<TSample, TFormat>(source), initialBlockSize, internalBufferNumber);
            if (Format is IInterleavedAudioFormat<TSample> nformat)
            {
                blockSizeDivisor = new((uint)nformat.BlockSize);
            }
        }

        /// <inheritdoc/>
        public ReadResult Read(Span<TSample> buffer)
        {
            buffer = buffer.SliceAlign(blockSizeDivisor);
            return dataBuffer.Read(buffer);
        }

        #region IDisposable Support

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                    // Block intentionally left empty.
                }
                dataBuffer.Dispose();
                //dataBuffer = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="StreamBuffer{TSample, TFormat}"/> class.
        /// </summary>
        ~StreamBuffer()
        {
            Dispose(false);
        }

        #endregion IDisposable Support
    }
}
