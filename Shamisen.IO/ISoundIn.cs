using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Formats;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base infrastructure of a sound input.
    /// </summary>
    public interface ISoundIn : IRecordingController, IDisposable
    {
        /// <summary>
        /// Raised when the recording data has been available.
        /// </summary>
        event DataAvailableEventHandler? DataAvailable;

        /// <summary>
        /// Raised when the recording has been stopped.
        /// </summary>
        event EventHandler<StoppedEventArgs>? Stopped;

        /// <summary>
        /// Gets the <see cref="IWaveFormat"/> of this <see cref="ISoundIn"/>.
        /// </summary>
        IWaveFormat Format { get; }
    }
}
