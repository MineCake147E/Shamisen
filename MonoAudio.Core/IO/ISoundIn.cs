using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Formats;

namespace MonoAudio.IO
{
    /// <summary>
    /// Defines a base infrastructure of a sound input.<br/>
    /// CAUTION! IT HAS SOME EVENT HANDLERS! IMPLEMENTERS MUST NOT FORGET TO RELEASE THEM!
    /// </summary>
    public interface ISoundIn : IDisposable
    {
        /// <summary>
        /// Occurs when some data are available.
        /// </summary>
        event DataAvailableEventHandler DataAvailable;

        /// <summary>
        /// Occurs when the recording stopped.
        /// </summary>
        event EventHandler<RecordingStoppedEventArgs> Stopped;

        /// <summary>
        /// Starts recording.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops recording.
        /// </summary>
        void Stop();

        /// <summary>
        /// Initializes the recorder.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets the state of the recording.
        /// </summary>
        /// <value>
        /// The state of the recording.
        /// </value>
        RecordingState RecordingState { get; }
    }
}
