using System;

using Shamisen.Formats;

namespace Shamisen
{
    /// <summary>
    /// Provides several utilities for manipulating <see cref="StandardSpeakerChannels"/>.
    /// </summary>
    public static class ChannelMaskUtils
    {
        /// <summary>
        /// Gets the value which indicates how the speakers are used.
        /// </summary>
        /// <typeparam name="TSample">The type of sample.</typeparam>
        /// <param name="format">The format.</param>
        /// <returns>The matching <see cref="StandardSpeakerChannels"/> combination.</returns>
        public static StandardSpeakerChannels GetChannelMasks<TSample>(this IAudioFormat<TSample> format)
            where TSample : unmanaged
            => format is IChannelMaskedFormat cmFormat
                ? cmFormat.ChannelCombination
                : format.Channels switch
                {
                    //Ordinal Monaural
                    1 => StandardSpeakerChannels.Monaural,
                    //Assuming Speaker Stereo
                    2 => StandardSpeakerChannels.FrontStereo,
                    //Assuming 3ch Stereo
                    3 => StandardSpeakerChannels.FrontThree,
                    //Assuming 4ch Surround
                    4 => StandardSpeakerChannels.Quad,
                    //Assuming 5ch Surround
                    5 => StandardSpeakerChannels.FrontFive,
                    //Assuming 5.1ch Surround
                    6 => StandardSpeakerChannels.FrontFivePointOne,
                    //Assuming 6.1ch Surround
                    7 => StandardSpeakerChannels.FrontFivePointOne | StandardSpeakerChannels.RearCenter,
                    //Assuming 7.1ch Surround
                    8 => StandardSpeakerChannels.SevenPointOne,
                    //Assuming 8.1ch Surround
                    9 => StandardSpeakerChannels.SevenPointOne | StandardSpeakerChannels.RearCenter,
                    //Assuming 7.1.2ch Surround+
                    10 => StandardSpeakerChannels.SevenPointOne | StandardSpeakerChannels.TopSideStereo,
                    _ => throw new NotSupportedException($"The given number of channels ({format.Channels}) is not supported!"),
                };
    }
}
