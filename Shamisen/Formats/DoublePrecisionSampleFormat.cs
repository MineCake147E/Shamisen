using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Represents a format of a double-precision sample source.
    /// </summary>
    public readonly struct DoublePrecisionSampleFormat : IInterleavedAudioFormat<double>, IEquatable<DoublePrecisionSampleFormat>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoublePrecisionSampleFormat"/> struct.
        /// </summary>
        /// <param name="channels">The channels.</param>
        /// <param name="sampleRate">The sample rate.</param>
        public DoublePrecisionSampleFormat(int channels, int sampleRate)
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
        public int BitDepth => 64;

        /// <summary>
        /// Gets the value indicates how many <see cref="double"/> values are required per whole frame.<br/>
        /// It depends on <see cref="IAudioFormat{TSample}.Channels"/>.
        /// </summary>
        /// <value>
        /// The size of block.
        /// </value>
        public int BlockSize { [MethodImpl(MethodImplOptions.AggressiveInlining)]get => Channels; }

        /// <summary>
        /// Gets the value indicates how many <see cref="double"/>s are required per 1-channel sample.<br/>
        /// Does not depend on the number of <see cref="Channels"/>.<br/>
        /// </summary>
        /// <value>
        /// The size of a frame in <see cref="double"/>s.
        /// </value>
        public int SampleSize { [MethodImpl(MethodImplOptions.AggressiveInlining)]get => 1; }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is DoublePrecisionSampleFormat format && Equals(format);

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(DoublePrecisionSampleFormat other) => Channels == other.Channels &&
                   SampleRate == other.SampleRate;

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(IAudioFormat<double>? other) => !(other is null) && other.BitDepth == BitDepth && other.Channels == Channels && other.SampleRate == SampleRate;

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => HashCode.Combine(Channels, SampleRate);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="DoublePrecisionSampleFormat"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="DoublePrecisionSampleFormat"/> to compare.</param>
        /// <param name="right">The second <see cref="DoublePrecisionSampleFormat"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the value of int1 is the same as the value of int2; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(DoublePrecisionSampleFormat left, DoublePrecisionSampleFormat right) => left.Equals(right);

        /// <summary>
		/// Indicates whether the values of two specified <see cref="DoublePrecisionSampleFormat"/> objects are different.
		/// </summary>
		/// <param name="left">The first <see cref="DoublePrecisionSampleFormat"/> to compare.</param>
		/// <param name="right">The second <see cref="DoublePrecisionSampleFormat"/> to compare.</param>
		/// <returns>
		///   <c>true</c> if the value of int1 is not the same as the value of int2; otherwise, <c>false</c>.
		/// </returns>
        public static bool operator !=(DoublePrecisionSampleFormat left, DoublePrecisionSampleFormat right) => !(left == right);
    }
}
