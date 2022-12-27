using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Formats;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base infrastructure of a sound input.
    /// </summary>
    public interface ISoundIn : IRecordingController, ISoundInterface
    {
        /// <summary>
        /// Raised when the recording data has been available.
        /// </summary>
#pragma warning disable RCS1159 // FALSE POSITIVE: Use EventHandler<T>. See https://github.com/JosefPihrt/Roslynator/issues/1019
        event DataAvailableEventHandler? DataAvailable;
#pragma warning restore RCS1159

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
