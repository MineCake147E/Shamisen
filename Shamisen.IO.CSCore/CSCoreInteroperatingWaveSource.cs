using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShamisenWaveSource = Shamisen.IWaveSource;
using CSCore;

namespace Shamisen.IO
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
        public CSCoreInteroperatingWaveSource(ShamisenWaveSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            var format = source.Format;
            WaveFormat = ConvertToCSCoreWaveFormat(format);
        }

        internal static CSCore.WaveFormat ConvertToCSCoreWaveFormat(IWaveFormat format) => new CSCore.WaveFormat(format.SampleRate, format.BitDepth, format.Channels,
                                                       (CSCore.AudioEncoding)(short)format.Encoding, 0);

        /// <summary>
        /// Gets the source to read the audio from.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public ShamisenWaveSource Source { get; }

        /// <inheritdoc/>
        public CSCore.WaveFormat WaveFormat { get; }

        /// <summary>
        /// Gets or sets where the <see cref="CSCore.IWaveSource"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        public long Position
        {
            get => (long)(Source.Position ?? throw new NotSupportedException());
            set => Source.SeekSupport?.SeekTo((ulong)value);
        }

        /// <summary>
        /// Gets how long the <see cref="CSCore.IWaveSource"/> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        public long Length => (long)(Source.Length ?? throw new NotSupportedException());

        /// <summary>
        /// Gets a value indicating whether this instance can seek.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can seek; otherwise, <c>false</c>.
        /// </value>
        public bool CanSeek { get => !(Source.SeekSupport is null); }

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
