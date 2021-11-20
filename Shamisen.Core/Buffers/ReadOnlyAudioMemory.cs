using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Represents a read-only audio buffer.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public readonly struct ReadOnlyAudioMemory<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> struct.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="memory">The memory.</param>
        public ReadOnlyAudioMemory(TFormat format, ReadOnlyMemory<TSample> memory)
        {
            Format = format;
            Memory = memory;
        }

        /// <summary>
        /// Gets the element at the specified <paramref name="index"/> in the <see cref="AudioMemory{TSample, TFormat}"/>.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public TSample this[int index]
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Memory.Span[index];
        }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format { get; }

        /// <summary>
        /// Gets the length of this <see cref="AudioMemory{TSample, TFormat}"/>.
        /// </summary>
        /// <value>
        /// The length of this buffer.
        /// </value>
        public int Length => Memory.Span.Length;

        /// <summary>
        /// Gets the internal memory region.
        /// </summary>
        /// <value>
        /// The memory.
        /// </value>
        public ReadOnlyMemory<TSample> Memory { get; }

        /// <summary>
        /// Gets the span.
        /// </summary>
        /// <value>
        /// The span.
        /// </value>
        public ReadOnlyAudioSpan<TSample, TFormat> Span => new(Format, Memory.Span);

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => Memory.IsEmpty;

        /// <summary>
        /// Forms a slice out of the current memory starting at a specified index for a specified <paramref name="length"/>.
        /// </summary>
        /// <param name="start">The index in samples at which to begin this slice.</param>
        /// <param name="length">The desired length in samples for the slice.</param>
        /// <returns>A memory that consists of <paramref name="length"/> elements from the current memory starting at <paramref name="start"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadOnlyAudioMemory<TSample, TFormat> Slice(int start, int length) => new(Format, Memory.Slice(start, length));

        /// <summary>
        /// Forms a slice out of the current memory that begins at a specified index.
        /// </summary>
        /// <param name="start">The index in samples at which to begin this slice.</param>
        /// <returns>
        /// A memory that consists of all elements of the current memory from <paramref name="start"/> to the end of the memory.
        /// </returns>
        public ReadOnlyAudioMemory<TSample, TFormat> Slice(int start) => new(Format, Memory.Slice(start));

        /// <summary>
        /// Copies the contents of this <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> into a destination <see cref="AudioMemory{TSample, TFormat}"/>.
        /// </summary>
        /// <param name="destination">The destination.</param>
        public void CopyTo(AudioMemory<TSample, TFormat> destination)
        {
            if (!Format.Equals(destination.Format))
                throw new ArgumentException($"The Format of this memory must be the same as destination's one!", nameof(destination));
            else
                Memory.CopyTo(destination.Memory);
        }

        /// <summary>
        /// Attempts to copy the current <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> to a destination <see cref="AudioMemory{TSample, TFormat}"/> and returns a value that indicates whether the copy operation succeeded.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>true if the copy operation succeeded; otherwise, false.</returns>
        public bool TryCopyTo(AudioMemory<TSample, TFormat> destination) => Format.Equals(destination.Format) && Memory.TryCopyTo(destination.Memory);

        #region Static fields

        /// <summary>
        /// Returns an empty <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> object.
        /// </summary>
        /// <value>
        /// An empty <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> object.
        /// </value>
        public static ReadOnlyAudioMemory<TSample, TFormat> Empty => default;

        #endregion Static fields

        #region Equality

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> to compare.</param>
        /// <param name="right">The second  <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(ReadOnlyAudioMemory<TSample, TFormat> left, ReadOnlyAudioMemory<TSample, TFormat> right) => !(left == right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> to compare.</param>
        /// <param name="right">The second <see cref="ReadOnlyAudioMemory{TSample, TFormat}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(ReadOnlyAudioMemory<TSample, TFormat> left, ReadOnlyAudioMemory<TSample, TFormat> right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => false;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="memory">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the memory parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ReadOnlyAudioMemory<TSample, TFormat> memory) => Format.Equals(memory.Format) && Memory.Span == memory.Memory.Span;

        /// <summary>
        /// Throws a System.NotSupportedException.
        /// </summary>
        /// <returns>
        /// Calls to this method always throw a System.NotSupportedException.
        /// </returns>
        /// <exception cref="NotSupportedException">Always</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("GetHashCode() on ReadOnlyAudioMemory will always throw an exception.")]
#pragma warning disable CS0809
        public override int GetHashCode() =>
            throw new NotSupportedException();

#pragma warning restore CS0809

        #endregion Equality
    }
}
