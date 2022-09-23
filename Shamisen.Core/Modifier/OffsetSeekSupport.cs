using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Modifier
{
    /// <summary>
    /// Implements an <see cref="ISeekSupport"/> with offset.
    /// </summary>
    public sealed class OffsetSeekSupport : ISeekSupport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetSeekSupport"/> class.
        /// </summary>
        /// <param name="seekSupport">The seek support.</param>
        /// <param name="offset">The offset.</param>
        /// <exception cref="ArgumentNullException">seekSupport</exception>
        public OffsetSeekSupport(ISeekSupport seekSupport, ulong offset)
        {
            ArgumentNullException.ThrowIfNull(seekSupport);
            SeekSupport = seekSupport;
            Offset = offset;
        }

        private ISeekSupport SeekSupport { get; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public ulong Offset { get; }

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}" /> to the specified index in frames from the end of stream.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public void SeekLast(ulong offset) => SeekSupport.SeekLast(offset);

        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}" /> to the specified index in frames.
        /// </summary>
        /// <param name="index">The index in frames.</param>
        public void SeekTo(ulong index) => SeekSupport.SeekTo(index + Offset);

        /// <summary>
        /// Skips the source the specified step in frames.
        /// </summary>
        /// <param name="step">The number of frames to skip.</param>
        public void Skip(ulong step) => SeekSupport.Skip(step);

        /// <summary>
        /// Steps this data source the specified step back in frames.
        /// </summary>
        /// <param name="step">The number of frames to step back.</param>
        public void StepBack(ulong step) => SeekSupport.StepBack(step);
    }
}
