using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Audio.OpenAL;

using Shamisen.Formats;
using Shamisen.IO.Devices;

namespace Shamisen.IO
{
    /// <summary>
    /// Represents a device for OpenAL.
    /// </summary>
    public sealed class OpenALDevice : IAudioOutputDevice<OpenALOutput, OpenALOutputConfiguration>, IEquatable<OpenALDevice>, IFormatSupportStatusSupport<OpenALOutputConfiguration>
    {
        private readonly HashSet<string> extensions;
        private readonly int maxSampleRate;
        private readonly HashSet<ALFormat> supportedFormats;

        /// <inheritdoc/>
        public DataFlow DataFlow => DataFlow.Render;

        /// <summary>
        /// Gets the name of this <see cref="OpenALDevice"/>.
        /// </summary>
        public string Name { get; }

        IFormatSupportStatusSupport<OpenALOutputConfiguration>? IAudioOutputDevice<OpenALOutput, OpenALOutputConfiguration>.FormatSupportStatusSupport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALDevice"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">name</exception>
        internal OpenALDevice(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            maxSampleRate = -1;
            supportedFormats = new(new[] { ALFormat.Mono16, ALFormat.Mono8, ALFormat.Stereo16, ALFormat.Stereo8 });
            var device = ALC.OpenDevice(Name);
            if (device == IntPtr.Zero)
            {
                extensions = new();
                return;
            }
            ALContext context;
            unsafe
            {
                context = ALC.CreateContext(device, (int*)null);
            }
            if (context == ALContext.Null)
            {
                extensions = new();
                return;
            }
            //sampleRate check
            ALC.GetInteger(device, AlcGetInteger.AttributesSize, 1, out var asize);
            var attr = new int[asize];
            ALC.GetInteger(device, AlcGetInteger.AllAttributes, asize, attr);
            var fmax = -1;
            for (var i = 0; i < attr.Length; i += 2)
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
            var exts = ALC.GetString(device, AlcGetString.Extensions);
            extensions = new(exts.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            CheckExtensionFormats(extensions, supportedFormats);
            ALC.DestroyContext(context);
            _ = ALC.CloseDevice(device);
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="OpenALDevice"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="OpenALDevice"/> to compare.</param>
        /// <param name="right">The second  <see cref="OpenALDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(OpenALDevice left, OpenALDevice right) => !(left == right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="OpenALDevice"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="OpenALDevice"/> to compare.</param>
        /// <param name="right">The second <see cref="OpenALDevice"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(OpenALDevice left, OpenALDevice right) => EqualityComparer<OpenALDevice>.Default.Equals(left, right);

        /// <inheritdoc/>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format)
        {
            if (maxSampleRate < 0) return FormatSupportStatus.Unchecked;
            var alf2check = ConvertToALFormat(format);
            if (supportedFormats.Contains(alf2check))
            {
                return FormatSupportStatus.SupportedByHardware;
            }
            return format.Channels > 2
                           ? FormatSupportStatus.NotSupported
                           : (format.Encoding switch
                           {
                               AudioEncoding.LinearPcm => format.BitDepth switch
                               {
                                   24 or 32 => FormatSupportStatus.SupportedByBinding,
                                   _ => FormatSupportStatus.NotSupported,
                               },
                               _ => FormatSupportStatus.NotSupported,
                           });
        }
        /// <inheritdoc/>
        public FormatSupportStatus CheckSupportStatus(IWaveFormat format, OpenALOutputConfiguration configuration = default) => CheckSupportStatus(format);
        /// <inheritdoc/>
        public OpenALOutput CreateSoundOut(OpenALOutputConfiguration configuration) => new(this, configuration.Latency?.Value ?? TimeSpan.Zero);

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
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        internal static ALFormat ConvertToALFormat(IWaveFormat format)
            => (format.Channels, format.Encoding, format.BitDepth) switch
            {
                (1, AudioEncoding.LinearPcm, 8) => ALFormat.Mono8,
                (1, AudioEncoding.LinearPcm, 16) => ALFormat.Mono16,
                (1, AudioEncoding.IeeeFloat, 32) => ALFormat.MonoFloat32Ext,
                (1, AudioEncoding.IeeeFloat, 64) => ALFormat.MonoDoubleExt,
                (1, AudioEncoding.Alaw, 8) => ALFormat.MonoALawExt,
                (1, AudioEncoding.Mulaw, 8) => ALFormat.MonoMuLawExt,
                (1, AudioEncoding.ImaAdpcm, 4) => ALFormat.MonoIma4Ext,
                (2, AudioEncoding.LinearPcm, 8) => ALFormat.Stereo8,
                (2, AudioEncoding.LinearPcm, 16) => ALFormat.Stereo16,
                (2, AudioEncoding.IeeeFloat, 32) => ALFormat.StereoFloat32Ext,
                (2, AudioEncoding.IeeeFloat, 64) => ALFormat.StereoDoubleExt,
                (2, AudioEncoding.Alaw, 8) => ALFormat.StereoALawExt,
                (2, AudioEncoding.Mulaw, 8) => ALFormat.StereoMuLawExt,
                (2, AudioEncoding.ImaAdpcm, 4) => ALFormat.StereoIma4Ext,
                (4, AudioEncoding.LinearPcm, 8) => ALFormat.MultiQuad8Ext,
                (4, AudioEncoding.LinearPcm, 16) => ALFormat.MultiQuad16Ext,
                (4, AudioEncoding.LinearPcm, 32) => ALFormat.MultiQuad32Ext,
                (6, AudioEncoding.LinearPcm, 8) => ALFormat.Multi51Chn8Ext,
                (6, AudioEncoding.LinearPcm, 16) => ALFormat.Multi51Chn16Ext,
                (6, AudioEncoding.LinearPcm, 32) => ALFormat.Multi51Chn32Ext,
                (7, AudioEncoding.LinearPcm, 8) => ALFormat.Multi61Chn8Ext,
                (7, AudioEncoding.LinearPcm, 16) => ALFormat.Multi61Chn16Ext,
                (7, AudioEncoding.LinearPcm, 32) => ALFormat.Multi61Chn32Ext,
                (8, AudioEncoding.LinearPcm, 8) => ALFormat.Multi71Chn8Ext,
                (8, AudioEncoding.LinearPcm, 16) => ALFormat.Multi71Chn16Ext,
                (8, AudioEncoding.LinearPcm, 32) => ALFormat.Multi71Chn32Ext,
                _ => default,
            };

        private static void CheckExtensionFormats(HashSet<string> extensions, HashSet<ALFormat> supportedFormats)
        {
            if (extensions.Contains("AL_EXT_ALAW"))
            {
                _ = supportedFormats.Add(ALFormat.MonoALawExt);
                _ = supportedFormats.Add(ALFormat.StereoALawExt);
            }
            if (extensions.Contains("AL_EXT_MULAW"))
            {
                _ = supportedFormats.Add(ALFormat.MonoMuLawExt);
                _ = supportedFormats.Add(ALFormat.StereoMuLawExt);
            }
            if (extensions.Contains("AL_EXT_mp3"))
            {
                _ = supportedFormats.Add(ALFormat.Mp3Ext);
            }
            if (extensions.Contains("AL_EXT_vorbis"))
            {
                _ = supportedFormats.Add(ALFormat.VorbisExt);
            }
            if (extensions.Contains("AL_EXT_MCFORMATS"))
            {
                _ = supportedFormats.Add(ALFormat.Multi51Chn16Ext);
                _ = supportedFormats.Add(ALFormat.Multi51Chn32Ext);
                _ = supportedFormats.Add(ALFormat.Multi51Chn8Ext);
                _ = supportedFormats.Add(ALFormat.Multi61Chn16Ext);
                _ = supportedFormats.Add(ALFormat.Multi61Chn32Ext);
                _ = supportedFormats.Add(ALFormat.Multi61Chn8Ext);
                _ = supportedFormats.Add(ALFormat.Multi71Chn16Ext);
                _ = supportedFormats.Add(ALFormat.Multi71Chn32Ext);
                _ = supportedFormats.Add(ALFormat.Multi71Chn8Ext);
                _ = supportedFormats.Add(ALFormat.MultiQuad16Ext);
                _ = supportedFormats.Add(ALFormat.MultiQuad32Ext);
                _ = supportedFormats.Add(ALFormat.MultiQuad8Ext);
                _ = supportedFormats.Add(ALFormat.MultiRear16Ext);
                _ = supportedFormats.Add(ALFormat.MultiRear32Ext);
                _ = supportedFormats.Add(ALFormat.MultiRear8Ext);
            }
            if (extensions.Contains("AL_EXT_double"))
            {
                _ = supportedFormats.Add(ALFormat.MonoDoubleExt);
                _ = supportedFormats.Add(ALFormat.StereoDoubleExt);
            }
            if (extensions.Contains("AL_EXT_float32"))
            {
                _ = supportedFormats.Add(ALFormat.MonoFloat32Ext);
                _ = supportedFormats.Add(ALFormat.StereoFloat32Ext);
            }
            if (extensions.Contains("AL_EXT_IMA4"))
            {
                _ = supportedFormats.Add(ALFormat.MonoIma4Ext);
                _ = supportedFormats.Add(ALFormat.StereoIma4Ext);
            }
        }
    }
}
