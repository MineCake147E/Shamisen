using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base structure of audio input device.
    /// </summary>
    public interface IAudioInputDevice<out TSoundIn> : IAudioDevice where TSoundIn : ISoundIn
    {
        /// <summary>
        /// Indicates whether the audio input device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns>The value which indicates how the <see cref="IWaveFormat"/> can be supported by <see cref="Shamisen"/>.</returns>
        FormatSupportStatus CheckSupportStatus(IWaveFormat format, IOExclusivity mode = IOExclusivity.Shared);

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
