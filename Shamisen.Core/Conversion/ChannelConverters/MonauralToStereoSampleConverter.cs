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
    public sealed class MonauralToStereoSampleConverter : IAudioFilter<float, SampleFormat>, ISampleSource
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonauralToStereoSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public MonauralToStereoSampleConverter(IReadableAudioSource<float, SampleFormat>? source)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            if (source.Format.Channels != 1) throw new ArgumentException("Channels must be 1!", nameof(source));
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
            if (Source is null) throw new ObjectDisposedException(nameof(MonauralToStereoSampleConverter));
            buffer = buffer.SliceAlign(2);
            var length = buffer.Length / 2;
            var readBuffer = buffer.SliceFromEnd(length);
            var rr = Source.Read(readBuffer);
            if (rr.HasData)
            {
                //It is guaranteed that DuplicateMonauralToStereo doesn't overwrite readBuffer while readBuffer is inside latter half of buffer or outside buffer.
                AudioUtils.DuplicateMonauralToStereo(buffer, readBuffer);
                return rr * 2;
            }
            else
            {
                return rr;
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
