namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base infrastructure of a sound recording controller.
    /// </summary>
    public interface IRecordingController
    {
        /// <summary>
        /// Starts recording.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops recording.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the state of the recording.
        /// </summary>
        /// <value>
        /// The state of the recording.
        /// </value>
        RecordingState RecordingState { get; }
    }
}