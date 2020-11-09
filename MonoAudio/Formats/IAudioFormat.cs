using System;
using System.Runtime.CompilerServices;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base structure of audio formats.<br/>
    /// <typeparamref name="TSample"/> must not be affected by the number of <see cref="Channels"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of sample.</typeparam>
    public interface IAudioFormat<TSample> : IEquatable<IAudioFormat<TSample>>
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
        /// </summary>
        /// <value>
        /// The bit depth.
        /// </value>
        int BitDepth { get; }

        /// <summary>
        /// Gets the value indicates how many <typeparamref name="TSample"/>s are required per whole frame.<br/>
        /// It depends on <see cref="Channels"/>.
        /// </summary>
        /// <value>
        /// The size of frame.
        /// </value>
        int BlockSize { [MethodImpl(MethodImplOptions.AggressiveInlining)]get; }

        /// <summary>
        /// Gets the value indicates how many <typeparamref name="TSample"/>s are required per 1-channel sample.<br/>
        /// Does not depend on the number of <see cref="Channels"/>.<br/>
        /// </summary>
        /// <value>
        /// The size of a sample in <typeparamref name="TSample"/>s.
        /// </value>
        int SampleSize { get; }
    }
}
