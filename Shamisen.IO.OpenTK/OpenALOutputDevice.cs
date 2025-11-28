using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using OpenTK;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;

using Shamisen.Formats;
using Shamisen.IO.Devices;

using ContextAttribute = OpenTK.Audio.OpenAL.ALC.ContextAttribute;
using GetPNameIV = OpenTK.Audio.OpenAL.ALC.GetPNameIV;
using StringName = OpenTK.Audio.OpenAL.ALC.StringName;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Represents a device for OpenAL.
    /// </summary>
    public sealed class OpenALOutputDevice : IAudioOutputDevice<OpenALOutput, OpenALOutputConfiguration, OpenALOutputConfigurationBuilder>, IEquatable<OpenALOutputDevice>, IFormatSupportStatusSupport<OpenALOutputConfiguration>
    {
        private readonly HashSet<string> extensions;
        private readonly int maxSampleRate;
        private readonly HashSet<Format> supportedFormats;

        /// <inheritdoc/>
        public DataFlow DataFlow => DataFlow.Render;

        /// <summary>
        /// Gets the name of this <see cref="OpenALOutputDevice"/>.
        /// </summary>
        public string Name { get; }

        IFormatSupportStatusSupport<OpenALOutputConfiguration>? IAudioOutputDevice<OpenALOutput, OpenALOutputConfiguration, OpenALOutputConfigurationBuilder>.FormatSupportStatusSupport => this;

        /// <inheritdoc/>
        public IWaveFormat? PreferredFormat { get; }
        /// <inheritdoc/>
        public OpenALOutputConfiguration PreferredConfiguration { get; }

        internal OpenALOutputDevice(HashSet<string> extensions, int maxSampleRate, HashSet<Format> supportedFormats, string name)
        {
            this.extensions = extensions ?? throw new ArgumentNullException(nameof(extensions));
            this.maxSampleRate = maxSampleRate;
            this.supportedFormats = supportedFormats ?? throw new ArgumentNullException(nameof(supportedFormats));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        internal static bool TryCreateDevice(string name, [NotNullWhen(true)] out OpenALOutputDevice? openALCDevice)
        {
            ArgumentNullException.ThrowIfNull(name);
            HashSet<Format> supportedFormats = [Format.FormatMono16, Format.FormatMono8, Format.FormatStereo16, Format.FormatStereo8];
            var device = ALC.OpenDevice(name);
            if (device == ALCDevice.Null)
            {
                openALCDevice = null;
                return false;
            }
            ALCContext context;
            unsafe
            {
                context = ALC.CreateContext(device, (int*)null);
            }
            if (context == ALCContext.Null)
            {
                openALCDevice = null;
                return false;
            }
            //sampleRate check
            var attributeSize = ALC.GetInteger(device, GetPNameIV.AttributesSize);
            var attr = new int[attributeSize];
            ALC.GetInteger(device, GetPNameIV.AllAttributes, attributeSize, attr);
            var maxSampleRate = -1;
            for (var i = 0; i < attr.Length; i += 2)
            {
                var flag = (ContextAttribute)attr[i];
                switch (flag)
                {
                    case ContextAttribute.Frequency:
                        maxSampleRate = Math.Max(maxSampleRate, attr[i + 1]);
                        break;
                    default:
                        break;
                }
            }
            var supportedExtensions = ALC.GetString(device, StringName.Extensions) ?? "";
            HashSet<string> extensions = [.. supportedExtensions.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
            CheckExtensionFormats(extensions, supportedFormats);
            ALC.DestroyContext(context);
            openALCDevice = new OpenALOutputDevice(extensions, maxSampleRate, supportedFormats, name);
            return true;
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="OpenALOutputDevice"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="OpenALOutputDevice"/> to compare.</param>
        /// <param name="right">The second  <see cref="OpenALOutputDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(OpenALOutputDevice left, OpenALOutputDevice right) => !(left == right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="OpenALOutputDevice"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="OpenALOutputDevice"/> to compare.</param>
        /// <param name="right">The second <see cref="OpenALOutputDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(OpenALOutputDevice left, OpenALOutputDevice right) => EqualityComparer<OpenALOutputDevice>.Default.Equals(left, right);

        /// <inheritdoc/>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format)
        {
            if (maxSampleRate < 0) return FormatSupportStatus.Unchecked;
            var alf2check = format.ConvertToFormat();
            if (!supportedFormats.Contains(alf2check))
            {
                return format.Channels > 2
                               ? FormatSupportStatus.NotSupported
                               : format.Encoding switch
                               {
                                   AudioEncoding.LinearPcm => format.BitDepth switch
                                   {
                                       24 or 32 => new(FormatPropertySupportStatus.SupportedByBinding),
                                       _ => FormatSupportStatus.NotSupported,
                                   },
                                   AudioEncoding.IeeeFloat => format.BitDepth switch
                                   {
                                       32 => new(FormatPropertySupportStatus.SupportedByBinding),
                                       _ => FormatSupportStatus.NotSupported,
                                   },
                                   _ => FormatSupportStatus.NotSupported,
                               };
            }

            return new(FormatPropertySupportStatus.SupportedByBackend);
        }
        /// <inheritdoc/>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format, OpenALOutputConfiguration configuration = default) => CheckSupportStatus(format);
        /// <inheritdoc/>
        public AudioDeviceCreationResult<OpenALOutput> CreateSoundOut(OpenALOutputConfiguration configuration) => new(new(this, configuration.Latency?.Value ?? TimeSpan.Zero));

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => Equals(obj as OpenALOutputDevice);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(OpenALOutputDevice? other) => other is { } && Name == other.Name;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(IAudioDevice? other) => Equals(other as OpenALOutputDevice);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);

        /// <summary>
        /// Converts the specified <see cref="IWaveFormat"/> to <see cref="Format"/>.
        /// </summary>
        /// <param name="format">The format to convert.</param>
        internal static Format ConvertToFormat(IWaveFormat format)
            => (format.Channels, format.Encoding, format.BitDepth) switch
            {
                (1, AudioEncoding.LinearPcm, 8) => Format.FormatMono8,
                (1, AudioEncoding.LinearPcm, 16) => Format.FormatMono16,
                (1, AudioEncoding.IeeeFloat, 32) => Format.FormatMonoFloat32,
                (1, AudioEncoding.IeeeFloat, 64) => Format.FormatMonoDoubleExt,
                (1, AudioEncoding.Alaw, 8) => Format.FormatMonoAlawExt,
                (1, AudioEncoding.Mulaw, 8) => Format.FormatMonoMulawExt,
                (1, AudioEncoding.ImaAdpcm, 4) => Format.FormatMonoIma4,
                (2, AudioEncoding.LinearPcm, 8) => Format.FormatStereo8,
                (2, AudioEncoding.LinearPcm, 16) => Format.FormatStereo16,
                (2, AudioEncoding.IeeeFloat, 32) => Format.FormatStereoFloat32,
                (2, AudioEncoding.IeeeFloat, 64) => Format.FormatStereoDoubleExt,
                (2, AudioEncoding.Alaw, 8) => Format.FormatStereoAlawExt,
                (2, AudioEncoding.Mulaw, 8) => Format.FormatStereoMulawExt,
                (2, AudioEncoding.ImaAdpcm, 4) => Format.FormatStereoIma4,
                (4, AudioEncoding.LinearPcm, 8) => Format.FormatQuad8,
                (4, AudioEncoding.LinearPcm, 16) => Format.FormatQuad16,
                (4, AudioEncoding.LinearPcm, 32) => Format.FormatQuad32,
                (6, AudioEncoding.LinearPcm, 8) => Format.Format51chn8,
                (6, AudioEncoding.LinearPcm, 16) => Format.Format51chn16,
                (6, AudioEncoding.LinearPcm, 32) => Format.Format51chn32,
                (7, AudioEncoding.LinearPcm, 8) => Format.Format61chn8,
                (7, AudioEncoding.LinearPcm, 16) => Format.Format61chn16,
                (7, AudioEncoding.LinearPcm, 32) => Format.Format61chn32,
                (8, AudioEncoding.LinearPcm, 8) => Format.Format71chn8,
                (8, AudioEncoding.LinearPcm, 16) => Format.Format71chn16,
                (8, AudioEncoding.LinearPcm, 32) => Format.Format71chn32,
                _ => default,
            };

        private static void CheckExtensionFormats(HashSet<string> extensions, HashSet<Format> supportedFormats)
        {
            if (extensions.Contains("AL_EXT_ALAW"))
            {
                _ = supportedFormats.Add(Format.FormatMonoAlawExt);
                _ = supportedFormats.Add(Format.FormatStereoAlawExt);
            }
            if (extensions.Contains("AL_EXT_MULAW"))
            {
                _ = supportedFormats.Add(Format.FormatMonoMulawExt);
                _ = supportedFormats.Add(Format.FormatStereoMulawExt);
            }
            if (extensions.Contains("AL_EXT_mp3"))
                _ = supportedFormats.Add(Format.FormatMp3Ext);
            if (extensions.Contains("AL_EXT_vorbis"))
                _ = supportedFormats.Add(Format.FormatVorbisExt);
            if (extensions.Contains("AL_EXT_MCFORMATS"))
            {
                _ = supportedFormats.Add(Format.Format51chn16);
                _ = supportedFormats.Add(Format.Format51chn32);
                _ = supportedFormats.Add(Format.Format51chn8);
                _ = supportedFormats.Add(Format.Format61chn16);
                _ = supportedFormats.Add(Format.Format61chn32);
                _ = supportedFormats.Add(Format.Format61chn8);
                _ = supportedFormats.Add(Format.Format71chn16);
                _ = supportedFormats.Add(Format.Format71chn32);
                _ = supportedFormats.Add(Format.Format71chn8);
                _ = supportedFormats.Add(Format.FormatQuad16);
                _ = supportedFormats.Add(Format.FormatQuad32);
                _ = supportedFormats.Add(Format.FormatQuad8);
                _ = supportedFormats.Add(Format.FormatRear16);
                _ = supportedFormats.Add(Format.FormatRear32);
                _ = supportedFormats.Add(Format.FormatRear8);
            }
            if (extensions.Contains("AL_EXT_double"))
            {
                _ = supportedFormats.Add(Format.FormatMonoDoubleExt);
                _ = supportedFormats.Add(Format.FormatStereoDoubleExt);
            }
            if (extensions.Contains("AL_EXT_FLOAT32") || extensions.Contains("AL_EXT_float32"))
            {
                _ = supportedFormats.Add(Format.FormatMonoFloat32);
                _ = supportedFormats.Add(Format.FormatStereoFloat32);
            }
            if (extensions.Contains("AL_EXT_IMA4"))
            {
                _ = supportedFormats.Add(Format.FormatMonoIma4);
                _ = supportedFormats.Add(Format.FormatStereoIma4);
            }
        }
    }
}
