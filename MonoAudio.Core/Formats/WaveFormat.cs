using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio.Formats
{
    /// <summary>
    /// Represents a wave format.
    /// </summary>
    /// <seealso cref="Formats.IAudioFormat{TSample}" />
    public readonly struct WaveFormat : IWaveFormat, IEquatable<WaveFormat>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveFormat"/> struct.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="bitDepth">The bit depth.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="encoding">The encoding.</param>
        public WaveFormat(int sampleRate, int bitDepth, int channels, AudioEncoding encoding) : this()
        {
            SampleRate = sampleRate;
            BitDepth = bitDepth;
            Channels = channels;
            Encoding = encoding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveFormat"/> struct.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="bitDepth">The bit depth.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="extraSize">Size of the extra.</param>
        public WaveFormat(int sampleRate, int bitDepth, int channels, AudioEncoding encoding, int extraSize) : this(sampleRate, bitDepth, channels, encoding) => ExtraSize = extraSize;

        /// <summary>
        /// Gets the value indicates how many bytes are required per whole sample.
        /// It depends on <see cref="IAudioFormat{TSample}.Channels"/>.
        /// </summary>
        /// <value>
        /// The size of block.
        /// </value>
        public int BlockSize => Channels * (BitDepth / 8);

        /// <summary>
        /// Gets the value indicates how the samples are encoded.
        /// </summary>
        /// <value>
        /// The sample encoding.
        /// </value>
        public AudioEncoding Encoding { get; }

        /// <summary>
        /// Gets the size of the extra information.
        /// </summary>
        /// <value>
        /// The size of the extra information.
        /// </value>
        public int ExtraSize { get; }

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
        public int BitDepth { get; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is WaveFormat format && Equals(format);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(WaveFormat other)
        {
            return BlockSize == other.BlockSize &&
                   Encoding == other.Encoding &&
                   ExtraSize == other.ExtraSize &&
                   Channels == other.Channels &&
                   SampleRate == other.SampleRate &&
                   BitDepth == other.BitDepth;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(IAudioFormat<byte> other) => other is WaveFormat format && Equals(format);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = -16010822;
            hashCode = hashCode * -1521134295 + BlockSize.GetHashCode();
            hashCode = hashCode * -1521134295 + Encoding.GetHashCode();
            hashCode = hashCode * -1521134295 + ExtraSize.GetHashCode();
            hashCode = hashCode * -1521134295 + Channels.GetHashCode();
            hashCode = hashCode * -1521134295 + SampleRate.GetHashCode();
            hashCode = hashCode * -1521134295 + BitDepth.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="WaveFormat"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="WaveFormat"/> to compare.</param>
        /// <param name="right">The second <see cref="WaveFormat"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(WaveFormat left, WaveFormat right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="WaveFormat"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="WaveFormat"/> to compare.</param>
        /// <param name="right">The second  <see cref="WaveFormat"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(WaveFormat left, WaveFormat right)
        {
            return !(left == right);
        }
    }
}
