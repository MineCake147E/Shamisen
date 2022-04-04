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
        /// The <see cref="IConfigurationProperty{T}"/> for the latency.<br/>
        /// Returns <see langword="null"/> if it's not supported.
        /// </summary>
        IConfigurationProperty<TimeSpan>? Latency { get; }

        /// <summary>
        /// The <see cref="IConfigurationProperty{T}"/> for the exclusivity.<br/>
        /// Returns <see langword="null"/> if it's not supported.
        /// </summary>
        IConfigurationProperty<IOExclusivity>? Exclusivity { get; }
    }
    /// <summary>
    /// Defines a base structure of audio device's mutable initialization configuration.
    /// </summary>
    public interface IMutableAudioDeviceConfiguration<out TAudioDeviceConfiguration> where TAudioDeviceConfiguration : IAudioDeviceConfiguration
    {
        /// <summary>
        /// The <see cref="IConfigurationProperty{T}"/> for the latency.<br/>
        /// Returns <see langword="null"/> if it's not supported.
        /// </summary>
        IMutableConfigurationProperty<TimeSpan>? Latency { get; }

        /// <summary>
        /// The <see cref="IConfigurationProperty{T}"/> for the exclusivity.<br/>
        /// Returns <see langword="null"/> if it's not supported.
        /// </summary>
        IMutableConfigurationProperty<IOExclusivity>? Exclusivity { get; }

        /// <summary>
        /// Generates a <typeparamref name="TAudioDeviceConfiguration"/> instance.
        /// </summary>
        /// <returns></returns>
        TAudioDeviceConfiguration GenerateConfiguration();
    }
}
