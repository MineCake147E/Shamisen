using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Formats;

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
        /// <returns>The value which indicates how the <see cref="IWaveFormat"/> can be supported by <see cref="MonoAudio"/>.</returns>
        FormatSupportStatus CheckSupportStatus(IWaveFormat format);
    }
}
