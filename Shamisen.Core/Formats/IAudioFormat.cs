using System;
using System.Runtime.CompilerServices;

namespace Shamisen
{
    /// <summary>
    /// Defines a base structure of audio formats.<br/>
    /// <typeparamref name="TSample"/> must not be affected by the number of <see cref="Channels"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of sample.</typeparam>
    public interface IAudioFormat<TSample> : IEquatable<IAudioFormat<TSample>> where TSample : unmanaged
    {
        /// <summary>
        /// Gets the number of channels.<br/>
        /// It does not describe how these loudspeakers of each channels are placed in the room.<br/>
        /// It does not affect the type of <typeparamref name="TSample"/>.
        /// </summary>
        /// <value>
        /// The number of channels.
        /// </value>
        int Channels { get; }

        /// <summary>
        /// Gets the average number of samples contained in one second.<br/>
        ///
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        int SampleRate { get; }

        /// <summary>
        /// Gets the number indicates how many bits are consumed per every single 1-channel sample.<br/>
        /// Does not depend on the number of <see cref="Channels"/>.<br/>
        /// -1 means the bit depth is variable in the whole stream.
        /// </summary>
        /// <value>
        /// The bit depth.
        /// </value>
        int BitDepth { get; }
    }
}
