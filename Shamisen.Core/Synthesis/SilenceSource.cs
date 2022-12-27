using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shamisen.Synthesis
{
    /// <summary>
    /// Generates a silence or DC offset.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    /// <seealso cref="IReadableAudioSource{TSample, TFormat}" />
    public sealed class SilenceSource<TSample, TFormat> : IReadableAudioSource<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue = false;

        /// <inheritdoc/>
        public TFormat Format { get; }

        /// <inheritdoc/>
        public long Position { get; set; }

        /// <summary>
        /// Gets and sets the DC offset that this <see cref="SilenceSource{TSample, TFormat}"/> generates.
        /// </summary>
        public TSample Offset { get; set; }

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => throw new NotImplementedException();

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => throw new NotImplementedException();

        /// <inheritdoc/>
        ulong? IAudioSource<TSample, TFormat>.Length => null;

        /// <inheritdoc/>
        ulong? IAudioSource<TSample, TFormat>.TotalLength => null;

        /// <inheritdoc/>
        ulong? IAudioSource<TSample, TFormat>.Position => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SilenceSource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="offset">The DC offset to generate.</param>
        public SilenceSource(TFormat format, TSample offset = default)
        {
            Format = format;
            Offset = offset;
        }

        /// <inheritdoc/>
        public ReadResult Read(Span<TSample> buffer)
        {
            var offset = Offset;
            buffer.QuickFill(offset);
            return buffer.Length;
        }

        #region IDisposable Support

        /// <inheritdoc/>
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

                //
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
