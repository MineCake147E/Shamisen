using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Formats
{
    /// <summary>
    /// Represents a format of an sample source.
    /// </summary>
    public readonly struct SampleFormat : IAudioFormat<float>, IEquatable<SampleFormat>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleFormat"/> struct.
        /// </summary>
        /// <param name="channels">The channels.</param>
        /// <param name="sampleRate">The sample rate.</param>
        public SampleFormat(int channels, int sampleRate)
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
        public int BitDepth => 32;

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is SampleFormat format && Equals(format);
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(SampleFormat other)
        {
            return Channels == other.Channels &&
                   SampleRate == other.SampleRate;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(IAudioFormat<float> other)
        {
            return other.BitDepth == 32 && other.Channels == Channels && other.SampleRate == SampleRate;
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            var hashCode = -709472342;
            hashCode = hashCode * -1521134295 + Channels.GetHashCode();
            hashCode = hashCode * -1521134295 + SampleRate.GetHashCode();
            return hashCode;
        }

        /// <summary>
		/// Indicates whether the values of two specified <see cref="SampleFormat"/> objects are equal.
		/// </summary>
		/// <param name="left">The first <see cref="SampleFormat"/> to compare.</param>
		/// <param name="right">The second <see cref="SampleFormat"/> to compare.</param>
		/// <returns>
		///   <c>true</c> if the value of int1 is the same as the value of int2; otherwise, <c>false</c>.
		/// </returns>
        public static bool operator ==(SampleFormat left, SampleFormat right)
        {
            return left.Equals(right);
        }

        /// <summary>
		/// Indicates whether the values of two specified <see cref="SampleFormat"/> objects are different.
		/// </summary>
		/// <param name="left">The first <see cref="SampleFormat"/> to compare.</param>
		/// <param name="right">The second <see cref="SampleFormat"/> to compare.</param>
		/// <returns>
		///   <c>true</c> if the value of int1 is not the same as the value of int2; otherwise, <c>false</c>.
		/// </returns>
        public static bool operator !=(SampleFormat left, SampleFormat right)
        {
            return !(left == right);
        }
    }
}
