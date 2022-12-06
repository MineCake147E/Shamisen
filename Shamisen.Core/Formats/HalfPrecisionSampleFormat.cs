#if NET5_0_OR_GREATER

using System.Runtime.CompilerServices;

namespace Shamisen.Formats
{
    /// <summary>
    /// Represents a format of a half-precision sample source.
    /// </summary>
    public readonly struct HalfPrecisionSampleFormat : IInterleavedAudioFormat<Half>, IEquatable<HalfPrecisionSampleFormat>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HalfPrecisionSampleFormat"/> struct.
        /// </summary>
        /// <param name="channels">The channels.</param>
        /// <param name="sampleRate">The sample rate.</param>
        public HalfPrecisionSampleFormat(int channels, int sampleRate)
        {
            Channels = channels;
            SampleRate = sampleRate;
        }

        /// <summary>
        /// Gets the number of channels.
        /// It does not describe how these loudspeakers of each channels are placed in the room.
        /// </summary>
        /// <value>
        /// The number of channels.
        /// </value>
        public int Channels { get; }

        /// <summary>
        /// Gets the number indicates how many times the audio signal is sampled.
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        public int SampleRate { get; }

        /// <summary>
        /// Gets the number indicates how many bits are consumed per every single 1-channel sample.
        /// Does not depend on the number of <see cref="Channels"/>.
        /// </summary>
        /// <value>
        /// The bit depth.
        /// </value>
        public int BitDepth => 16;

        /// <summary>
        /// Gets the value indicates how many <see cref="Half"/> values are required per whole frame.<br/>
        /// It depends on <see cref="IAudioFormat{TSample}.Channels"/>.
        /// </summary>
        /// <value>
        /// The size of block.
        /// </value>
        public int BlockSize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Channels; }

        /// <summary>
        /// Gets the value indicates how many <see cref="Half"/>s are required per 1-channel sample.<br/>
        /// Does not depend on the number of <see cref="Channels"/>.<br/>
        /// </summary>
        /// <value>
        /// The size of a frame in <see cref="Half"/>s.
        /// </value>
        public int SampleSize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => 1; }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is HalfPrecisionSampleFormat format && Equals(format);

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(HalfPrecisionSampleFormat other) => Channels == other.Channels &&
                   SampleRate == other.SampleRate;

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(IAudioFormat<Half>? other)
            => other is not null && other.BitDepth == BitDepth && other.Channels == Channels && other.SampleRate == SampleRate;

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => HashCode.Combine(Channels, SampleRate);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="HalfPrecisionSampleFormat"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="HalfPrecisionSampleFormat"/> to compare.</param>
        /// <param name="right">The second <see cref="HalfPrecisionSampleFormat"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the value of int1 is the same as the value of int2; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(HalfPrecisionSampleFormat left, HalfPrecisionSampleFormat right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="HalfPrecisionSampleFormat"/> objects are different.
        /// </summary>
        /// <param name="left">The first <see cref="HalfPrecisionSampleFormat"/> to compare.</param>
        /// <param name="right">The second <see cref="HalfPrecisionSampleFormat"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the value of int1 is not the same as the value of int2; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(HalfPrecisionSampleFormat left, HalfPrecisionSampleFormat right) => !(left == right);
    }
}

#endif
