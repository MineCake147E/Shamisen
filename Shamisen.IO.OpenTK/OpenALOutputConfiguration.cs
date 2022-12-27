using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.IO.Devices;

namespace Shamisen.IO
{
    /// <summary>
    ///
    /// </summary>
    public readonly struct OpenALOutputConfiguration : IAudioDeviceConfiguration
    {
        /// <summary>
        ///
        /// </summary>
        public OpenALOutputConfiguration() : this(new())
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="latency"></param>
        public OpenALOutputConfiguration(ConfigurationProperty<TimeSpan>? latency)
        {
            Latency = latency;
        }

        /// <inheritdoc/>
        public ConfigurationProperty<TimeSpan>? Latency { get; }
        /// <inheritdoc/>
        public ConfigurationProperty<IOExclusivity>? Exclusivity => null;
    }
}
