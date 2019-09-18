using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.IO
{
    /// <summary>
    /// Defines a base structure of an audio device.
    /// </summary>
    public interface IAudioDevice
    {
        /// <summary>
        /// Gets the name of this audio device.
        /// </summary>
        /// <value>
        /// The name of this audio device.
        /// </value>
        string Name { get; }
    }
}
