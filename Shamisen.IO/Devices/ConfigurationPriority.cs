using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.IO.Devices
{
    /// <summary>
    /// Defines the values for priority of settings.<br/>
    /// The "best" in documents of each values means highest for <see cref="IAudioFormat{TSample}.SampleRate"/> and <see cref="IAudioFormat{TSample}.BitDepth"/>,
    /// lowest for <see cref="IAudioDeviceConfiguration.Latency"/>.<br/>
    /// For <see cref="IAudioFormat{TSample}.Channels"/>, the device should be rendering each channels as close as it's originally intended as a result of initialization.
    /// </summary>
    public enum ConfigurationPropertyPriority
    {
        /// <summary>
        /// If the value is set, the initializer should set the property exactly to the specified value, but can ignore the setting if it's impossible to obey.
        /// </summary>
        Optional,
        /// <summary>
        /// The initializer should set the property to the best possible value, regardless of the set value.
        /// </summary>
        BestPossible,
        /// <summary>
        /// The initializer should set the property to a minimum value greater than the specified value.<br/>
        /// The initializer can compromise the setting if it's impossible to meet at least the minimum necessary conditions.
        /// </summary>
        BestEffort,
        /// <summary>
        /// The initializer must set the property to the best possible value, regardless of the set value.<br/>
        /// The initializer must cancel the initialization if it's impossible to meet at least the minimum necessary conditions.
        /// </summary>
        RequiredBestOrExact,
        /// <summary>
        /// The initializer must set the property to a minimum value greater than the specified value.<br/>
        /// The initializer must cancel the initialization if it's impossible to meet at least the minimum necessary conditions.
        /// </summary>
        RequiredExactOrBetter,
        /// <summary>
        /// The initializer must set the property exactly to the specified value.<br/>
        /// The initializer must cancel the initialization if it's impossible to set the specified conditions exactly.
        /// </summary>
        RequiredExact
    }
}
