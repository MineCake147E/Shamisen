using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Formats;

namespace MonoAudio.Synthesis
{
    /// <summary>
    /// Generates a silence.
    /// </summary>
    /// <seealso cref="MonoAudio.IWaveSource" />
    public sealed class SilenceWaveSource : IWaveSource
    {
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SilenceWaveSource"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <exception cref="ArgumentNullException">format</exception>
        public SilenceWaveSource(IWaveFormat format)
        {
            Format = format ?? throw new ArgumentNullException(nameof(format));
        }

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        public bool CanSeek { get; }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public IWaveFormat Format { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        [Obsolete("Not Supported", true)]
        public long Position { get; set; }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}"/> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public int Read(Span<byte> buffer)
        {
            buffer.FastFill();
            return buffer.Length;
        }

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

                //
                disposedValue = true;
            }
        }

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
