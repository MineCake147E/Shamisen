using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure for all audio sources.
    /// </summary>
    /// <typeparam name="TSample">The type of sample.</typeparam>
    /// <typeparam name="TFormat">The type of audio format.</typeparam>
    public interface IAudioSource<TSample, out TFormat> : IDisposable where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        [Obsolete("Moving to ISourceModifier!", true)]
        bool CanSeek { get; }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        TFormat Format { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// The implementation which doesn't support this property entirely(without depending on source) must have an <see cref="ObsoleteAttribute"/> and cause an compile-time error.
        /// </summary>
        [Obsolete("Moving to ISourceModifier!", true)]
        long Position { get; set; }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}"/> lasts in specific types.
        /// Negative value Means Infinity.
        /// </summary>
        [Obsolete("Moving to ISourceModifier!", true)]
        long Length { get; }
    }
}
