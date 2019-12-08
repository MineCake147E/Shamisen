using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Represents an "extension" part of <see cref="ExtensibleWaveFormat"/>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct ExtensionPart : IEndiannessReversible<ExtensionPart>
    {
        [FieldOffset(0)]
        private readonly ushort extensionSize;

        [FieldOffset(2)]
        private readonly ushort validBitsPerSample;

        [FieldOffset(4)]
        private readonly uint channelMask;

        [FieldOffset(8)]
        private readonly Guid subFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionPart"/> struct.
        /// </summary>
        /// <param name="extensionSize">Size of the extension.</param>
        /// <param name="validBitsPerSample">The valid bits per sample.</param>
        /// <param name="channelMask">The channel mask.</param>
        /// <param name="subFormat">The sub format.</param>
        public ExtensionPart(ushort extensionSize, ushort validBitsPerSample, uint channelMask, Guid subFormat)
        {
            this.extensionSize = extensionSize;
            this.validBitsPerSample = validBitsPerSample;
            this.channelMask = channelMask;
            this.subFormat = subFormat;
        }

        /// <summary>
        /// Gets the size of the extension region data.
        /// </summary>
        /// <value>
        /// The size of the extension region data.
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
        /// Gets the channel mask.
        /// </summary>
        /// <value>
        /// The channel mask.
        /// </value>
        public uint ChannelMask => channelMask;

        /// <summary>
        /// Gets the sub format.
        /// </summary>
        /// <value>
        /// The sub format.
        /// </value>
        public Guid SubFormat => subFormat;

        /// <summary>
        /// Reverses endianness for all fields, and returns a new value.
        /// </summary>
        /// <returns></returns>
        public ExtensionPart ReverseEndianness()
        {
            var xS = BinaryPrimitives.ReverseEndianness(extensionSize);
            var vBPS = BinaryPrimitives.ReverseEndianness(validBitsPerSample);
            var cM = BinaryPrimitives.ReverseEndianness(channelMask);
            var sF = BinaryExtensions.ReverseEndianness(subFormat);
            return new ExtensionPart(xS, vBPS, cM, sF);
        }
    }
}
