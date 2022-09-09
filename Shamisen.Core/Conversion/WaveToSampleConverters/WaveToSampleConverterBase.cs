﻿using System;
using System.Collections.Generic;
using System.Text;

using DivideSharp;

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts wave data to samples.
    /// </summary>
    public abstract class WaveToSampleConverterBase : IAudioConverter<byte, IWaveFormat, float, SampleFormat>, ISampleSource
    {
        /// <inheritdoc cref="IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}.Source"/>
        public IReadableAudioSource<byte, IWaveFormat> Source { get; }

        /// <inheritdoc/>
        public SampleFormat Format { get; }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected abstract int BytesPerSample { get; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public abstract ulong? Length { get; }

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public abstract ulong? TotalLength { get; }

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public abstract ulong? Position { get; }

        /// <inheritdoc/>
        public abstract ISkipSupport? SkipSupport { get; }

        /// <inheritdoc/>
        public abstract ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveToSampleConverterBase"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="format">The format.</param>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// format
        /// </exception>
        protected WaveToSampleConverterBase(IReadableAudioSource<byte, IWaveFormat> source, SampleFormat format)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            Format = format;
        }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public abstract ReadResult Read(Span<float> buffer);

        #region IDisposable Support

        /// <summary>
        /// The disposed value
        /// </summary>
        protected bool disposedValue = false;

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
