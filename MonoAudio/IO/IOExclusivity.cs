using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.IO
{
    /// <summary>
    /// Represents an exclusivity of audio IO.
    /// </summary>
    public enum IOExclusivity
    {
        /// <summary>
        /// The <see cref="ISoundOut"/> will be opened under some sharing system.<br/>
        /// The other applications will be able to access the device.
        /// </summary>
        Shared,

        /// <summary>
        /// The <see cref="ISoundOut"/> will be created exclusively.<br/>
        /// The other applications can't access the device anymore.
        /// </summary>
        Exclusive
    }
}
