using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Formats;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base structure of audio output device.
    /// </summary>
    public interface IAudioOutputDevice<out TSoundOut, in TAudioDeviceConfiguration> : IAudioDevice where TSoundOut : ISoundOut where TAudioDeviceConfiguration : IAudioDeviceConfiguration
    {
        /// <summary>
        /// Gets the <see cref="IFormatSupportStatusSupport{TAudioDeviceConfiguration}"/> for this <see cref="IAudioOutputDevice{TSoundOut, TAudioDeviceConfiguration}"/>.
        /// </summary>
        IFormatSupportStatusSupport<TAudioDeviceConfiguration>? FormatSupportStatusSupport { get; }

        /// <summary>
        /// Creates the <see cref="ISoundOut"/> that outputs audio to this device.
        /// </summary>
        /// <param name="latency">The desired latency for output.</param>
        /// <returns>The <typeparamref name="TSoundOut"/> instance.</returns>
        TSoundOut CreateSoundOut(TimeSpan latency = default);

        /// <summary>
        /// Creates the <see cref="ISoundOut"/> that outputs audio to this device with the specified <paramref name="mode"/>.
        /// </summary>
        /// <param name="latency">The latency.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns></returns>
        TSoundOut CreateSoundOut(TimeSpan latency, IOExclusivity mode);
    }
}