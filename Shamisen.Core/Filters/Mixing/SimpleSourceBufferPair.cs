using System;

using Shamisen.Utils.Buffers;

namespace Shamisen.Filters.Mixing
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SimpleSourceBufferPair : ISourceBufferPair
    {
        private bool disposedValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SimpleSourceBufferPair(ISampleSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            buffer = new(1024 * Format.Channels);
        }

        /// <inheritdoc/>
        public ISampleSource Source { get; }
        /// <inheritdoc/>
        public Memory<float> Buffer => buffer.Array.AsMemory();
        /// <inheritdoc/>
        public SampleFormat Format => Source.Format;
        /// <inheritdoc/>
        public ulong? Length => Source.Length;
        /// <inheritdoc/>
        public ulong? TotalLength => Source.TotalLength;
        /// <inheritdoc/>
        public ulong? Position => Source.Position;
        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => Source.SkipSupport;
        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => Source.SeekSupport;

        private PooledArrayResizer<float> buffer;

        /// <inheritdoc/>
        public void CheckBuffer(int length)
        {
            if (Buffer.Length < length)
            {
                _ = buffer.Resize(length);
            }
        }

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer) => Source.Read(buffer);

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                buffer.Dispose();
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        ~SimpleSourceBufferPair()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
