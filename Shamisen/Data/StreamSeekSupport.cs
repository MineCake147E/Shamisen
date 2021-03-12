using System;
using System.IO;

namespace Shamisen.Data
{
    internal sealed class StreamSeekSupport : ISeekSupport, IDisposable
    {
        private Stream source;
        private bool disposedValue;

        public StreamSeekSupport(Stream source) => this.source = source ?? throw new ArgumentNullException(nameof(source));

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

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
