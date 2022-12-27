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
    public class OpenALOutputConfigurationBuilder : IAudioDeviceConfigurationBuilder<OpenALOutputConfiguration>
    {
        /// <inheritdoc/>
        public MutableConfigurationProperty<TimeSpan>? Latency { get; } = new();

        /// <inheritdoc/>
        public MutableConfigurationProperty<IOExclusivity>? Exclusivity => null;

        /// <inheritdoc/>
        public OpenALOutputConfiguration GenerateConfiguration() => new(Latency);
    }
}
