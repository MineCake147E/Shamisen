using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Represents a format of wave data.
    /// </summary>
    public class WaveFormat
    {
        /// <summary>
        /// Gets the audio encoding.
        /// </summary>
        /// <value>
        /// The audio encoding.
        /// </value>
        public uint AudioEncoding { get; }

        /// <summary>
        /// Gets the channels.
        /// </summary>
        /// <value>
        /// The channels.
        /// </value>
        public short Channels { get; }

        /// <summary>
        /// Gets the sample rate[Samples/s].
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        public int SampleRate { get; }

        /// <summary>
        /// Gets the byte rate[Bytes/s].
        /// </summary>
        /// <value>
        /// The byte rate.
        /// </value>
        public int ByteRate { get; }

        /// <summary>
        /// Gets the block align.
        /// </summary>
        /// <value>
        /// The block align.
        /// </value>
        public short BlockAlign { get; }

        /// <summary>
        /// Gets the bit rate.
        /// </summary>
        /// <value>
        /// The bit rate.
        /// </value>
        public short BitRate { get; }

        /// <summary>
        /// Gets the length of the footer.
        /// </summary>
        /// <value>
        /// The length of the footer.
        /// </value>
        public short FooterLength { get; }
    }
}
