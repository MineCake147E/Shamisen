using System;

namespace Shamisen.Filters.Mixing
{
    /// <summary>
    /// Defines a base infrastructure of a pair of <see cref="ISampleSource"/> and buffer.
    /// </summary>
    public interface ISourceBufferPair : ISampleSource
    {
        /// <summary>
        /// Gets the buffer associated with this <see cref="ISourceBufferPair"/>.
        /// </summary>
        /// <value>
        /// The buffer associated with this <see cref="ISourceBufferPair"/>.
        /// </value>
        Memory<float> Buffer { get; }

        /// <summary>
        /// Checks and stretches the <see cref="Buffer"/>.
        /// </summary>
        /// <param name="length">The length.</param>
        void CheckBuffer(int length);
        /// <summary>
        /// Gets the source associated with this <see cref="ISourceBufferPair"/>.
        /// </summary>
        /// <value>
        /// The source associated with this <see cref="ISourceBufferPair"/>.
        /// </value>
        ISampleSource Source { get; }
    }
}