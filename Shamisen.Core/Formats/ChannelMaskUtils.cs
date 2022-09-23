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
                    //Assuming 2.1ch Stereo
                    3 => StandardSpeakerChannels.FrontStereo | StandardSpeakerChannels.FrontCenterLowFrequency,
                    //Assuming 3.1ch Stereo
                    4 => StandardSpeakerChannels.ThreePointOne,
                    //Assuming 4.1ch Surround
                    5 => StandardSpeakerChannels.FrontStereo | StandardSpeakerChannels.RearLeft | StandardSpeakerChannels.RearRight | StandardSpeakerChannels.FrontCenterLowFrequency,
                    //Assuming 5.1ch Surround
                    6 => StandardSpeakerChannels.FrontFivePointOne,
                    //Assuming 6.1ch Surround
                    7 => StandardSpeakerChannels.FrontStereo | StandardSpeakerChannels.SideStereo | StandardSpeakerChannels.RearStereo | StandardSpeakerChannels.FrontCenterLowFrequency,
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
