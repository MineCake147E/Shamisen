using System;
using System.Collections.Generic;
using System.Text;

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
        {
            if (format is IChannelMaskedFormat cmFormat)
            {
                return cmFormat.ChannelCombination;
            }
            else
            {
                switch (format.Channels)
                {
                    case 1: //Ordinal Monaural
                        return Speakers.Monaural;
                    case 2: //Assuming Speaker Stereo
                        return Speakers.FrontStereo;
                    case 3: //Assuming 2.1ch Stereo
                        return Speakers.FrontStereo | Speakers.FrontLowFrequency;
                    case 4: //Assuming 3.1ch Stereo
                        return Speakers.ThreePointOne;
                    case 5: //Assuming 4.1ch Surround
                        return Speakers.FrontStereo | Speakers.RearLeft | Speakers.RearRight | Speakers.FrontLowFrequency;
                    case 6: //Assuming 5.1ch Surround
                        return Speakers.FivePointOne;
                    case 7: //Assuming 6.1ch Surround
                        return Speakers.FrontStereo | Speakers.SideStereo | Speakers.RearStereo | Speakers.FrontLowFrequency;
                    case 8: //Assuming 7.1ch Surround
                        return Speakers.SevenPointOne;
                    case 9: //Assuming 8.1ch Surround
                        return Speakers.SevenPointOne | Speakers.RearCenter;
                    case 10: //Assuming 7.1.2ch Surround+
                        return Speakers.SevenPointOne | Speakers.TopSideStereo;
                    default:
                        throw new NotSupportedException($"The given number of channels ({format.Channels}) is not supported!");
                }
            }
        }
    }
}
