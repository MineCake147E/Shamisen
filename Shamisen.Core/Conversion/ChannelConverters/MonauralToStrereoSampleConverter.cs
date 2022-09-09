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
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            if (source.Format.Channels != 1) throw new ArgumentException("Channels must be 1!", nameof(source));
            bufferWrapper = new ResizablePooledBufferWrapper<float>(2048);
            Format = new SampleFormat(2, source.Format.SampleRate);
        }

        /// <inheritdoc cref="IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}.Source"/>
        public IReadableAudioSource<float, SampleFormat>? Source { get; }

        /// <inheritdoc/>
        public SampleFormat Format { get; }

        /// <inheritdoc/>
        public ulong? Length => Source?.Length;

        /// <inheritdoc/>
        public ulong? TotalLength => Source?.TotalLength;

        /// <inheritdoc/>
        public ulong? Position => Source?.Position;

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => Source?.SkipSupport;

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => Source?.SeekSupport;

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            if (Source is null) throw new ObjectDisposedException(nameof(MonauralToStrereoSampleConverter));
            buffer = buffer.SliceAlign(2);
            var internalBufferLengthRequired = CheckBuffer(buffer.Length / 2);
            var srcBuffer = bufferWrapper.Buffer.Slice(0, internalBufferLengthRequired);
            var readBuffer = srcBuffer;
            var rr = Source.Read(readBuffer);
            if (rr.HasData)
            {
                AudioUtils.DuplicateMonauralToStereo(buffer, readBuffer);
                return rr;
            }
            else
            {
                return rr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckBuffer(int sampleLengthOut)
        {
            var v = sampleLengthOut;
            var samplesRequired = v;
            var internalBufferLengthRequired = samplesRequired;
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
