using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Metadata
{
    /// <summary>
    /// Represents a picture type.
    /// </summary>
    public enum FlacPictureType : uint
    {
        /// <summary>
        /// Other
        /// </summary>
        Other = 0,

        /// <summary>
        /// 32x32 pixels 'file icon' (PNG only)
        /// </summary>
        FileIcon = 1,

        /// <summary>
        /// Other file icon
        /// </summary>
        OtherIcon = 2,

        /// <summary>
        /// Cover (front)
        /// </summary>
        FrontCover = 3,

        /// <summary>
        /// Cover (back)
        /// </summary>
        BackCover = 4,

        /// <summary>
        /// Leaflet page
        /// </summary>
        LeafletPage = 5,

        /// <summary>
        /// Media (e.g. label side of CD)
        /// </summary>
        Media = 6,

        /// <summary>
        /// Lead artist/lead performer/soloist
        /// </summary>
        LeadArtist = 7,

        /// <summary>
        /// Artist/performer
        /// </summary>
        Artist = 8,

        /// <summary>
        /// Conductor
        /// </summary>
        Conductor = 9,

        /// <summary>
        /// Band/Orchestra
        /// </summary>
        Band = 10,

        /// <summary>
        /// Composer
        /// </summary>
        Composer = 11,

        /// <summary>
        /// Lyricist/text writer
        /// </summary>
        Lyricist = 12,

        /// <summary>
        /// Recording Location
        /// </summary>
        RecordingLocation = 13,

        /// <summary>
        /// During recording
        /// </summary>
        DuringRecording = 14,

        /// <summary>
        /// During performance
        /// </summary>
        DuringPerformance = 15,

        /// <summary>
        /// Movie/video screen capture
        /// </summary>
        MovieScreenCapture = 16,

        /// <summary>
        /// A bright coloured fish
        /// </summary>
        BrightColouredFish = 17,

        /// <summary>
        /// Illustration
        /// </summary>
        Illustration = 18,

        /// <summary>
        /// Band/artist logotype
        /// </summary>
        ArtistLogotype = 19,

        /// <summary>
        /// Publisher/Studio logotype
        /// </summary>
        StudioLogotype = 20,
    }
}
