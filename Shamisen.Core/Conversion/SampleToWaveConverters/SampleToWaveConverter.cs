using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Conversion.SampleToWaveConverters
{
    /// <summary>
    /// Converts samples to wave data.
    /// </summary>
    public abstract class SampleToWaveConverterBase : IAudioConverter<float, SampleFormat, byte, IWaveFormat>, IWaveSource
    {
        /// <inheritdoc cref="IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}.Source"/>
        public IReadableAudioSource<float, SampleFormat> Source { get; }

        /// <inheritdoc/>
        public IWaveFormat Format { get; }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected abstract int BytesPerSample { get; }

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => null;

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => null;

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

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public abstract ReadResult Read(Span<byte> buffer);

        #region IDisposable Support

        /// <summary>
        /// The disposed value
        /// </summary>
        protected bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToWaveConverterBase"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="format">The format.</param>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// format
        /// </exception>
        protected SampleToWaveConverterBase(IReadableAudioSource<float, SampleFormat> source, IWaveFormat format)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            ArgumentNullException.ThrowIfNull(format);
            Format = format;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
