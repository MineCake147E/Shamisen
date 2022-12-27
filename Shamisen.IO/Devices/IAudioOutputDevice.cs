using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Formats;
using Shamisen.IO.Devices;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base structure of audio output device.
    /// </summary>
    public interface IAudioOutputDevice<TSoundOut, TAudioDeviceConfiguration, TAudioDeviceConfigurationBuilder> : IAudioDevice
        where TSoundOut : class, ISoundOut where TAudioDeviceConfiguration : IAudioDeviceConfiguration
        where TAudioDeviceConfigurationBuilder : IAudioDeviceConfigurationBuilder<TAudioDeviceConfiguration>, new()
    {
        /// <summary>
        /// Gets the <see cref="IFormatSupportStatusSupport{TAudioDeviceConfiguration}"/> for this <see cref="IAudioOutputDevice{TSoundOut, TAudioDeviceConfiguration, TAudioDeviceConfigurationBuilder}"/>.
        /// </summary>
        IFormatSupportStatusSupport<TAudioDeviceConfiguration>? FormatSupportStatusSupport { get; }

        /// <summary>
        /// Gets the preferred <see cref="IWaveFormat"/>, or <see langword="null"/> if not supported.
        /// </summary>
        IWaveFormat? PreferredFormat { get; }

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
        /// Creates the <see cref="ISoundOut"/> that outputs audio to this device.
        /// </summary>
        /// <param name="configuration">The configuration for output.</param>
        /// <returns>The <see cref="AudioDeviceCreationResult{TSoundDevice}"/> that represents the result of creation.</returns>
        AudioDeviceCreationResult<TSoundOut> CreateSoundOut(TAudioDeviceConfiguration configuration);
    }
}