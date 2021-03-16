namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Represents a FLAC metadata block's type.
    /// </summary>
    public enum FlacMetadataBlockType : byte
    {
        /// <summary>
        /// The stream information.
        /// </summary>
        StreamInfo = 0,

        /// <summary>
        /// The padding.
        /// </summary>
        Padding = 1,

        /// <summary>
        /// The application specific information.
        /// </summary>
        Application = 2,

        /// <summary>
        /// The seek table.
        /// </summary>
        SeekTable = 3,

        /// <summary>
        /// The comment.
        /// </summary>
        VorbisComment = 4,

        /// <summary>
        /// The cue sheet.
        /// </summary>
        CueSheet = 5,

        /// <summary>
        /// The picture.
        /// </summary>
        Picture = 6,

        /// <summary>
        /// The invalid value.
        /// </summary>
        Invalid = 127
    }
}
