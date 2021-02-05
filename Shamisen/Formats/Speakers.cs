using System;

namespace Shamisen
{
    /// <summary>
    /// Represents a mask of channels' combination.
    /// </summary>
    [Flags]
    public enum Speakers : uint
    {
        #region Masks

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
        FrontLowFrequency = 0b1000,

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

        #endregion Masks

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
        /// The 3.1ch combination.
        /// </summary>
        ThreePointOne = FrontStereo | FrontCenter | FrontLowFrequency,

        /// <summary>
        /// The 5.1ch combination.
        /// </summary>
        FivePointOne = ThreePointOne | RearLeft | RearRight,

        /// <summary>
        /// The 7.1ch combination.
        /// </summary>
        SevenPointOne = FivePointOne | SideLeft | SideRight,

        #endregion Pre-Combined masks
    }
}
