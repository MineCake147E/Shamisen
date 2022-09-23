using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Filters;
using Shamisen.Utils;

namespace Shamisen.Conversion.ChannelConverters
{
    /// <summary>
    /// Converts monaural audio to stereo.
    /// </summary>
    public sealed class MonauralSampleDuplicator : IAudioFilter<float, SampleFormat>, ISampleSource
    {
        private bool disposedValue;

        private Int32Divisor ChannelsDivisor { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonauralSampleDuplicator"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="newChannels">The number of channels of output.</param>
        public MonauralSampleDuplicator(IReadableAudioSource<float, SampleFormat>? source, int newChannels)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            if (source.Format.Channels != 1) throw new ArgumentException("Channels must be 1!", nameof(source));
            if (newChannels < 1) throw new ArgumentException($"The {nameof(newChannels)} must be greater than 0!", nameof(source));
            Format = new SampleFormat(newChannels, source.Format.SampleRate);
            ChannelsDivisor = new(newChannels);
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
            var chd = ChannelsDivisor;
            if (Source is null) throw new ObjectDisposedException(nameof(MonauralSampleDuplicator));
            if (chd.Divisor != 1)
            {
                buffer = buffer.SliceAlign(chd);
                var length = buffer.Length / chd;
                var readBuffer = buffer.SliceFromEnd(length);
                var rr = Source.Read(readBuffer);
                if (rr.HasData)
                {
                    //It is guaranteed that DuplicateMonauralToChannels doesn't overwrite readBuffer while readBuffer is inside latter half of buffer or outside buffer.
                    var channels = chd.Divisor;
                    AudioUtils.DuplicateMonauralToChannels(buffer, readBuffer, channels);
                    return rr * channels;
                }
                else
                {
                    return rr;
                }
            }
            else
            {
                return Source.Read(buffer);
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
