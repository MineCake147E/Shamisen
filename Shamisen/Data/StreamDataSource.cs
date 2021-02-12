using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Shamisen.Data
{
    /// <summary>
    /// Reads data from <see cref="Stream"/>
    /// </summary>
    public sealed class StreamDataSource : ISeekableDataSource<byte>
    {
        private Stream source;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDataSource"/> class.
        /// </summary>
        /// <param name="steram">The steram.</param>
        /// <exception cref="ArgumentNullException">steram</exception>
        public StreamDataSource(Stream steram) => this.source = steram ?? throw new ArgumentNullException(nameof(steram));

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource{TSample}"/>.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong Position => (ulong)source.Position;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => source.Dispose();

#if !NETSTANDARD2_0

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <see cref="byte"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        public ReadResult Read(Span<byte> destination) => source.Read(destination);

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <see cref="byte"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        public async ValueTask<ReadResult> ReadAsync(Memory<byte> destination) => new ReadResult(await source.ReadAsync(destination));

#else
        private byte[] buffer = new byte[2048];

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <typeparamref name="TSample"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        public ReadResult Read(Span<byte> destination)
        {
            if (destination.Length < buffer.Length)
            {
                var res = source.Read(buffer, 0, destination.Length);
                buffer.AsSpan().SliceWhile(res).CopyTo(destination);
                return res;
            }
            else
            {
                var d = destination;
                int res = 0;
                while (!d.IsEmpty)
                {
                    var rres = source.Read(buffer, 0, Math.Min(d.Length, buffer.Length));
                    buffer.AsSpan().SliceWhile(rres).CopyTo(d);
                    d = d.Slice(rres);
                    res += rres;
                    if (rres == 0)
                    {
                        return res;
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The number of <typeparamref name="TSample"/>s read from this <see cref="IDataSource{TSample}"/>.</returns>
        public async ValueTask<ReadResult> ReadAsync(Memory<byte> destination) => Read(destination.Span);

#endif

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> with the specified offset in frames.
        /// </summary>
        /// <param name="offset">The offset in frames.</param>
        /// <param name="origin">The origin.</param>
        public void Seek(long offset, SeekOrigin origin) => source.Seek(offset, origin);

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> to the specified index in frames from the end of stream.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public void SeekLast(ulong offset) => source.Seek((long)offset, SeekOrigin.End);

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> to the specified index in frames.
        /// </summary>
        /// <param name="index">The index in frames.</param>
        public void SeekTo(ulong index) => source.Seek((long)index, SeekOrigin.Begin);

        /// <summary>
        /// Skips the source the specified step in frames.
        /// </summary>
        /// <param name="step">The number of frames to skip.</param>
        public void Skip(ulong step) => source.Seek((long)step, SeekOrigin.Current);

        /// <summary>
        /// Steps this data source the specified step back in frames.
        /// </summary>
        /// <param name="step">The number of frames to step back.</param>
        public void StepBack(ulong step) => source.Seek(-(long)step, SeekOrigin.Current);
    }
}
