using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.IO.Devices;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base structure of audio input device.
    /// </summary>
    public interface IAudioInputDevice<TSoundIn, TAudioDeviceConfiguration, TAudioDeviceConfigurationBuilder> : IAudioDevice
        where TSoundIn : class, ISoundIn where TAudioDeviceConfiguration : IAudioDeviceConfiguration
        where TAudioDeviceConfigurationBuilder : IAudioDeviceConfigurationBuilder<TAudioDeviceConfiguration>, new()
    {
        /// <summary>
        /// Gets the <see cref="IFormatSupportStatusSupport{TAudioDeviceConfiguration}"/> for this <see cref="IAudioInputDevice{TSoundIn, TAudioDeviceConfiguration, TAudioDeviceConfigurationBuilder}"/>.
        /// </summary>
        IFormatSupportStatusSupport<TAudioDeviceConfiguration>? FormatSupportStatusSupport { get; }

        /// <summary>
        /// Gets the <typeparamref name="TAudioDeviceConfiguration"/> that represents the preferred configuration.
        /// </summary>
        TAudioDeviceConfiguration PreferredConfiguration { get; }

        /// <summary>
        /// Creates a new instance of <typeparamref name="TAudioDeviceConfigurationBuilder"/>.
        /// </summary>
        /// <returns>A new instance of <typeparamref name="TAudioDeviceConfigurationBuilder"/>.</returns>
        TAudioDeviceConfigurationBuilder CreateAudioDeviceConfigurationBuilder() => new();

        /// <summary>
        /// Creates the <see cref="ISoundIn"/> that records audio to this device.
        /// </summary>
        /// <param name="configuration">The <typeparamref name="TAudioDeviceConfiguration"/> for initializing input.</param>
        /// <returns>The <see cref="AudioDeviceCreationResult{TSoundDevice}"/> that represents the result of creation.</returns>
        AudioDeviceCreationResult<TSoundIn> CreateSoundIn(TAudioDeviceConfiguration configuration);
    }
}