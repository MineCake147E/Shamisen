using System.IO;
using System.Numerics;

using Shamisen.Data;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure that contains seek support of <see cref="IAudioSource{TSample, TFormat}"/> or <see cref="IDataSource{TSample}"/>.
    /// </summary>
    public interface ISeekSupport : ISkipSupport
    {
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

    /// <summary>
    /// Contains some utility functions for <see cref="ISeekSupport"/>.
    /// </summary>
    public static class SeekSupportUtils
    {
        /// <inheritdoc cref="IClassicSeekSupport.Seek(long, SeekOrigin)"/>
        public static void Seek(this ISeekSupport seekSupport, long offset, SeekOrigin origin)
        {
            if (seekSupport is IClassicSeekSupport classicSeekSupport)
            {
                classicSeekSupport.Seek(offset, origin);
            }
            else
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        seekSupport.SeekTo((ulong)offset);
                        break;
                    case SeekOrigin.Current when offset > 0:
                        seekSupport.Skip((ulong)offset);
                        break;
                    case SeekOrigin.Current when offset < 0:
                        seekSupport.StepBack((ulong)-offset);
                        break;
                    case SeekOrigin.End:
                        seekSupport.SeekLast((ulong)offset);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
