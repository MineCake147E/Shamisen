using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Synthesis
{
    /// <summary>
    /// Implements an audio source that does nothing when the <see cref="Read(Span{TSample})"/> is called.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    /// <seealso cref="IReadableAudioSource{TSample, TFormat}" />
    public sealed class DummySource<TSample, TFormat> : IReadableAudioSource<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue = false;

        /// <inheritdoc/>
        public TFormat Format { get; }

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummySource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        public DummySource(TFormat format)
        {
            Format = format;
        }

        /// <inheritdoc/>
        public ulong? Length => null;

        /// <inheritdoc/>
        public ulong? TotalLength => null;

        /// <inheritdoc/>
        public ulong? Position => null;

        /// <inheritdoc/>
        public ReadResult Read(Span<TSample> buffer) => buffer.Length;

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

                //
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
