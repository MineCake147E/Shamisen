using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace MonoAudio.IO
{
    /// <summary>
    /// Inter-operate with CSCore to output the audio data.
    /// </summary>
    /// <seealso cref="CSCore.IWaveSource" />
    public sealed class CSCoreInteroperatingWaveSource : CSCore.IWaveSource
    {
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSCoreInteroperatingWaveSource"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public CSCoreInteroperatingWaveSource(IWaveSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            var format = source.Format;
            WaveFormat = ConvertToCSCoreWaveFormat(format);
        }

        internal static CSCore.WaveFormat ConvertToCSCoreWaveFormat(Formats.IWaveFormat format) => new CSCore.WaveFormat(format.SampleRate, format.BitDepth, format.Channels,
                                                       (CSCore.AudioEncoding)(short)format.Encoding, format.ExtraSize);

        /// <summary>
        /// Gets the source to read the audio from.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IWaveSource Source { get; }

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        public bool CanSeek => Source.CanSeek;

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public CSCore.WaveFormat WaveFormat { get; }

        /// <summary>
        /// Gets or sets where the <see cref="CSCore.IWaveSource"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        public long Position { get => Source.Position; set => Source.Position = value; }

        /// <summary>
        /// Gets how long the <see cref="CSCore.IWaveSource"/> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        public long Length => Source.Length;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset to overwrite the <paramref name="buffer"/>.</param>
        /// <param name="count">The number of bytes to overwrite the <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(byte[] buffer, int offset, int count) => Source.Read(new Span<byte>(buffer, offset, count));

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
