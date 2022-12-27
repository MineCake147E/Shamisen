using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base infrastructure of audio device that supports querying the format support status.
    /// </summary>
    public interface IFormatSupportStatusSupport<in TAudioDeviceConfiguration> where TAudioDeviceConfiguration : IAudioDeviceConfiguration
    {
        /// <summary>
        /// Indicates whether the audio device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The value which indicates how the <see cref="IWaveFormat"/> can be supported by either frontend or backend.</returns>
        FormatSupportStatus CheckSupportStatus(IWaveFormat format, TAudioDeviceConfiguration? configuration = default);
    }
}
