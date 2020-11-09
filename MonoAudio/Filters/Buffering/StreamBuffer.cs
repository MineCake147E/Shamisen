using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MonoAudio.Data;
using MonoAudio.Formats;
using MonoAudio.Utils;

namespace MonoAudio.Filters
{
    /// <summary>
    /// Buffers the samples like YouTube does.<br/>
    /// It reads a little more than required, and prevents waiting for IOs / decoding.
    /// </summary>
    public sealed partial class StreamBuffer<TSample, TFormat> : IAudioFilter<TSample, TFormat>
        where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue = false;

        private DataBuffer<TSample> dataBuffer;

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        public bool CanSeek => Source.CanSeek;

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format => Source.Format;

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}"/> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        public long Length => Source.Length;

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        public long Position { get => Source.Position; set => Source.Position = value; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<TSample, TFormat> Source { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamBuffer{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="initialBlockSize">
        /// The size of initial buffer in Frames(independent on the number of channel and the type of sample).<br/>
        /// The buffer is automatically extended if the internal buffer is smaller than the size of reading buffers.
        /// </param>
        /// <param name="internalBufferNumber">The number of internal buffer.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> should not be <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialBlockSize"/> should be larger than or equals to 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="internalBufferNumber"/> should be larger than or equals to 2.</exception>
        public StreamBuffer(IReadableAudioSource<TSample, TFormat> source, int initialBlockSize, int internalBufferNumber = 4)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (initialBlockSize < 0) throw new ArgumentOutOfRangeException(nameof(initialBlockSize));
            if (internalBufferNumber < 2) throw new ArgumentOutOfRangeException(nameof(internalBufferNumber));
            dataBuffer = new DataBuffer<TSample>(new SampleDataSource<TSample, TFormat>(source), initialBlockSize, internalBufferNumber);
        }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<TSample> buffer)
        {
            buffer = buffer.SliceAlign(Format.Channels);
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
                dataBuffer = null;
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
