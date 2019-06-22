using System;

namespace MonoAudio.Formats
{
    /// <summary>
    /// Defines a base structure of audio formats.
    /// </summary>
    /// <typeparam name="TSample">The type of sample.</typeparam>
    public interface IAudioFormat<TSample> : IEquatable<IAudioFormat<TSample>>
    {
        /// <summary>
        /// Gets the number of channels.
        /// It does not describe how these loudspeakers of each channels are placed in the room.
        /// </summary>
        /// <value>
        /// The number of channels.
        /// </value>
        int Channels { get; }

        /// <summary>
        /// Gets the number indicates how many times the audio signal is sampled.
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        int SampleRate { get; }

        /// <summary>
        /// Gets the number indicates how many bits are consumed per every single 1-channel sample.
        /// Does not depend on the number of <see cref="Channels"/>.
        /// </summary>
        /// <value>
        /// The bit depth.
        /// </value>
        int BitDepth { get; }
    }
}
