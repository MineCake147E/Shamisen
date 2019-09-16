using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.IO
{
    /// <summary>
    /// Represents a flow kind of audio data.
    /// </summary>
    [Flags]
    public enum DataFlow
    {
        /// <summary>
        /// Invalid.
        /// </summary>
        None = 0,

        /// <summary>
        /// Output.
        /// </summary>
        Render = 1,

        /// <summary>
        /// Input.
        /// </summary>
        Capture = 2,
    }
}
