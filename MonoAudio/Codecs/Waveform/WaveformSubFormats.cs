using System;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Represents a sub format for <see cref="ExtensibleWaveFormat.subFormat"/>.
    /// </summary>
    public static class WaveformSubFormats
    {
        /// <summary>
        /// IEEE floating-point audio.
        /// </summary>
        public static readonly Guid IeeeFloat = new Guid(0x00000003, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        /// <summary>
        /// PCM audio.
        /// </summary>
        public static readonly Guid Pcm = new Guid(0x00000001, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

        /// <summary>
        /// Dolby AC-3 over S/PDIF.
        /// </summary>
        public static readonly Guid DolbyAc3Spdif = new Guid(0x00000092, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        /// <summary>
        /// AC-3 over S/PDIF;
        /// </summary>
        public static readonly Guid RawSport = new Guid(0x00000240, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        /// <summary>
        /// AC-3 over S/PDIF;
        /// </summary>
        public static readonly Guid SpdifTag_241h = new Guid(0x00000241, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        /// <summary>
        /// Audio with digital rights management (DRM) protection.
        /// </summary>
        public static readonly Guid DrmAudio = new Guid(0x00000009, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        /// <summary>
        /// MPEG-1 audio payload.
        /// </summary>
        public static readonly Guid Mpeg1Packet = new Guid(0xe436eb80, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70);

        /// <summary>
        /// MPEG1 audio packet.
        /// </summary>
        public static readonly Guid Mpeg1Payload = new Guid(0xe436eb81, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70);

        /// <summary>
        /// MPEG1 audio Payload.
        /// </summary>
        public static readonly Guid Mpeg1AudioPayload = new Guid(0x00000050, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
    }
}
