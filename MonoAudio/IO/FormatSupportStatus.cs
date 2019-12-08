using System;
using System.Runtime.InteropServices;

using MonoAudio.Formats;

using TFlags = System.Byte;

namespace MonoAudio.IO
{
    /// <summary>
    /// Indicates how the <see cref="IWaveFormat"/> is supported by the <see cref="MonoAudio"/>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = sizeof(TFlags))]
    public readonly struct FormatSupportStatus : IEquatable<FormatSupportStatus>
    {
        [FieldOffset(0)]
        private readonly TFlags value;

        private const TFlags FlagChecked = 0b1;

        private const TFlags FlagHasSoftwareSupport = 0b10;

        private const TFlags FlagNativelySupported = 0b100;

        private const TFlags FlagSupported = 0b110;

        /// <summary>
        /// The value which indicates the <see cref="IAudioDevice"/> has no ability to check the support status currently.
        /// </summary>
        public static readonly FormatSupportStatus Unchecked = new FormatSupportStatus(true, false, false);

        /// <summary>
        /// The value which indicates the <see cref="IWaveFormat"/> is not supported by the <see cref="IAudioDevice"/>.
        /// </summary>
        public static readonly FormatSupportStatus NotSupported = new FormatSupportStatus(true, false, false);

        /// <summary>
        /// The value which indicates the <see cref="IWaveFormat"/> is supported by the <see cref="IAudioDevice"/>, by converting the audio into some different format.
        /// </summary>
        public static readonly FormatSupportStatus SupportedBySoftware = new FormatSupportStatus(true, true, false);

        /// <summary>
        /// The value which indicates the <see cref="IWaveFormat"/> is supported by the <see cref="IAudioDevice"/> natively, without converting the audio into some different format.
        /// </summary>
        public static readonly FormatSupportStatus SupportedByHardware = new FormatSupportStatus(true, false, true);

        private FormatSupportStatus(bool isChecked, bool hasSoftwareSupport, bool isNativelySupported)
        {
            TFlags v = 0;
            if (isChecked) v |= FlagChecked;
            if (hasSoftwareSupport) v |= FlagHasSoftwareSupport;
            if (isNativelySupported) v |= FlagNativelySupported;
            value = v;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IWaveFormat"/> has been checked the availability on the device.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsChecked => (value & FlagChecked) > 0;

        /// <summary>
        /// Gets a value indicating whether the format is supported.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is supported; otherwise, <c>false</c>.
        /// </value>
        public bool IsSupported => (value & FlagSupported) > 0;

        /// <summary>
        /// Gets a value indicating whether the <see cref="IWaveFormat"/> is natively supported.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is natively supported; otherwise, <c>false</c>.
        /// </value>
        public bool IsNativelySupported => (value & FlagNativelySupported) > 0;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj) => obj is FormatSupportStatus status && Equals(status);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(FormatSupportStatus other) => value == other.value;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => -1584136870 + value.GetHashCode();

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FormatSupportStatus"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="FormatSupportStatus"/> to compare.</param>
        /// <param name="right">The second <see cref="FormatSupportStatus"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(FormatSupportStatus left, FormatSupportStatus right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FormatSupportStatus"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="FormatSupportStatus"/> to compare.</param>
        /// <param name="right">The second  <see cref="FormatSupportStatus"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(FormatSupportStatus left, FormatSupportStatus right) => !(left == right);
    }
}
