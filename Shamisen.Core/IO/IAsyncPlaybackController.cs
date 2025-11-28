using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base infrastructure of playback controller.
    /// </summary>
    public interface IAsyncPlaybackController : IPlaybackController
    {
        /// <summary>
        /// Starts the audio playback.
        /// </summary>
        ValueTask<bool> PlayAsync();

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        ValueTask<bool> PauseAsync();

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        ValueTask<bool> ResumeAsync();

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        ValueTask<bool> StopAsync();
    }
}
