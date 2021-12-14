using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.IO
{
    /// <summary>
    /// Represents a state of recording.
    /// </summary>
    public enum RecordingState
    {
        /// <summary>
        /// The recording is stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// The recording is in progress.
        /// </summary>
        Recording
    }
}
