using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Composing;
using Shamisen.Codecs.Waveform.Composing;

namespace Shamisen.Codecs.Waveform
{
    /// <summary>
    /// Represents an "extended" wave format.
    /// </summary>
    public readonly struct StandardWaveFormatWithExtraData : IWaveFormat, IEquatable<StandardWaveFormatWithExtraData>, IRf64Content
    {
        private readonly StandardWaveFormat format;
        private readonly ushort extensionSize;
        private readonly ReadOnlyMemory<byte> extraData;

        /// <summary>
        /// Initializes a new instance of <see cref="StandardWaveFormatWithExtraData"/> struct.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="extraData"></param>
        public StandardWaveFormatWithExtraData(StandardWaveFormat format, ReadOnlyMemory<byte> extraData) : this()
        {
            this.format = format;
            this.extraData = extraData;
            if (ExtraData.Length > ushort.MaxValue) throw new ArgumentException($"{nameof(extraData)} must be shorter than 65536!", nameof(extraData));
            extensionSize = (ushort)extraData.Length;
        }

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public AudioEncoding Encoding => format.Encoding;

        /// <summary>
        /// Gets the extra data.
        /// </summary>
        /// <value>
        /// The extra data.
        /// </value>
        public ReadOnlyMemory<byte> ExtraData => extraData;

        /// <summary>
        /// Gets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public int BlockSize => format.BlockSize;

        /// <summary>
        /// Gets the size of the sample.
        /// </summary>
        /// <value>
        /// The size of the sample.
        /// </value>
        public int SampleSize => format.SampleSize;

        /// <summary>
        /// Gets the channels.
        /// </summary>
        /// <value>
        /// The channels.
        /// </value>
        public int Channels => format.Channels;

        /// <summary>
        /// Gets the sample rate.
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        public int SampleRate => format.SampleRate;

        /// <summary>
        /// Gets the bit depth.
        /// </summary>
        /// <value>
        /// The bit depth.
        /// </value>
        public int BitDepth => format.BitDepth;

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public ulong Size => 18ul + (ulong)ExtraData.Length;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is StandardWaveFormatWithExtraData format && Equals(format);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(StandardWaveFormatWithExtraData other) => format.Equals(other.format) && extensionSize == other.extensionSize && extraData.Equals(other.extraData);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(IAudioFormat<byte>? other) => other is StandardWaveFormatWithExtraData ef && ef.Equals(this);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(format, extensionSize, extraData);

        /// <summary>
        /// Writes this <see cref="IComposable" /> instance to <see cref="IDataSink{TSample}" />.
        /// </summary>
        /// <param name="sink">The sink.</param>
        public void WriteTo(IDataSink<byte> sink)
        {
            if (BitConverter.IsLittleEndian)
            {
                Span<byte> span = stackalloc byte[40];
                Unsafe.As<byte, (StandardWaveFormat, ushort)>(ref span[0]) = (format, extensionSize);
                sink.Write(span);
                sink.Write(extraData.Span);
            }
            else
            {
                format.WriteTo(sink);
                Span<byte> span = stackalloc byte[40 - 16];
                BinaryPrimitives.WriteUInt16LittleEndian(span, extensionSize);
                sink.Write(span);
                sink.Write(extraData.Span);
            }
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="StandardWaveFormatWithExtraData"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="StandardWaveFormatWithExtraData"/> to compare.</param>
        /// <param name="right">The second <see cref="StandardWaveFormatWithExtraData"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(StandardWaveFormatWithExtraData left, StandardWaveFormatWithExtraData right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="StandardWaveFormatWithExtraData"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="StandardWaveFormatWithExtraData"/> to compare.</param>
        /// <param name="right">The second  <see cref="StandardWaveFormatWithExtraData"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(StandardWaveFormatWithExtraData left, StandardWaveFormatWithExtraData right) => !(left == right);
    }
}
