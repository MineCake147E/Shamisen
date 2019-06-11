using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure for all audio sources.
    /// </summary>
    public interface IAudioSource : IDisposable
    {
        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource"/> supports seeking or not.
        /// </summary>
        bool CanSeek { get; }

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        WaveFormat Format { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        long Position { get; set; }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource"/> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        long Length { get; }
    }
}
