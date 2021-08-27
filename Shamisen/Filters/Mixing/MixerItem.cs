using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Filters.Mixing.Advanced;
using Shamisen.Pipeline;
using Shamisen.Utils;

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

        /// <summary>
        /// Gets the source of this <see cref="MixerItem"/>.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public ISampleSource Source { get; }

        private IReadableAudioSource<float, SampleFormat> ActualSource { get; }

        /// <summary>
        /// Gets the volume of this <see cref="MixerItem"/>.
        /// </summary>
        /// <value>
        /// The volume.
        /// </value>
        public float Volume { get; set; }

        /// <summary>
        /// Gets the source properties.
        /// </summary>
        /// <value>
        /// The source properties.
        /// </value>
        public AudioSourceProperties<float, SampleFormat> SourceProperties { get; }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public SampleFormat Format => Source.Format;

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </value>
        public ulong? Length => Source.Length;

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength => Source.TotalLength;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Position => Source.Position;

        ISkipSupport? IAudioSource<float, SampleFormat>.SkipSupport => Source.SkipSupport;

        ISeekSupport? IAudioSource<float, SampleFormat>.SeekSupport => Source.SeekSupport;

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        public Memory<float> Buffer => buffer;

        private float[] buffer;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
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

        /// <summary>
        /// Checks and stretches the buffer.
        /// </summary>
        /// <param name="length">The length.</param>
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
