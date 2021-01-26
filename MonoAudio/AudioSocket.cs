using System;
using System.Collections.Generic;
using System.Text;

using MonoAudio.Filters;
using MonoAudio.Synthesis;

namespace MonoAudio
{
    /// <summary>
    /// The re-pluggable socket receiving audio from <see cref="IReadableAudioSource{TSample, TFormat}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    /// <seealso cref="IAudioFilter{TSample, TFormat}" />
    public sealed class AudioSocket<TSample, TFormat> : IAudioFilter<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue = false;

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format { get; }

        /// <summary>
        /// Gets the source to read the samples from.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<TSample, TFormat>? Source { get; private set; }

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => Source?.SkipSupport;

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => Source?.SeekSupport;

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </value>
        public ulong? Length => Source?.Length;

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength => Source?.TotalLength;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Position => Source?.Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioSocket{TSample, TFormat}"/> class with the specified <paramref name="format"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        public AudioSocket(TFormat format) => Format = format;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<TSample> buffer) => Source?.Read(buffer) ?? ReadResult.WaitingForSource;

        /// <summary>
        /// Replaces the source to the <paramref name="newSource"/>.
        /// </summary>
        /// <param name="newSource">The new source.</param>
        /// <returns>The <see cref="Source"/> that was previously set.</returns>
        /// <exception cref="ArgumentException">The Format is not same as newSource's Format!</exception>
        public IReadableAudioSource<TSample, TFormat>? ReplaceSource(IReadableAudioSource<TSample, TFormat> newSource)
        {
            if (newSource.Format.Equals(Format) && Format.Equals(newSource.Format))
            {
                var result = Source;
                Source = newSource;
                return result;
            }
            else
            {
                throw new ArgumentException($"The Format is not same as newSource's Format!");
            }
        }

        /// <summary>
        /// De-plugs the source.
        /// </summary>
        /// <returns>The old <see cref="Source"/>.</returns>
        public IReadableAudioSource<TSample, TFormat>? DeplugSource() => ReplaceSource(new SilenceSource<TSample, TFormat>(Format));

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
                    //
                }
                //Do not dispose actual Source
                Source = null;
                //

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
