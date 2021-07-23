using System;
using System.Collections.Generic;
using System.Text;
using Shamisen.Formats;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace Shamisen.IO
{
    /// <summary>
    /// Represents a device for OpenAL.
    /// </summary>
    public sealed class OpenALDevice : IAudioOutputDevice<OpenALOutput>, IEquatable<OpenALDevice>
    {
        private readonly int maxSampleRate;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALDevice"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">name</exception>
        internal OpenALDevice(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            maxSampleRate = -1;
            var device = ALC.OpenDevice(Name);
            if (device == IntPtr.Zero) return;
            ALContext context;
            unsafe
            {
                context = ALC.CreateContext(device, (int*)null);
            }
            if (context == ALContext.Null) return;
            //sampleRate check
            ALC.GetInteger(device, AlcGetInteger.AttributesSize, 1, out int asize);
            int[] attr = new int[asize];
            ALC.GetInteger(device, AlcGetInteger.AllAttributes, asize, attr);
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
            ALC.DestroyContext(context);
            _ = ALC.CloseDevice(device);
        }

        /// <summary>
        /// Gets the name of this <see cref="OpenALDevice"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>

        /// <param name="mode">The share mode.</param>
        /// <returns>The value which indicates how the <see cref="IWaveFormat"/> can be supported by <see cref="Shamisen"/>.</returns>
        /// <exception cref="NotSupportedException">The {nameof(IOExclusivity.Exclusive)} mode is not supported!</exception>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format, IOExclusivity mode = IOExclusivity.Shared)
            => mode == IOExclusivity.Exclusive
                ? throw new NotSupportedException($"The {nameof(IOExclusivity.Exclusive)} mode is not supported!")
                : maxSampleRate < 0
                ? FormatSupportStatus.Unchecked
                : format.Channels > 2
                ? FormatSupportStatus.NotSupported
                : (format.Encoding switch
                {
                    AudioEncoding.LinearPcm => format.BitDepth switch
                    {
                        8 or 16 => FormatSupportStatus.SupportedByHardware,
                        24 or 32 => FormatSupportStatus.SupportedBySoftware,
                        _ => FormatSupportStatus.NotSupported,
                    },
                    AudioEncoding.IeeeFloat => FormatSupportStatus.SupportedBySoftware,
                    _ => FormatSupportStatus.NotSupported,
                });

        /// <summary>
        /// Creates the <see cref="ISoundOut" /> that outputs audio to this device.
        /// </summary>
        /// <param name="latency">The desired latency for output.</param>
        /// <returns>
        /// The <see cref="OpenALOutput"/> instance.
        /// </returns>
        public OpenALOutput CreateSoundOut(TimeSpan latency = default) => ((IAudioOutputDevice<OpenALOutput>)this).CreateSoundOut(latency, IOExclusivity.Shared);

        /// <summary>
        /// Creates the <see cref="ISoundOut" /> that outputs audio to this device with the specified <paramref name="mode" />.
        /// </summary>
        /// <param name="latency">The latency.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns></returns>
        OpenALOutput IAudioOutputDevice<OpenALOutput>.CreateSoundOut(TimeSpan latency, IOExclusivity mode) => new OpenALOutput(this, latency);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => Equals(obj as OpenALDevice);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(OpenALDevice? other) => other is { } && Name == other.Name;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(IAudioDevice? other) => Equals(other as OpenALDevice);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="OpenALDevice"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="OpenALDevice"/> to compare.</param>
        /// <param name="right">The second <see cref="OpenALDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(OpenALDevice left, OpenALDevice right) => EqualityComparer<OpenALDevice>.Default.Equals(left, right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="OpenALDevice"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="OpenALDevice"/> to compare.</param>
        /// <param name="right">The second  <see cref="OpenALDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(OpenALDevice left, OpenALDevice right) => !(left == right);
    }
}
