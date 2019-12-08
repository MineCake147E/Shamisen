using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Formats;
using OpenTK;
using OpenTK.Audio.OpenAL;


namespace MonoAudio.IO
{
    /// <summary>
    /// Represents a device for OpenAL.
    /// </summary>
    public sealed class ALDevice : IAudioOutputDevice<ALOutput>, IEquatable<ALDevice>
    {
        private readonly int maxSampleRate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ALDevice"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">name</exception>
        internal ALDevice(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            maxSampleRate = -1;
            var device = Alc.OpenDevice(Name);
            if (device == IntPtr.Zero) return;
            var context = Alc.CreateContext(device, (int[])null);
            if (context == ContextHandle.Zero) return;
            //sampleRate check
            Alc.GetInteger(device, AlcGetInteger.AttributesSize, 1, out int asize);
            int[] attr = new int[asize];
            Alc.GetInteger(device, AlcGetInteger.AllAttributes, asize, attr);
            int fmax = -1;
            for (int i = 0; i < attr.Length; i += 2)
            {
                var flag = (AlcContextAttributes)attr[i];
                switch (flag)
                {
                    case AlcContextAttributes.Frequency:
                        fmax = Math.Max(fmax, attr[i + 1]);
                        break;
                    default:
                        break;
                }
            }
            maxSampleRate = fmax;
            Alc.DestroyContext(context);
            Alc.CloseDevice(device);
        }
        /// <summary>
        /// Gets the name of this <see cref="ALDevice"/>.
        /// </summary>
        public string Name { get; }

        string IAudioDevice.Name { get; }

        /// <summary>
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>

        /// <param name="mode">The share mode.</param>
        /// <returns>The value which indicates how the <see cref="IWaveFormat"/> can be supported by <see cref="MonoAudio"/>.</returns>
        /// <exception cref="NotSupportedException">The {nameof(IOExclusivity.Exclusive)} mode is not supported!</exception>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format, IOExclusivity mode = IOExclusivity.Shared)
        {
            if (mode == IOExclusivity.Exclusive) throw new NotSupportedException($"The {nameof(IOExclusivity.Exclusive)} mode is not supported!");
            if (maxSampleRate < 0) return FormatSupportStatus.Unchecked;
            if (format.Channels > 2) return FormatSupportStatus.NotSupported;
            switch (format.Encoding)
            {
                case AudioEncoding.LinearPcm:
                    break;
                case AudioEncoding.IeeeFloat:
                    return FormatSupportStatus.SupportedBySoftware;
                default:
                    return FormatSupportStatus.NotSupported;
            }
            switch (format.BitDepth)
            {
                case 8:
                case 16:
                    return FormatSupportStatus.SupportedByHardware;
                case 24:
                case 32:
                    return FormatSupportStatus.SupportedBySoftware;
                default:
                    return FormatSupportStatus.NotSupported;
            }
        }

        /// <summary>
        /// Creates the <see cref="ISoundOut" /> that outputs audio to this device.
        /// </summary>
        /// <param name="latency">The desired latency for output.</param>
        /// <returns>
        /// The <see cref="ALOutput"/> instance.
        /// </returns>
        public ALOutput CreateSoundOut(TimeSpan latency = default) => ((IAudioOutputDevice<ALOutput>)this).CreateSoundOut(latency, IOExclusivity.Shared);

        /// <summary>
        /// Creates the <see cref="ISoundOut" /> that outputs audio to this device with the specified <paramref name="mode" />.
        /// </summary>
        /// <param name="latency">The latency.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns></returns>
        ALOutput IAudioOutputDevice<ALOutput>.CreateSoundOut(TimeSpan latency, IOExclusivity mode) => new ALOutput(this, latency);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => Equals(obj as ALDevice);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ALDevice other) => other != null && Name == other.Name;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(IAudioDevice other) => Equals(other as ALDevice);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ALDevice"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ALDevice"/> to compare.</param>
        /// <param name="right">The second <see cref="ALDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(ALDevice left, ALDevice right) => EqualityComparer<ALDevice>.Default.Equals(left, right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ALDevice"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ALDevice"/> to compare.</param>
        /// <param name="right">The second  <see cref="ALDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(ALDevice left, ALDevice right) => !(left == right);

    }
}
