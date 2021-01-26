using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio
{
    /// <summary>
    /// Represents a contiguous region of channel-interleaved memory that contains audio data.
    /// </summary>
    public readonly struct InterleavedAudioMemory<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IInterleavedAudioFormat<TSample>
    {
        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format { get; }

        /// <summary>
        /// Gets the internal memory region.
        /// </summary>
        /// <value>
        /// The memory.
        /// </value>
        public Memory<TSample> Memory { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => Memory.IsEmpty;

        /// <summary>
        /// Gets the length of this <see cref="InterleavedAudioMemory{TSample, TFormat}"/> in samples.
        /// </summary>
        /// <value>
        /// The length of this memory in samples.
        /// </value>
        public int SampleLength => Memory.Length;

        /// <summary>
        /// Gets the length of this <see cref="InterleavedAudioMemory{TSample, TFormat}"/> in frames.
        /// </summary>
        /// <value>
        /// The length of this memory in frames.
        /// </value>
        public int Length { get; }

        /// <summary>
        /// Gets the frame at the specified <paramref name="frame"/> in the <see cref="InterleavedAudioMemory{TSample, TFormat}"/>.
        /// </summary>
        /// <value>
        /// The memory to the frame.
        /// </value>
        /// <param name="frame">The index of frame.</param>
        /// <returns></returns>
        public Memory<TSample> this[int frame]
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Memory.Slice(Format.BlockSize * frame, Format.BlockSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterleavedAudioMemory{TSample, TFormat}"/> struct.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="memory">The memory.</param>
        /// <exception cref="ArgumentException">The <paramref name="memory"/> must be aligned with <paramref name="format"/>'s <see cref="IInterleavedAudioFormat{TSample}.BlockSize"/>!</exception>
        public InterleavedAudioMemory(TFormat format, Memory<TSample> memory)
        {
            Format = format;
            if (memory.Length % format.BlockSize != 0) throw new ArgumentException($"The {nameof(memory)} must be aligned with {nameof(format.BlockSize)}!", nameof(memory));
            Memory = memory;
            Length = memory.Length / format.Channels;
        }

        /// <summary>
        /// Forms a slice out of the current memory starting at a specified index for a specified <paramref name="length"/>.
        /// </summary>
        /// <param name="start">The index in frames at which to begin this slice.</param>
        /// <param name="length">The desired length in frame for the slice.</param>
        /// <returns>A memory that consists of <paramref name="length"/> elements from the current memory starting at <paramref name="start"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public InterleavedAudioMemory<TSample, TFormat> Slice(int start, int length)
            => new InterleavedAudioMemory<TSample, TFormat>(Format, Memory.Slice(Format.BlockSize * start, Format.BlockSize * length));

        /// <summary>
        /// Forms a slice out of the current memory that begins at a specified index.
        /// </summary>
        /// <param name="start">The index in frames at which to begin this slice.</param>
        /// <returns>
        /// A memory that consists of all elements of the current memory from <paramref name="start"/> to the end of the memory.
        /// </returns>
        public InterleavedAudioMemory<TSample, TFormat> Slice(int start)
            => new InterleavedAudioMemory<TSample, TFormat>(Format, Memory.Slice(Format.BlockSize * start));

        /// <summary>
        /// Copies the contents of this <see cref="InterleavedAudioMemory{TSample, TFormat}"/> into a destination <see cref="InterleavedAudioMemory{TSample, TFormat}"/>.
        /// </summary>
        /// <param name="destination">The destination.</param>
        public void CopyTo(InterleavedAudioMemory<TSample, TFormat> destination)
        {
            if (!Format.Equals(destination.Format))
                throw new ArgumentException($"The Format of this memory must be the same as destination's one!", nameof(destination));
            else
                Memory.CopyTo(destination.Memory);
        }

        /// <summary>
        /// Attempts to copy the current <see cref="InterleavedAudioMemory{TSample, TFormat}"/> to a destination <see cref="InterleavedAudioMemory{TSample, TFormat}"/> and returns a value that indicates whether the copy operation succeeded.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>true if the copy operation succeeded; otherwise, false.</returns>
        public bool TryCopyTo(InterleavedAudioMemory<TSample, TFormat> destination) => Format.Equals(destination.Format) && Memory.TryCopyTo(destination.Memory);

        #region Static fields

        /// <summary>
        /// Returns an empty <see cref="InterleavedAudioMemory{TSample, TFormat}"/> object.
        /// </summary>
        /// <value>
        /// An empty <see cref="InterleavedAudioMemory{TSample, TFormat}"/> object.
        /// </value>
        public static InterleavedAudioMemory<TSample, TFormat> Empty { get => default; }

        #endregion Static fields

        /// <summary>
        /// Performs an implicit conversion from <see cref="InterleavedAudioMemory{TSample, TFormat}"/> to <see cref="ReadOnlyInterleavedAudioMemory{TSample, TFormat}"/>.
        /// </summary>
        /// <param name="memory">The memory.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ReadOnlyInterleavedAudioMemory<TSample, TFormat>(InterleavedAudioMemory<TSample, TFormat> memory) => new ReadOnlyInterleavedAudioMemory<TSample, TFormat>(memory.Format, memory.Memory);

        /// <summary>
        /// Performs an implicit conversion from <see cref="AudioMemory{TSample, TFormat}"/> to <see cref="InterleavedAudioMemory{TSample, TFormat}"/>.
        /// </summary>
        /// <param name="memory">The memory.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator InterleavedAudioMemory<TSample, TFormat>(AudioMemory<TSample, TFormat> memory) => new InterleavedAudioMemory<TSample, TFormat>(memory.Format, memory.Memory);

        #region Equality

        /// <summary>
        /// Indicates whether the values of two specified <see cref="InterleavedAudioMemory{TSample, TFormat}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="InterleavedAudioMemory{TSample, TFormat}"/> to compare.</param>
        /// <param name="right">The second  <see cref="InterleavedAudioMemory{TSample, TFormat}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(InterleavedAudioMemory<TSample, TFormat> left, InterleavedAudioMemory<TSample, TFormat> right) => !(left == right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="InterleavedAudioMemory{TSample, TFormat}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="InterleavedAudioMemory{TSample, TFormat}"/> to compare.</param>
        /// <param name="right">The second <see cref="InterleavedAudioMemory{TSample, TFormat}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(InterleavedAudioMemory<TSample, TFormat> left, InterleavedAudioMemory<TSample, TFormat> right) => left.Equals(right);

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
        public bool Equals(InterleavedAudioMemory<TSample, TFormat> memory) => Format.Equals(memory.Format) && Memory.Span == memory.Memory.Span;

        /// <summary>
        /// Throws a System.NotSupportedException.
        /// </summary>
        /// <returns>
        /// Calls to this method always throw a System.NotSupportedException.
        /// </returns>
        /// <exception cref="NotSupportedException">Always</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("GetHashCode() on InterleavedAudioMemory will always throw an exception.")]
#pragma warning disable CS0809
        public override int GetHashCode() => throw new NotSupportedException();

#pragma warning restore CS0809

        #endregion Equality
    }
}
