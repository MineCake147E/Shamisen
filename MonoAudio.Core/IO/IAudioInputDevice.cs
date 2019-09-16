using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.IO
{
    /// <summary>
    /// Defines a base structure of audio input device.
    /// </summary>
    public interface IAudioInputDevice : IAudioDevice
    {
        /// <summary>
        /// Indicates whether the audio input device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <returns><c>true</c> if succeeded and the audio device supports the specified stream format.</returns>
        bool IsFormatSupported(WaveFormat format);
    }
}
