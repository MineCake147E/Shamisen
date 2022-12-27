using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.IO.Devices;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base structure of audio device's initialization configuration.
    /// </summary>
    public interface IAudioDeviceConfiguration
    {
        /// <summary>
        /// The <see cref="ConfigurationProperty{T}"/> for the latency.<br/>
        /// Returns <see langword="null"/> if it's not supported or if it's not specified at the moment of building.
        /// </summary>
        ConfigurationProperty<TimeSpan>? Latency { get; }

        /// <summary>
        /// The <see cref="ConfigurationProperty{T}"/> for the exclusivity.<br/>
        /// Returns <see langword="null"/> if it's not supported or if it's not specified at the moment of building.
        /// </summary>
        ConfigurationProperty<IOExclusivity>? Exclusivity { get; }
    }
    /// <summary>
    /// Defines a base structure of audio device's mutable initialization configuration.
    /// </summary>
    public interface IAudioDeviceConfigurationBuilder<out TAudioDeviceConfiguration> where TAudioDeviceConfiguration : IAudioDeviceConfiguration
    {
        /// <summary>
        /// The <see cref="ConfigurationProperty{T}"/> for the latency.<br/>
        /// Returns <see langword="null"/> if it's not supported.
        /// </summary>
        MutableConfigurationProperty<TimeSpan>? Latency { get; }

        /// <summary>
        /// The <see cref="ConfigurationProperty{T}"/> for the exclusivity.<br/>
        /// Returns <see langword="null"/> if it's not supported.
        /// </summary>
        MutableConfigurationProperty<IOExclusivity>? Exclusivity { get; }

        /// <summary>
        /// Generates a <typeparamref name="TAudioDeviceConfiguration"/> instance.
        /// </summary>
        /// <returns>A new instance of <typeparamref name="TAudioDeviceConfiguration"/>.</returns>
        TAudioDeviceConfiguration GenerateConfiguration();
    }
}
