﻿using System;
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
        /// <summary>
        /// The <see cref="Speakers.Monaural"/>.
        /// </summary>
        Monaural = 0b0000,

        /// <summary>
        /// The <see cref="Speakers.SideStereo"/>.
        /// </summary>
        OrdinalStereo = 0b0001,

        /// <summary>
        /// The <see cref="Speakers.FrontThree"/>.
        /// </summary>
        FrontThree = 0b0010,

        /// <summary>
        /// The <see cref="Speakers.Quad"/>.
        /// </summary>
        Quad = 0b0011,

        /// <summary>
        /// The <see cref="Speakers.FrontFive"/>.
        /// </summary>
        FrontFive = 0b0100,

        /// <summary>
        /// The <see cref="Speakers.FrontFivePointOne"/>.
        /// </summary>
        FivePointOne = 0b0101,

        /// <summary>
        /// The <see cref="Speakers.DolbySixPointOne"/>.
        /// </summary>
        DolbySixPointOne = 0b0110,

        /// <summary>
        /// The <see cref="Speakers.SevenPointOne"/>.
        /// </summary>
        SevenPointOne = 0b0111,

        /// <summary>
        /// The <see cref="Speakers.SideStereo"/> but with raw left and (left - right).
        /// </summary>
        LeftAndDifference = 0b1000,

        /// <summary>
        /// The <see cref="Speakers.SideStereo"/> but with (left - right) and raw right.
        /// </summary>
        RightAndDifference = 0b1001,

        /// <summary>
        /// The <see cref="Speakers.SideStereo"/> but with (left + right) >> 1 and (left - right).
        /// </summary>
        CenterAndDifference = 0b1010,

        /// <summary>
        /// The Reserved space with bit pattern 1011.
        /// </summary>
        Reserved1011 = 0b1011,

        /// <summary>
        /// The Reserved space with bit pattern 1100.
        /// </summary>
        Reserved1100 = 0b1100,

        /// <summary>
        /// The Reserved space with bit pattern 1101.
        /// </summary>
        Reserved1101 = 0b1101,

        /// <summary>
        /// The Reserved space with bit pattern 1110.
        /// </summary>
        Reserved1110 = 0b1110,

        /// <summary>
        /// The Reserved space with bit pattern 1111.
        /// </summary>
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
             FlacChannelAssignments.FrontThree => 3,
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
