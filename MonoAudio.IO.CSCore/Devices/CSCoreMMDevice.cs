using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using MonoAudio.Formats;
using MonoAudio.Synthesis;
using CFormat = CSCore.WaveFormat;

namespace MonoAudio.IO.Devices
{
    /// <summary>
    /// Represents a device for <see cref="WasapiOut"/>.
    /// </summary>
    public readonly struct CSCoreMMDevice : IAudioOutputDevice<CSCoreSoundOutput>, IEquatable<CSCoreMMDevice>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="device"></param>
        internal CSCoreMMDevice(MMDevice device)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
        }

        /// <summary>
        /// Gets the name of this audio device.
        /// </summary>
        /// <value>
        /// The name of this audio device.
        /// </value>
        public string Name => Device.FriendlyName;

        /// <summary>
        /// Gets the actual device.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        public MMDevice Device { get; }

        /// <summary>
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns>
        /// The value which indicates how the <see cref="IWaveFormat" /> can be supported by <see cref="MonoAudio" />.
        /// </returns>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format, IOExclusivity mode = IOExclusivity.Shared)
        {
            switch (format.Encoding)
            {
                case AudioEncoding.Pcm:
                case AudioEncoding.IeeeFloat:
                    break;
                default:
                    return FormatSupportStatus.NotSupported;
            }
            try
            {
                using (var g = new WasapiOut(true, mode == IOExclusivity.Exclusive ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared, 10))
                using (var ss = new SilenceWaveSource(format))
                using (var iws = new CSCoreInteroperatingWaveSource(ss))
                {
                    g.Device = Device;
                    g.Initialize(iws);
                }
                return FormatSupportStatus.SupportedBySoftware;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (NotSupportedException)
            {
                return FormatSupportStatus.NotSupported;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// false if NOT supported
        /// </summary>
        /// <param name="ac"></param>
        /// <param name="closet"></param>
        /// <returns></returns>
        private static bool CheckInternalSupport(AudioClient ac, CFormat closet) => !(closet is null) && ac.IsFormatSupported(AudioClientShareMode.Exclusive, closet);

        /// <summary>
        /// Creates the <see cref="ISoundOut" /> that outputs audio to this device.
        /// </summary>
        /// <param name="latency">The desired latency for output.</param>
        /// <returns>
        /// The <see cref="CSCoreSoundOutput"/> instance.
        /// </returns>
        public CSCoreSoundOutput CreateSoundOut(TimeSpan latency = default) => CreateSoundOut(latency, IOExclusivity.Shared);

        /// <summary>
        /// Creates the <see cref="ISoundOut" /> that outputs audio to this device with the specified <paramref name="mode" />.
        /// </summary>
        /// <param name="latency">The latency.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public CSCoreSoundOutput CreateSoundOut(TimeSpan latency, IOExclusivity mode)
            => new CSCoreSoundOutput(
                new WasapiOut(true, (AudioClientShareMode)(int)mode, (int)latency.TotalMilliseconds)
                {
                    Device = Device
                });

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is CSCoreMMDevice device && Equals(device);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(CSCoreMMDevice other) => EqualityComparer<MMDevice>.Default.Equals(Device, other.Device);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => -684672021 + EqualityComparer<MMDevice>.Default.GetHashCode(Device);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="CSCoreMMDevice"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="CSCoreMMDevice"/> to compare.</param>
        /// <param name="right">The second <see cref="CSCoreMMDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(CSCoreMMDevice left, CSCoreMMDevice right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="CSCoreMMDevice"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="CSCoreMMDevice"/> to compare.</param>
        /// <param name="right">The second  <see cref="CSCoreMMDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(CSCoreMMDevice left, CSCoreMMDevice right) => !(left == right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Equals(IAudioDevice other) => other is CSCoreMMDevice device && Equals(device);
    }
}
