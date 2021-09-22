using System;
using System.Buffers;

using Shamisen.Filters.Mixing.Advanced;
using Shamisen.Pipeline;

namespace Shamisen.Filters.Mixing
{
    /// <summary>
    /// Represents an item of <see cref="SimpleMixer"/> and <see cref="AdvancedMixer"/>.
    /// </summary>
    public sealed class MixerItem : IMixerItem
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MixerItem"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceProperties">The source properties.</param>
        /// <exception cref="ArgumentNullException">sourceProperties</exception>
        public MixerItem(ISampleSource source, AudioSourceProperties<float, SampleFormat> sourceProperties)
        {
            Source = source;
            SourceProperties = sourceProperties ?? throw new ArgumentNullException(nameof(sourceProperties));
            ActualSource = !sourceProperties.IsDynamic
                ? new StreamBuffer<float, SampleFormat>(Source, Format.GetBufferSizeRequired(TimeSpan.FromSeconds(SourceProperties.PreferredLatency)))
                : source;
            buffer = ArrayPool<float>.Shared.Rent(1024 * Format.Channels);
        }

        /// <inheritdoc/>
        public ISampleSource Source { get; }

        private IReadableAudioSource<float, SampleFormat> ActualSource { get; }

        /// <inheritdoc/>
        public float Volume { get; set; }

        /// <inheritdoc/>
        public AudioSourceProperties<float, SampleFormat> SourceProperties { get; }

        /// <inheritdoc/>
        public SampleFormat Format => Source.Format;

        /// <inheritdoc/>
        public ulong? Length => Source.Length;

        /// <inheritdoc/>
        public ulong? TotalLength => Source.TotalLength;

        /// <inheritdoc/>
        public ulong? Position => Source.Position;

        /// <inheritdoc/>
        ISkipSupport? IAudioSource<float, SampleFormat>.SkipSupport => Source.SkipSupport;

        /// <inheritdoc/>
        ISeekSupport? IAudioSource<float, SampleFormat>.SeekSupport => Source.SeekSupport;

        /// <inheritdoc/>
        public Memory<float> Buffer => buffer;

        private float[] buffer;

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            var res = ActualSource.Read(buffer);
            if (!res.HasData || res.Length != buffer.Length)
            {
                if (res.HasData)
                {
                    buffer.Slice(res.Length).FastFill(0);
                }
                else
                {
                    buffer.FastFill(0);
                }
            }
            return res;
        }

        /// <inheritdoc/>
        public void CheckBuffer(int length)
        {
            if (Buffer.Length < length)
            {
                var pool = ArrayPool<float>.Shared;
                pool.Return(buffer);
                buffer = pool.Rent(length);
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
                var pool = ArrayPool<float>.Shared;
                pool.Return(buffer);
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
