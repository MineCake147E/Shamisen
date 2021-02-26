using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Filters;
using Shamisen.Utils;

namespace Shamisen.Conversion.ChannelConverters
{
    /// <summary>
    /// Converts monaural audio to stereo.
    /// </summary>
    public sealed class MonauralToStrereoSampleConverter : IAudioFilter<float, SampleFormat>, ISampleSource
    {
        private bool disposedValue;
        private ResizableBufferWrapper<float> bufferWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonauralToStrereoSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public MonauralToStrereoSampleConverter(IReadableAudioSource<float, SampleFormat>? source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (source.Format.Channels != 1) throw new ArgumentException("Channels must be 1!", nameof(source));
            bufferWrapper = new ResizablePooledBufferWrapper<float>(2048);
            Format = new SampleFormat(2, source.Format.SampleRate);
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<float, SampleFormat>? Source { get; }

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public SampleFormat Format { get; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public ulong? Length => Source?.Length;

        /// <summary>
        /// Gets the total length.
        /// </summary>
        /// <value>
        /// The total length.
        /// </value>
        public ulong? TotalLength => Source?.TotalLength;

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong? Position => Source?.Position;

        /// <summary>
        /// Gets the skip support.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => Source?.SkipSupport;

        /// <summary>
        /// Gets the seek support.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => Source?.SeekSupport;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<float> buffer)
        {
            if (Source is null) throw new ObjectDisposedException(nameof(MonauralToStrereoSampleConverter));
            buffer = buffer.SliceAlign(2);
            int internalBufferLengthRequired = CheckBuffer(buffer.Length / 2);
            var srcBuffer = bufferWrapper.Buffer.Slice(0, internalBufferLengthRequired);
            var readBuffer = srcBuffer;
            var rr = Source.Read(readBuffer);
            if (rr.HasData)
            {
                unchecked
                {
                    var rb = readBuffer.SliceWhile(rr.Length);
                    var wb = MemoryMarshal.Cast<float, Vector2>(buffer).SliceWhile(rb.Length);
                    for (int i = 0; i < rb.Length; i++)
                    {
                        wb[i] = new Vector2(rb[i]);
                    }
                    return wb.Length * 2;
                }
            }
            else
            {
                return rr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckBuffer(int sampleLengthOut)
        {
            int v = sampleLengthOut;
            int samplesRequired = v;
            int internalBufferLengthRequired = samplesRequired;
            if (internalBufferLengthRequired > bufferWrapper.Buffer.Length)
            {
                ExpandBuffer(internalBufferLengthRequired);
            }

            return internalBufferLengthRequired;
        }

        private void ExpandBuffer(int internalBufferLengthRequired) => bufferWrapper.Resize(internalBufferLengthRequired);

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
