using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.IO
{
    /// <summary>
    /// Represents an event arguments that tells you that the recording has (been) stopped and holds why.<br/>
    /// </summary>
    /// <seealso cref="StoppedEventArgs" />
    public class RecordingStoppedEventArgs : StoppedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingStoppedEventArgs"/> class.
        /// </summary>
        public RecordingStoppedEventArgs() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingStoppedEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public RecordingStoppedEventArgs(Exception exception) : base(exception)
        {
        }
    }
}
