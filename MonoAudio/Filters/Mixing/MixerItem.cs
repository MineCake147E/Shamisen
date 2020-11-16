using System;
using System.Collections.Generic;
using System.Text;

using MonoAudio.Filters.Mixing.Advanced;

namespace MonoAudio.Filters.Mixing
{
    /// <summary>
    /// Represents an item of <see cref="SimpleMixer"/> and <see cref="AdvancedMixer"/>.
    /// </summary>
    public sealed class MixerItem
    {
        /// <summary>
        /// Gets the source of this <see cref="MixerItem"/>.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public ISampleSource Source { get; }

        /// <summary>
        /// Gets the volume of this <see cref="MixerItem"/>.
        /// </summary>
        /// <value>
        /// The volume.
        /// </value>
        public float Volume { get; set; }

        /// <summary>
        /// Gets or sets the input buffer for this <see cref="MixerItem"/>.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        public AudioMemory<float, SampleFormat> Buffer { get; internal set; }
    }
}
