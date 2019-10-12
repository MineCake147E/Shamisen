using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.IO
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ALDevice : IAudioOutputDevice
    {
        /// <summary>
        /// Gets the name of this <see cref="ALDevice"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <returns><c>true</c> if succeeded and the audio device supports the specified stream format.</returns>
        public bool IsFormatSupported(WaveFormat format) => throw new NotImplementedException();
    }
}
