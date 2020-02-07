using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Represents a standard wave format inside "*.wav" files.
    /// </summary>
    /// <seealso cref="IWaveFormat" />
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct StandardWaveFormat : IWaveFormat, IEquatable<StandardWaveFormat>
    {
        [FieldOffset(0)]
        private readonly AudioEncoding encoding;

        [FieldOffset(2)]
        private readonly ushort channels;

        [FieldOffset(4)]
        private readonly uint sampleRate;

        [FieldOffset(8)]
        private readonly uint bytesPerSecond;

        [FieldOffset(12)]
        private readonly ushort blockSize;

        [FieldOffset(14)]
        private readonly ushort bitDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardWaveFormat"/> struct.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="bytesPerSecond">The bytes per second.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="bitDepth">The bit depth.</param>
        public StandardWaveFormat(AudioEncoding encoding, ushort channels, uint sampleRate, uint bytesPerSecond, ushort blockSize, ushort bitDepth)
        {
            this.encoding = encoding;
            this.channels = channels;
            this.sampleRate = sampleRate;
            this.bytesPerSecond = bytesPerSecond;
            this.blockSize = blockSize;
            this.bitDepth = bitDepth;
        }

        internal uint BytesPerSecond => bytesPerSecond;

        /// <summary>
        /// Gets the value indicates how many bytes are required per whole sample.
        /// It depends on <see cref="IAudioFormat{TSample}.Channels"/>.
        /// </summary>
        /// <value>
        /// The size of block.
        /// </value>
        public int BlockSize => blockSize;

        /// <summary>
        /// Gets the value indicates how the samples are encoded.
        /// </summary>
        /// <value>
        /// The sample encoding.
        /// </value>
        public AudioEncoding Encoding => encoding;

        /// <summary>
        /// Gets the number of channels.
        /// It does not describe how these loudspeakers of each channels are placed in the room.
        /// </summary>
        /// <value>
        /// The number of channels.
        /// </value>
        public int Channels => channels;

        /// <summary>
        /// Gets the number indicates how many times the audio signal is sampled.
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        public int SampleRate => unchecked((int)sampleRate);

        /// <summary>
        /// Gets the number indicates how many bits are consumed per every single 1-channel sample.
        /// Does not depend on the number of <see cref="Channels"/>.
        /// </summary>
        /// <value>
        /// The bit depth.
        /// </value>
        public int BitDepth => bitDepth;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj) => obj is StandardWaveFormat format && Equals(format);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(StandardWaveFormat other) => encoding == other.encoding && channels == other.channels && sampleRate == other.sampleRate && bytesPerSecond == other.bytesPerSecond && blockSize == other.blockSize && bitDepth == other.bitDepth;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(IAudioFormat<byte> other) => other is StandardWaveFormat format && Equals(format);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = -1775714179;
            hashCode = hashCode * -1521134295 + encoding.GetHashCode();
            hashCode = hashCode * -1521134295 + channels.GetHashCode();
            hashCode = hashCode * -1521134295 + sampleRate.GetHashCode();
            hashCode = hashCode * -1521134295 + bytesPerSecond.GetHashCode();
            hashCode = hashCode * -1521134295 + blockSize.GetHashCode();
            hashCode = hashCode * -1521134295 + bitDepth.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="StandardWaveFormat"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="StandardWaveFormat"/> to compare.</param>
        /// <param name="right">The second <see cref="StandardWaveFormat"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(StandardWaveFormat left, StandardWaveFormat right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="StandardWaveFormat"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="StandardWaveFormat"/> to compare.</param>
        /// <param name="right">The second  <see cref="StandardWaveFormat"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(StandardWaveFormat left, StandardWaveFormat right) => !(left == right);
    }
}
