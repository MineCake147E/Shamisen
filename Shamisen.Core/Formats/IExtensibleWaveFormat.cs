using System;

namespace Shamisen.Formats
{
    /// <summary>
    /// Represents a base infrastructure of an extensible wave format.
    /// </summary>
    public interface IExtensibleWaveFormat : IWaveFormat, IChannelMaskedFormat
    {
        /// <summary>
        /// Gets the size of the extension.
        /// </summary>
        /// <value>
        /// The size of the extension.
        /// </value>
        ushort ExtensionSize { get; }

        /// <summary>
        /// Gets the valid bits per sample.
        /// </summary>
        /// <value>
        /// The valid bits per sample.
        /// </value>
        ushort ValidBitsPerSample { get; }

        /// <summary>
        /// Gets the sub format.
        /// </summary>
        /// <value>
        /// The sub format.
        /// </value>
        Guid SubFormat { get; }

        /// <summary>
        /// Gets the extra data.
        /// </summary>
        /// <value>
        /// The extra data.
        /// </value>
        ReadOnlyMemory<byte> ExtraData { get; }
    }
}
