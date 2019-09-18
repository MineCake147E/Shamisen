using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Formats
{
    /// <summary>
    /// Defines a base structure of an <see cref="IAudioFormat{TSample}"/> that has an information of channel masks.
    /// </summary>
    public interface IChannelMaskedFormat
    {
        /// <summary>
        /// Gets the value which indicates how the speakers are used.
        /// </summary>
        Speakers ChannelCombination { get; }
    }
}
