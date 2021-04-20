using System;

namespace Shamisen
{
    /// <summary>
    /// Represents a mask of channels' combination.
    /// </summary>
    [Flags]
    public enum Speakers : uint
    {
        #region Standard Masks

        /// <summary>
        /// Invalid.
        /// </summary>
        None = 0,

        /// <summary>
        /// The speaker placed in left front of the listener.
        /// </summary>
        FrontLeft = 0b1,

        /// <summary>
        /// The speaker placed in right front of the listener.
        /// </summary>
        FrontRight = 0b10,

        /// <summary>
        /// The speaker placed in front of the listener.
        /// </summary>
        FrontCenter = 0b100,

        /// <summary>
        /// The subwoofer speaker placed in front of the listener.
        /// </summary>
        FrontCenterLowFrequency = 0b1000,

        /// <summary>
        /// The speaker placed in left rear of the listener.
        /// </summary>
        RearLeft = 0b1_0000,

        /// <summary>
        /// The speaker placed in right rear of the listener.
        /// </summary>
        RearRight = 0b10_0000,

        /// <summary>
        /// The speaker placed between <see cref="FrontCenter"/> and <see cref="FrontLeft"/>, of the listener.
        /// </summary>
        FrontLeftOfCenter = 0b100_0000,

        /// <summary>
        /// The speaker placed between <see cref="FrontCenter"/> and <see cref="FrontRight"/>, of the listener.
        /// </summary>
        FrontRightOfCenter = 0b1000_0000,

        /// <summary>
        /// The speaker placed behind the listener.
        /// </summary>
        RearCenter = 0b1_0000_0000,

        /// <summary>
        /// The speaker placed to the left of the listener.
        /// </summary>
        SideLeft = 0b10_0000_0000,

        /// <summary>
        /// The speaker placed to the right of the listener.
        /// </summary>
        SideRight = 0b100_0000_0000,

        /// <summary>
        /// The subwoofer speaker placed behind the listener.
        /// </summary>
        RearLowFrequency = 0b1000_0000_0000,

        /// <summary>
        /// The speaker placed at the upper left front of the listener.
        /// </summary>
        TopFrontLeft = 0b1_0000_0000_0000,

        /// <summary>
        /// The speaker placed at the upper front of the listener.
        /// </summary>
        TopFrontCenter = 0b10_0000_0000_0000,

        /// <summary>
        /// The speaker placed at the upper right front of the listener.
        /// </summary>
        TopFrontRight = 0b100_0000_0000_0000,

        /// <summary>
        /// The speaker placed at the upper left rear of the listener.
        /// </summary>
        TopRearLeft = 0b1000_0000_0000_0000,

        /// <summary>
        /// The speaker placed at the upper rear of the listener.
        /// </summary>
        TopRearCenter = 0b1_0000_0000_0000_0000,

        /// <summary>
        /// The speaker placed at the upper right rear of the listener.
        /// </summary>
        TopRearRight = 0b10_0000_0000_0000_0000,

        /// <summary>
        /// The speaker placed at the upper left of the listener.
        /// </summary>
        TopSideLeft = 0b100_0000_0000_0000_0000,

        /// <summary>
        /// The speaker placed above the listener.
        /// </summary>
        TopSideCenter = 0b1000_0000_0000_0000_0000,

        /// <summary>
        /// The speaker placed at the upper right of the listener.
        /// </summary>
        TopSideRight = 0b1_0000_0000_0000_0000_0000,

        #endregion Standard Masks

        #region Pre-Combined masks

        /// <summary>
        /// The representation of single speaker.
        /// </summary>
        Monaural = FrontCenter,

        /// <summary>
        /// The ordinal Stereo combination.
        /// </summary>
        FrontStereo = FrontLeft | FrontRight,

        /// <summary>
        /// The side Stereo combination.
        /// </summary>
        SideStereo = SideLeft | SideRight,

        /// <summary>
        /// The rear Stereo combination.
        /// </summary>
        RearStereo = RearLeft | RearRight,

        /// <summary>
        /// The top front Stereo combination.
        /// </summary>
        TopFrontStereo = TopFrontLeft | TopFrontRight,

        /// <summary>
        /// The top side Stereo combination.
        /// </summary>
        TopSideStereo = TopSideLeft | TopSideRight,

        /// <summary>
        /// The top rear Stereo combination.
        /// </summary>
        TopRearStereo = TopRearLeft | TopRearRight,

        /// <summary>
        /// The 2.1ch combination.
        /// </summary>
        TwoPointOne = FrontStereo | FrontCenterLowFrequency,

        /// <summary>
        /// The 3ch surround combination.
        /// </summary>
        FrontThree = FrontStereo | RearCenter,

        /// <summary>
        /// The 3.1ch combination.
        /// </summary>
        ThreePointOne = FrontStereo | FrontCenter | FrontCenterLowFrequency,

        /// <summary>
        /// The 4ch surround combination.
        /// </summary>
        Quad = FrontStereo | RearStereo,

        /// <summary>
        /// The 5ch surround combination.
        /// </summary>
        FrontFive = FrontStereo | FrontCenter | RearStereo,

        /// <summary>
        /// The 5ch surround combination.
        /// </summary>
        SideFive = FrontStereo | FrontCenter | SideStereo,

        /// <summary>
        /// The 4.1ch combination.
        /// </summary>
        FourPointOne = FrontStereo | FrontCenterLowFrequency | RearLeft | RearRight,

        /// <summary>
        /// The standard 5.1ch combination.
        /// </summary>
        FrontFivePointOne = ThreePointOne | RearStereo,

        /// <summary>
        /// The side 5.1ch combination.
        /// </summary>
        SideFivePointOne = ThreePointOne | SideStereo,

        /// <summary>
        /// The hexagonal 6ch combination.
        /// </summary>
        Hexagonal = FrontStereo | FrontCenter | RearStereo | RearCenter,

        /// <summary>
        /// The 6.1ch combination with top-side center.
        /// </summary>
        ShamisenSixPointOne = FrontFivePointOne | TopSideCenter,

        /// <summary>
        /// The 6.1ch combination with rear center.
        /// </summary>
        DolbySixPointOne = FrontFivePointOne | RearCenter,

        /// <summary>
        /// The 7ch surround combination.
        /// </summary>
        SevenSurround = FrontFive | SideStereo,

        /// <summary>
        /// The 7.1ch combination.
        /// </summary>
        SevenPointOne = FrontFivePointOne | SideLeft | SideRight,

        #endregion Pre-Combined masks
    }

    /// <summary>
    /// Contains some utility functions for <see cref="Speakers"/>.
    /// </summary>
    public static class SpeakersUtils
    {
        /// <summary>
        /// Counts the number of channels of specified <paramref name="speakers"/>.
        /// </summary>
        /// <param name="speakers">The speakers.</param>
        /// <returns></returns>
        public static int CountChannels(this Speakers speakers) => MathI.PopCount((uint)speakers);
    }
}
