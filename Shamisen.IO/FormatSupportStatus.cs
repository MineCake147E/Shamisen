using System.Runtime.CompilerServices;

namespace Shamisen.IO
{
    /// <summary>
    /// Represents the support status of certain values in format in certain device.
    /// </summary>
    public enum FormatPropertySupportStatus : byte
    {
        /// <summary>
        /// The <see cref="IAudioDevice"/> had no ability to check the support status for specified values.
        /// </summary>
        Unchecked,
        /// <summary>
        /// The specified values is not supported by both device and binding.
        /// </summary>
        NotSupported,
        /// <summary>
        /// The specified values is supported by the frontend, but frontend certainly performs additional conversion, which may result in lower quality.
        /// </summary>
        SupportedByBinding,
        /// <summary>
        /// The specified values is supported by the backend, but backend may perform additional conversion, which may result in lower quality.
        /// </summary>
        SupportedByBackend,
        /// <summary>
        /// The specified values is certainly supported by the hardware and does not require any additional conversion.
        /// </summary>
        SupportedByHardware
    }

    /// <summary>
    /// Represents the support status of certain format in certain device.
    /// </summary>
    public readonly struct FormatSupportStatus
    {
        #region Properties
        /// <summary>
        /// Gets the <see cref="FormatPropertySupportStatus"/> for <see cref="IAudioFormat{TSample}.SampleRate"/> property.
        /// </summary>
        public FormatPropertySupportStatus SampleRate { get; }
        /// <summary>
        /// Gets the <see cref="FormatPropertySupportStatus"/> for <see cref="IAudioFormat{TSample}.Channels"/> property.
        /// </summary>
        public FormatPropertySupportStatus Channels { get; }
        /// <summary>
        /// Gets the <see cref="FormatPropertySupportStatus"/> for <see cref="IAudioFormat{TSample}.BitDepth"/> property.
        /// </summary>
        public FormatPropertySupportStatus BitDepth { get; }
        /// <summary>
        /// Gets the <see cref="FormatPropertySupportStatus"/> for <see cref="IWaveFormat.Encoding"/> property.
        /// </summary>
        public FormatPropertySupportStatus Encoding { get; }
        #endregion
        #region Static Properties

        /// <summary>
        /// Returns the value that specifies all fields to be <see cref="FormatPropertySupportStatus.Unchecked"/>.
        /// </summary>
        public static FormatSupportStatus Unchecked
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new(FormatPropertySupportStatus.Unchecked);
        }

        /// <summary>
        /// Returns the value that specifies all fields to be <see cref="FormatPropertySupportStatus.Unchecked"/>.
        /// </summary>
        public static FormatSupportStatus NotSupported
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => new(FormatPropertySupportStatus.NotSupported);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="FormatSupportStatus"/>.
        /// </summary>
        /// <param name="value">The <see cref="FormatPropertySupportStatus"/> for all properties.</param>
        public FormatSupportStatus(FormatPropertySupportStatus value)
        {
            SampleRate = Channels = BitDepth = Encoding = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FormatSupportStatus"/>.
        /// </summary>
        /// <param name="sampleRate">The <see cref="FormatPropertySupportStatus"/> for <see cref="IAudioFormat{TSample}.SampleRate"/> property.</param>
        /// <param name="channels">The <see cref="FormatPropertySupportStatus"/> for <see cref="IAudioFormat{TSample}.Channels"/> property.</param>
        /// <param name="bitDepth">The <see cref="FormatPropertySupportStatus"/> for <see cref="IAudioFormat{TSample}.BitDepth"/> property.</param>
        /// <param name="encoding">The <see cref="FormatPropertySupportStatus"/> for <see cref="IWaveFormat.Encoding"/> property.</param>
        public FormatSupportStatus(FormatPropertySupportStatus sampleRate, FormatPropertySupportStatus channels, FormatPropertySupportStatus bitDepth, FormatPropertySupportStatus encoding)
        {
            SampleRate = sampleRate;
            Channels = channels;
            BitDepth = bitDepth;
            Encoding = encoding;
        }
    }
}
