namespace Shamisen.Formats
{
    /// <summary>
    /// Defines known PCM sample encoding types.
    /// </summary>
    public enum PcmAudioEncoding : uint
    {
        /// <summary>
        /// Linear PCM
        /// </summary>
        [FixedBitDepth]
        LinearPcm = 0x0001,

        /// <summary>
        /// IEEE 754 Single Precision Floating-Point Number
        /// </summary>
        IeeeFloat = 0x0003,

        /// <summary>
        /// A-law PCM from ITU-T Recommendation G.711.
        /// </summary>
        Alaw = 0x0006,

        /// <summary>
        /// μ-law PCM from ITU-T Recommendation G.711.
        /// </summary>
        Mulaw = 0x0007,
    }
}
