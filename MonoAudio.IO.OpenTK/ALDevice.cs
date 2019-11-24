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
    public sealed class ALDevice : IAudioOutputDevice
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

        /// <summary>
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <returns>
        /// The value which indicates how the <see cref="IWaveFormat" /> can be supported by <see cref="MonoAudio" />.
        /// </returns>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format)
        {
            if (maxSampleRate < 0) return FormatSupportStatus.Unchecked;
            if (format.Channels > 2) return FormatSupportStatus.NotSupported;
            switch (format.Encoding)
            {
                case AudioEncoding.Pcm:
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
    }
}
