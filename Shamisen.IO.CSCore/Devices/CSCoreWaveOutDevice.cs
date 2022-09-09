using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.SoundOut;
using Shamisen.Formats;

namespace Shamisen.IO.Devices
{
    /// <summary>
    /// Represents a device that is available on <see cref="WaveOut"/>.
    /// </summary>
    /// <seealso cref="IAudioOutputDevice{TSoundOut}" />
    public readonly struct CSCoreWaveOutDevice : IAudioOutputDevice<CSCoreSoundOutput>, IEquatable<CSCoreWaveOutDevice>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CSCoreWaveOutDevice"/> struct.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <exception cref="ArgumentNullException">device</exception>
        public CSCoreWaveOutDevice(WaveOutDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);
            Device = device;
        }

        /// <summary>
        /// Gets the name of this audio device.
        /// </summary>
        /// <value>
        /// The name of this audio device.
        /// </value>
        public string Name => Device.Name;

        /// <summary>
        /// Gets the actual device.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        public WaveOutDevice Device { get; }

        /// <summary>
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <returns>
        /// The value which indicates how the <see cref="IWaveFormat" /> can be supported by <see cref="Shamisen" />.
        /// </returns>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format)
        {
            CSCore.WaveFormat nearest = null;
            //check for native support
            foreach (var item in Device.SupportedFormats.Where(a =>
                a.SampleRate == format.SampleRate &&
                a.Channels == format.Channels &&
                a.BitsPerSample == format.BitDepth &&
                (short)a.WaveFormatTag == (short)format.Encoding))  //The CSCore.AudioEncoding has equivalent values to Shamisen.AudioEncoding.
            {
                nearest = item;
            }
            if (!(nearest is null)) return FormatSupportStatus.SupportedByHardware;
            //check for oversampling-only formats
            nearest = Device.SupportedFormats.Where(a =>
                a.SampleRate >= format.SampleRate &&
                a.Channels == format.Channels &&
                a.BitsPerSample == format.BitDepth &&
                (short)a.WaveFormatTag == (short)format.Encoding).OrderBy(a => a.SampleRate).FirstOrDefault();
            if (!(nearest is null)) return FormatSupportStatus.SupportedBySoftware;
            //check for format-converting formats
            //TODO: Implementation
            return FormatSupportStatus.Unchecked;
        }

        /// <summary>
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns>
        /// The value which indicates how the <see cref="T:Shamisen.Formats.IWaveFormat" /> can be supported by <see cref="N:Shamisen" />.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format, IOExclusivity mode = IOExclusivity.Shared)
        {
            if (mode == IOExclusivity.Exclusive) throw new NotSupportedException($"Exclusive mode is not supported!");
            return CheckSupportStatus(format);
        }

        /// <summary>
        /// Creates the <see cref="ISoundOut" /> that outputs audio to this device.
        /// </summary>
        /// <param name="latency">The desired latency for output.</param>
        /// <returns>
        /// The <see cref="CSCoreWaveOutDevice"/> instance.
        /// </returns>
        public CSCoreSoundOutput CreateSoundOut(TimeSpan latency = default)
            => new CSCoreSoundOutput(
                new WaveOut((int)latency.TotalMilliseconds)
                {
                    Device = Device
                });

        /// <summary>
        /// Creates the <see cref="T:Shamisen.IO.ISoundOut" /> that outputs audio to this device with the specified <paramref name="mode" />.
        /// </summary>
        /// <param name="latency">The latency.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public CSCoreSoundOutput CreateSoundOut(TimeSpan latency, IOExclusivity mode)
        {
            if (mode == IOExclusivity.Exclusive) throw new NotSupportedException($"Exclusive mode is not supported!");
            return CreateSoundOut(latency);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is CSCoreWaveOutDevice device && Equals(device);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(CSCoreWaveOutDevice other) => EqualityComparer<WaveOutDevice>.Default.Equals(Device, other.Device);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(IAudioDevice other) => other is CSCoreWaveOutDevice device && Equals(device);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => -684672021 + EqualityComparer<WaveOutDevice>.Default.GetHashCode(Device);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="CSCoreWaveOutDevice"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="CSCoreWaveOutDevice"/> to compare.</param>
        /// <param name="right">The second <see cref="CSCoreWaveOutDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(CSCoreWaveOutDevice left, CSCoreWaveOutDevice right) => EqualityComparer<CSCoreWaveOutDevice>.Default.Equals(left, right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="CSCoreWaveOutDevice"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="CSCoreWaveOutDevice"/> to compare.</param>
        /// <param name="right">The second  <see cref="CSCoreWaveOutDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(CSCoreWaveOutDevice left, CSCoreWaveOutDevice right) => !(left == right);
    }
}
