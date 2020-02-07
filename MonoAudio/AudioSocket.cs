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
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        public long Position
        {
            get => Source?.Position ?? -1;
            set
            {
                if (!(Source is null)) Source.Position = value;
            }
        }

        /// <summary>
        /// Gets the source to read the samples from.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<TSample, TFormat> Source { get; private set; }

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        public bool CanSeek => Source?.CanSeek ?? false;

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}"/> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        public long Length => Source?.Length ?? -1;

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
        public ReadResult Read(Span<TSample> buffer) => Source.Read(buffer);

        /// <summary>
        /// Replaces the source to the <paramref name="newSource"/>.
        /// </summary>
        /// <param name="newSource">The new source.</param>
        /// <returns>The old <see cref="Source"/>.</returns>
        public IReadableAudioSource<TSample, TFormat> ReplaceSource(IReadableAudioSource<TSample, TFormat> newSource)
        {
            var result = Source;
            Source = newSource;
            return result;
        }

        /// <summary>
        /// De-plugs the source.
        /// </summary>
        /// <returns>The old <see cref="Source"/>.</returns>
        public IReadableAudioSource<TSample, TFormat> DeplugSource() => ReplaceSource(new SilenceSource<TSample, TFormat>(Format));

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
