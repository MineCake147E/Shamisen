using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

using NWaveFormat = NAudio.Wave.WaveFormat;

namespace Shamisen.IO
{
    /// <summary>
    /// Provides audio data to <see cref="IWavePlayer"/>.
    /// </summary>
    /// <seealso cref="IWaveProvider" />
    /// <seealso cref="IDisposable" />
    public sealed class NAudioInteroperatingWaveSource : IWaveProvider, IDisposable
    {
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="NAudioInteroperatingWaveSource"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public NAudioInteroperatingWaveSource(IWaveSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            WaveFormat = NWaveFormat.CreateCustomFormat(
                (WaveFormatEncoding)(short)source.Format.Encoding, source.Format.SampleRate, source.Format.Channels,
                source.Format.BlockSize * source.Format.SampleRate, source.Format.BlockSize, source.Format.BitDepth);
        }

        /// <summary>
        /// Gets the WaveFormat of this WaveProvider.
        /// </summary>
        /// <value>
        /// The wave format.
        /// </value>
        public NWaveFormat WaveFormat { get; }

        /// <summary>
        /// Gets the source to read the audio from.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IWaveSource Source { get; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset to overwrite the <paramref name="buffer"/>.</param>
        /// <param name="count">The number of bytes to overwrite the <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(byte[] buffer, int offset, int count) => Source.Read(new Span<byte>(buffer, offset, count)).Length;

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    Source.Dispose();

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
