using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Formats;

namespace Shamisen.Synthesis
{
    /// <summary>
    /// Generates a silence.
    /// </summary>
    /// <seealso cref="IWaveSource" />
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
            ArgumentNullException.ThrowIfNull(format);
            Format = format;
        }

        /// <inheritdoc/>
        public IWaveFormat Format { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        [Obsolete("Not Supported", true)]
        public long Position { get; set; }

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => throw new NotImplementedException();

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => throw new NotImplementedException();

        ulong? IAudioSource<byte, IWaveFormat>.Length => null;

        ulong? IAudioSource<byte, IWaveFormat>.TotalLength => null;

        ulong? IAudioSource<byte, IWaveFormat>.Position => null;

        /// <inheritdoc/>
        public ReadResult Read(Span<byte> buffer)
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
