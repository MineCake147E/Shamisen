using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Buffers
{
    /// <summary>
    /// Provides a type-, memory-, and format-safe representation of a contiguous region of channel-interleaved memory that contains audio data.
    /// </summary>
    /// <typeparam name="TSample">The type of audio sample.</typeparam>
    /// <typeparam name="TFormat">The type of audio format.</typeparam>
    public readonly ref struct InterleavedAudioSpan<TSample, TFormat>
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
        /// The span.
        /// </value>
        public Span<TSample> Span { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => Span.IsEmpty;

        /// <summary>
        /// Gets the length of this <see cref="InterleavedAudioSpan{TSample, TFormat}"/> in samples.
        /// </summary>
        /// <value>
        /// The length of this span in samples.
        /// </value>
        public int SampleLength => Span.Length;

        /// <summary>
        /// Gets the length of this <see cref="InterleavedAudioSpan{TSample, TFormat}"/> in frames.
        /// </summary>
        /// <value>
        /// The length of this span in frames.
        /// </value>
        public int Length { get; }

        /// <summary>
        /// Gets the frame at the specified <paramref name="frame"/> in the <see cref="InterleavedAudioSpan{TSample, TFormat}"/>.
        /// </summary>
        /// <value>
        /// The span to the frame.
        /// </value>
        /// <param name="frame">The index of frame.</param>
        /// <returns></returns>
        public Span<TSample> this[int frame]
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Span.Slice(Format.BlockSize * frame, Format.BlockSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterleavedAudioSpan{TSample, TFormat}"/> struct.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="span">The span.</param>
        /// <exception cref="ArgumentException">The <paramref name="span"/> must be aligned with <paramref name="format"/>'s <see cref="IInterleavedAudioFormat{TSample}.BlockSize"/>!</exception>
        public InterleavedAudioSpan(TFormat format, Span<TSample> span)
        {
            Format = format;
            if (span.Length % format.BlockSize != 0) throw new ArgumentException($"The {nameof(span)} must be aligned with {nameof(format.BlockSize)}!", nameof(span));
            Span = span;
            Length = span.Length / format.BlockSize;
        }

        /// <summary>
        /// Forms a slice out of the current span starting at a specified index for a specified <paramref name="length"/>.
        /// </summary>
        /// <param name="start">The index in frames at which to begin this slice.</param>
        /// <param name="length">The desired length in frame for the slice.</param>
        /// <returns>A span that consists of <paramref name="length"/> elements from the current span starting at <paramref name="start"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public InterleavedAudioSpan<TSample, TFormat> Slice(int start, int length)
            => new(Format, Span.Slice(Format.BlockSize * start, Format.BlockSize * length));

        /// <summary>
        /// Forms a slice out of the current span that begins at a specified index.
        /// </summary>
        /// <param name="start">The index in frames at which to begin this slice.</param>
        /// <returns>
        /// A span that consists of all elements of the current span from <paramref name="start"/> to the end of the span.
        /// </returns>
        public InterleavedAudioSpan<TSample, TFormat> Slice(int start)
            => new(Format, Span.Slice(Format.BlockSize * start));

        /// <summary>
        /// Copies the contents of this <see cref="InterleavedAudioSpan{TSample, TFormat}"/> into a destination <see cref="InterleavedAudioSpan{TSample, TFormat}"/>.
        /// </summary>
        /// <param name="destination">The destination.</param>
        public void CopyTo(InterleavedAudioSpan<TSample, TFormat> destination)
        {
            if (!Format.Equals(destination.Format))
                throw new ArgumentException($"The Format of this span must be the same as destination's one!", nameof(destination));
            else
                Span.CopyTo(destination.Span);
        }

        /// <summary>
        /// Attempts to copy the current <see cref="InterleavedAudioSpan{TSample, TFormat}"/> to a destination <see cref="InterleavedAudioSpan{TSample, TFormat}"/> and returns a value that indicates whether the copy operation succeeded.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>true if the copy operation succeeded; otherwise, false.</returns>
        public bool TryCopyTo(InterleavedAudioSpan<TSample, TFormat> destination) => Format.Equals(destination.Format) && Span.TryCopyTo(destination.Span);

        /// <summary>
        /// Fills the elements of this span with a specified value.
        /// </summary>
        /// <param name="value">The value to assign to each element of the span.</param>
        public void Fill(TSample value) => Span.Fill(value);

        /// <summary>
        /// Clears the contents of this <see cref="InterleavedAudioSpan{TSample, TFormat}"/> object.
        /// </summary>
        public void Clear() => Fill(default);

        #region Static fields

        /// <summary>
        /// Returns an empty <see cref="InterleavedAudioSpan{TSample, TFormat}"/> object.
        /// </summary>
        /// <value>
        /// An empty <see cref="InterleavedAudioSpan{TSample, TFormat}"/> object.
        /// </value>
        public static InterleavedAudioSpan<TSample, TFormat> Empty => default;

        #endregion Static fields

        /// <summary>
        /// Performs an implicit conversion from <see cref="InterleavedAudioSpan{TSample, TFormat}"/> to <see cref="ReadOnlyInterleavedAudioSpan{TSample, TFormat}"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ReadOnlyInterleavedAudioSpan<TSample, TFormat>(InterleavedAudioSpan<TSample, TFormat> span) => new(span.Format, span.Span);

        /// <summary>
        /// Performs an implicit conversion from <see cref="AudioSpan{TSample, TFormat}"/> to <see cref="InterleavedAudioSpan{TSample, TFormat}"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator InterleavedAudioSpan<TSample, TFormat>(AudioSpan<TSample, TFormat> span) => new(span.Format, span.Span);

        #region Equality

        /// <summary>
        /// Indicates whether the values of two specified <see cref="InterleavedAudioSpan{TSample, TFormat}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="InterleavedAudioSpan{TSample, TFormat}"/> to compare.</param>
        /// <param name="right">The second  <see cref="InterleavedAudioSpan{TSample, TFormat}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(InterleavedAudioSpan<TSample, TFormat> left, InterleavedAudioSpan<TSample, TFormat> right) => !(left == right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="InterleavedAudioSpan{TSample, TFormat}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="InterleavedAudioSpan{TSample, TFormat}"/> to compare.</param>
        /// <param name="right">The second <see cref="InterleavedAudioSpan{TSample, TFormat}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(InterleavedAudioSpan<TSample, TFormat> left, InterleavedAudioSpan<TSample, TFormat> right) => left.Equals(right);

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
        /// <param name="span">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the span parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(InterleavedAudioSpan<TSample, TFormat> span) => Format.Equals(span.Format) && Span == span.Span;

        /// <summary>
        /// Throws a System.NotSupportedException.
        /// </summary>
        /// <returns>
        /// Calls to this method always throw a System.NotSupportedException.
        /// </returns>
        /// <exception cref="NotSupportedException">Always</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("GetHashCode() on Span will always throw an exception.")]
#pragma warning disable CS0809, S3877
        public override int GetHashCode() => throw new NotSupportedException();

#pragma warning restore CS0809, S3877

        #endregion Equality
    }
}
