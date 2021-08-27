using System;

using Shamisen.Formats;

namespace Shamisen
{
    /// <summary>
    /// Provides several utilities for manipulating <see cref="Speakers"/>.
    /// </summary>
    public static class ChannelMaskUtils
    {
        /// <summary>
        /// Gets the value which indicates how the speakers are used.
        /// </summary>
        /// <typeparam name="TSample">The type of sample.</typeparam>
        /// <param name="format">The format.</param>
        /// <returns>The matching <see cref="Speakers"/> combination.</returns>
        public static Speakers GetChannelMasks<TSample>(this IAudioFormat<TSample> format)
            where TSample : unmanaged
            => format is IChannelMaskedFormat cmFormat
                ? cmFormat.ChannelCombination
                : format.Channels switch
                {
                    //Ordinal Monaural
                    1 => Speakers.Monaural,
                    //Assuming Speaker Stereo
                    2 => Speakers.FrontStereo,
                    //Assuming 2.1ch Stereo
                    3 => Speakers.FrontStereo | Speakers.FrontCenterLowFrequency,
                    //Assuming 3.1ch Stereo
                    4 => Speakers.ThreePointOne,
                    //Assuming 4.1ch Surround
                    5 => Speakers.FrontStereo | Speakers.RearLeft | Speakers.RearRight | Speakers.FrontCenterLowFrequency,
                    //Assuming 5.1ch Surround
                    6 => Speakers.FrontFivePointOne,
                    //Assuming 6.1ch Surround
                    7 => Speakers.FrontStereo | Speakers.SideStereo | Speakers.RearStereo | Speakers.FrontCenterLowFrequency,
                    //Assuming 7.1ch Surround
                    8 => Speakers.SevenPointOne,
                    //Assuming 8.1ch Surround
                    9 => Speakers.SevenPointOne | Speakers.RearCenter,
                    //Assuming 7.1.2ch Surround+
                    10 => Speakers.SevenPointOne | Speakers.TopSideStereo,
                    _ => throw new NotSupportedException($"The given number of channels ({format.Channels}) is not supported!"),
                };
    }
}
