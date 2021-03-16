using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Represents a seek point of FLAC file.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 18)]
    public readonly struct FlacSeekPoint : IEquatable<FlacSeekPoint>
    {
        [FieldOffset(0)]
        private readonly ulong firstIndex;

        [FieldOffset(8)]
        private readonly ulong targetOffset;

        [FieldOffset(16)]
        private readonly ushort samplesInFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacSeekPoint"/> struct.
        /// </summary>
        /// <param name="firstIndex">The sample index of the first sample in the target FLAC frame in terms of the number of inter-channel samples.</param>
        /// <param name="targetOffset">The offset in bytes from the first byte of the header of the first FLAC frame, to the first byte of the header of the target FLAC frame.</param>
        /// <param name="samplesInFrame">The length of the target FLAC frame.</param>
        public FlacSeekPoint(ulong firstIndex, ulong targetOffset, ushort samplesInFrame)
        {
            this.firstIndex = firstIndex;
            this.targetOffset = targetOffset;
            this.samplesInFrame = samplesInFrame;
        }

        /// <summary>
        /// Get the sample index of the first sample in the target FLAC frame in terms of the number of inter-channel samples.
        /// </summary>
        /// <value>
        /// The first index.
        /// </value>
        public ulong FirstIndex => firstIndex;

        /// <summary>
        /// Gets the offset in bytes from the first byte of the header of the first FLAC frame, to the first byte of the header of the target FLAC frame.
        /// </summary>
        /// <value>
        /// The target frame offset.
        /// </value>
        public ulong TargetFrameOffset => targetOffset;

        /// <summary>
        /// Gets the length of the target FLAC frame.
        /// </summary>
        /// <value>
        /// The length of the target FLAC frame.
        /// </value>
        public ushort TargetFrameLength => samplesInFrame;

        /// <summary>
        /// Converts the specified <paramref name="value"/> read directly from <see cref="Span{T}"/> of <see cref="byte"/>, to readable value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacSeekPoint ToReadableValue(FlacSeekPoint value) => !BitConverter.IsLittleEndian ? value :
            ReverseEndianness(value);

        /// <summary>
        /// Converts the specified <paramref name="value"/> constructed manually in user code, to writable value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacSeekPoint ToWritableValue(FlacSeekPoint value) => ToReadableValue(value);

        /// <summary>
        /// Reverses the endianness of every field in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to convert endianness.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacSeekPoint ReverseEndianness(FlacSeekPoint value)
            => new(
                BinaryPrimitives.ReverseEndianness(value.firstIndex),
                BinaryPrimitives.ReverseEndianness(value.targetOffset),
                BinaryPrimitives.ReverseEndianness(value.samplesInFrame));

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is FlacSeekPoint point && Equals(point);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(FlacSeekPoint other) => firstIndex == other.firstIndex && targetOffset == other.targetOffset && samplesInFrame == other.samplesInFrame;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(firstIndex, targetOffset, samplesInFrame);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacSeekPoint"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacSeekPoint"/> to compare.</param>
        /// <param name="right">The second <see cref="FlacSeekPoint"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(FlacSeekPoint left, FlacSeekPoint right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacSeekPoint"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacSeekPoint"/> to compare.</param>
        /// <param name="right">The second  <see cref="FlacSeekPoint"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(FlacSeekPoint left, FlacSeekPoint right) => !(left == right);
    }
}
