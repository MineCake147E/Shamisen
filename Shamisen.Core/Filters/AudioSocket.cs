using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Filters;
using Shamisen.Synthesis;

namespace Shamisen
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

        /// <inheritdoc/>
        public TFormat Format { get; }

        /// <summary>
        /// Gets the source to read the samples from.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<TSample, TFormat>? Source { get; private set; }

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => Source?.SkipSupport;

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => Source?.SeekSupport;

        /// <inheritdoc/>
        public ulong? Length => Source?.Length;

        /// <inheritdoc/>
        public ulong? TotalLength => Source?.TotalLength;

        /// <inheritdoc/>
        public ulong? Position => Source?.Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioSocket{TSample, TFormat}"/> class with the specified <paramref name="format"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        public AudioSocket(TFormat format)
        {
            Format = format;
        }

        /// <inheritdoc/>
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
                throw new ArgumentException($"The Format is not same as {nameof(newSource)}'s Format!");
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
