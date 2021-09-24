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
        public ISkipSupport? SkipSupport { get => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get => throw new NotImplementedException(); }

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
            unsafe
            {
                switch (sizeof(TSample))
                {
                    case 1:
                        var boffset = Unsafe.As<TSample, byte>(ref offset);
                        var bbuffer = MemoryMarshal.Cast<TSample, byte>(buffer);
                        bbuffer.FastFill(boffset);
                        break;
                    case 2:
                        var woffset = Unsafe.As<TSample, ushort>(ref offset);
                        var wbuffer = MemoryMarshal.Cast<TSample, ushort>(buffer);
                        wbuffer.FastFill(woffset);
                        break;
                    case 4:
                        var doffset = Unsafe.As<TSample, float>(ref offset);
                        var dbuffer = MemoryMarshal.Cast<TSample, float>(buffer);
                        dbuffer.FastFill(doffset);
                        break;
                    case 8:
                        var qoffset = Unsafe.As<TSample, double>(ref offset);
                        var qbuffer = MemoryMarshal.Cast<TSample, double>(buffer);
                        qbuffer.FastFill(qoffset);
                        break;
                    default:
                        buffer.QuickFill(offset);
                        break;
                }
            }
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
