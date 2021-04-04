using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Represents a channel assignment of FLAC file.
    /// </summary>
    public enum FlacChannelAssignments : byte
    {
        Monaural = 0b0000,
        OrdinalStereo = 0b0001,
        ThreePointOne = 0b0010,
        Quad = 0b0011,
        FrontFive = 0b0100,
        FivePointOne = 0b0101,
        DolbySixPointOne = 0b0110,
        SevenPointOne = 0b0111,
        LeftAndDifference = 0b1000,
        RightAndDifference = 0b1001,
        CenterAndDifference = 0b1010,
        Reserved1011 = 0b1011,
        Reserved1100 = 0b1100,
        Reserved1101 = 0b1101,
        Reserved1110 = 0b1110,
        Reserved1111 = 0b1111,
    }
    /// <summary>
    /// Contains some utility functions for <see cref="FlacChannelAssignments"/>.
    /// </summary>
    public static class FlacChannelAssignmentsUtils
    {
        /// <summary>
        /// Gets the number of channels for specified <paramref name="assignments"/>.
        /// </summary>
        /// <param name="assignments">The <see cref="FlacChannelAssignments"/> to calculate the count of channels.</param>
        /// <returns></returns>
        public static int GetChannels(this FlacChannelAssignments assignments)
         => assignments switch
         {
             FlacChannelAssignments.Monaural => 1,
             FlacChannelAssignments.OrdinalStereo => 2,
             FlacChannelAssignments.ThreePointOne => 3,
             FlacChannelAssignments.Quad => 4,
             FlacChannelAssignments.FrontFive => 5,
             FlacChannelAssignments.FivePointOne => 6,
             FlacChannelAssignments.DolbySixPointOne => 7,
             FlacChannelAssignments.SevenPointOne => 8,
             FlacChannelAssignments.LeftAndDifference => 2,
             FlacChannelAssignments.RightAndDifference => 2,
             FlacChannelAssignments.CenterAndDifference => 2,
             _ => 0,
         };
    }
}
