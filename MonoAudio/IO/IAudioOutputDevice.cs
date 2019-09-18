using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.IO
{
    /// <summary>
    /// Defines a base structure of audio output device.
    /// </summary>
    public interface IAudioOutputDevice : IAudioDevice
    {
        /// <summary>
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <returns><c>true</c> if succeeded and the audio device supports the specified stream format.</returns>
        bool IsFormatSupported(WaveFormat format);
    }
}
