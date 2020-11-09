using System;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Represents a sub format for <see cref="ExtensibleWaveFormat.SubFormat"/>.
    /// </summary>
    public static partial class WaveformSubFormats
    {
        /// <summary>
        /// Converts specified <paramref name="encoding"/> to GUID.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static Guid ToGuid(this AudioEncoding encoding) => new Guid((uint)encoding, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        /// <summary>
        /// MPEG-1 audio payload.
        /// </summary>
        public static readonly Guid Mpeg1Packet = new Guid(0xe436eb80, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70);

        /// <summary>
        /// MPEG1 audio packet.
        /// </summary>
        public static readonly Guid Mpeg1Payload = new Guid(0xe436eb81, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70);
    }
}
