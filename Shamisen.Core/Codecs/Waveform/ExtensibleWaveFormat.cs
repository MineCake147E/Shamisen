﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Formats;

namespace Shamisen.Codecs.Waveform
{
    /// <summary>
    /// Represents an "extensible" wave format.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct ExtensibleWaveFormat : IWaveFormat, IEquatable<ExtensibleWaveFormat>, IExtensibleWaveFormat, IRf64Content
    {
        [FieldOffset(0)]
        private readonly StandardWaveFormat format;

        [FieldOffset(16)]
        private readonly ushort extensionSize;

        [FieldOffset(18)]
        private readonly ushort validBitsPerSample;

        [FieldOffset(20)]
        private readonly Speakers channelMask;

        [FieldOffset(24)]
        private readonly Guid subFormat;

        [FieldOffset(40)]
        private readonly ReadOnlyMemory<byte> extraData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleWaveFormat"/> struct.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="extensionSize">Size of the extension.</param>
        /// <param name="validBitsPerSample">The valid bits per sample.</param>
        /// <param name="channelMask">The channel mask.</param>
        /// <param name="subFormat">The sub format.</param>
        /// <param name="extraData">The extra data.</param>
        public ExtensibleWaveFormat(StandardWaveFormat format, ushort extensionSize, ushort validBitsPerSample, Speakers channelMask, Guid subFormat, ReadOnlyMemory<byte> extraData)
        {
            this.format = format;
            this.extensionSize = extensionSize;
            this.validBitsPerSample = validBitsPerSample;
            this.channelMask = channelMask;
            this.subFormat = subFormat;
            this.extraData = extraData;
        }

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public StandardWaveFormat Format => format;

        /// <summary>
        /// Gets the size of the extension.
        /// </summary>
        /// <value>
        /// The size of the extension.
        /// </value>
        public ushort ExtensionSize => extensionSize;

        /// <summary>
        /// Gets the valid bits per sample.
        /// </summary>
        /// <value>
        /// The valid bits per sample.
        /// </value>
        public ushort ValidBitsPerSample => validBitsPerSample;

        /// <summary>
        /// Gets the channel combination.
        /// </summary>
        /// <value>
        /// The channel combination.
        /// </value>
        public Speakers ChannelCombination => channelMask;

        /// <summary>
        /// Gets the sub format.
        /// </summary>
        /// <value>
        /// The sub format.
        /// </value>
        public Guid SubFormat => subFormat;

        /// <summary>
        /// Gets the value indicates how many bytes are required per whole sample.
        /// It depends on <see cref="IAudioFormat{TSample}.Channels"/>.
        /// </summary>
        /// <value>
        /// The size of block.
        /// </value>
        public int BlockSize => Format.BlockSize;

        /// <summary>
        /// Gets the value indicates how the samples are encoded.
        /// </summary>
        /// <value>
        /// The sample encoding.
        /// </value>
        public AudioEncoding Encoding => Format.Encoding;

        /// <summary>
        /// Gets the number of channels.
        /// It does not describe how these loudspeakers of each channels are placed in the room.
        /// </summary>
        /// <value>
        /// The number of channels.
        /// </value>
        public int Channels => Format.Channels;

        /// <summary>
        /// Gets the number indicates how many times the audio signal is sampled.
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        public int SampleRate => Format.SampleRate;

        /// <summary>
        /// Gets the number indicates how many bits are consumed per every single 1-channel sample.
        /// Does not depend on the number of <see cref="Channels"/>.
        /// </summary>
        /// <value>
        /// The bit depth.
        /// </value>
        public int BitDepth => Format.BitDepth;

        /// <summary>
        /// Gets the size of the frame.
        /// </summary>
        /// <value>
        /// The size of the frame.
        /// </value>
        public int SampleSize => Format.SampleSize;

        /// <summary>
        /// Gets the extra data.
        /// </summary>
        /// <value>
        /// The extra data.
        /// </value>
        public ReadOnlyMemory<byte> ExtraData => extraData;

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public ulong Size => 40ul + (ulong)extraData.Length;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is ExtensibleWaveFormat format && Equals(format);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ExtensibleWaveFormat other) => Format.Equals(other.Format) && ExtensionSize == other.ExtensionSize && ValidBitsPerSample == other.ValidBitsPerSample && ChannelCombination == other.ChannelCombination && SubFormat.Equals(other.SubFormat);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(IAudioFormat<byte>? other) => other is ExtensibleWaveFormat format && Equals(format);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(Format, ExtensionSize, ValidBitsPerSample, ChannelCombination, SubFormat);

        /// <summary>
        /// Writes this <see cref="IComparable" /> instance to <see cref="IDataSink{TSample}" />.
        /// </summary>
        /// <param name="sink">The sink.</param>
        public void WriteTo(IDataSink<byte> sink)
        {
            format.WriteTo(sink);
            Span<byte> span = stackalloc byte[40 - 16];
            var w = BitConverter.IsLittleEndian
                ? new ExtensionPart(extensionSize, ValidBitsPerSample, (uint)ChannelCombination, SubFormat)
                : new ExtensionPart(
                    BinaryExtensions.ConvertToLittleEndian(extensionSize),
                    BinaryExtensions.ConvertToLittleEndian(ValidBitsPerSample),
                    BinaryExtensions.ConvertToLittleEndian((uint)ChannelCombination),
                    BinaryExtensions.ReverseEndianness(SubFormat));
            MemoryMarshal.Write(span, ref w);
            sink.Write(span);
            sink.Write(extraData.Span);
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ExtensibleWaveFormat"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ExtensibleWaveFormat"/> to compare.</param>
        /// <param name="right">The second <see cref="ExtensibleWaveFormat"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(ExtensibleWaveFormat left, ExtensibleWaveFormat right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ExtensibleWaveFormat"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ExtensibleWaveFormat"/> to compare.</param>
        /// <param name="right">The second  <see cref="ExtensibleWaveFormat"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(ExtensibleWaveFormat left, ExtensibleWaveFormat right) => !(left == right);
    }
}
