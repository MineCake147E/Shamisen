using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base structure of audio input device.
    /// </summary>
    public interface IAudioInputDevice<out TSoundIn, in TAudioDeviceConfiguration> : IAudioDevice where TSoundIn : ISoundIn where TAudioDeviceConfiguration : IAudioDeviceConfiguration
    {
        /// <summary>
        /// Gets the <see cref="IFormatSupportStatusSupport{TAudioDeviceConfiguration}"/> for this <see cref="IAudioInputDevice{TSoundIn, TAudioDeviceConfiguration}"/>.
        /// </summary>
        IFormatSupportStatusSupport<TAudioDeviceConfiguration>? FormatSupportStatusSupport { get; }

        /// <summary>
        /// Creates the <see cref="ISoundIn"/> that records audio to this device.
        /// </summary>
        /// <param name="latency">The desired latency for input.</param>
        /// <returns>The <typeparamref name="TSoundIn"/> instance.</returns>
        TSoundIn CreateSoundIn(TimeSpan latency = default);

        /// <summary>
        /// Creates the <see cref="ISoundIn"/> that records audio to this device with the specified <paramref name="mode"/>.
        /// </summary>
        /// <param name="latency">The latency.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns></returns>
        TSoundIn CreateSoundIn(TimeSpan latency, IOExclusivity mode);
    }
}