using System.IO;
using System.Numerics;

using MonoAudio.Data;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure that contains seek support of <see cref="IAudioSource{TSample, TFormat}"/> or <see cref="IDataSource{TSample}"/>.
    /// </summary>
    public interface ISeekSupport : ISkipSupport
    {
        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> with the specified offset in frames.
        /// </summary>
        /// <param name="offset">The offset in frames.</param>
        /// <param name="origin">The origin.</param>
        void Seek(long offset, SeekOrigin origin);

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> to the specified index in frames.
        /// </summary>
        /// <param name="index">The index in frames.</param>
        void SeekTo(ulong index);

        /// <summary>
        /// Steps this data source the specified step back in frames.
        /// </summary>
        /// <param name="step">The number of frames to step back.</param>
        void StepBack(ulong step);

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> to the specified index in frames from the end of stream.
        /// </summary>
        /// <param name="offset">The offset.</param>
        void SeekLast(ulong offset);
    }
}
