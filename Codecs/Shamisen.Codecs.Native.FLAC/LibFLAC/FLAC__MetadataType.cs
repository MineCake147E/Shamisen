using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public enum FLAC__MetadataType
    {
        FLAC__METADATA_TYPE_STREAMINFO = 0,
        FLAC__METADATA_TYPE_PADDING = 1,
        FLAC__METADATA_TYPE_APPLICATION = 2,
        FLAC__METADATA_TYPE_SEEKTABLE = 3,
        FLAC__METADATA_TYPE_VORBIS_COMMENT = 4,
        FLAC__METADATA_TYPE_CUESHEET = 5,
        FLAC__METADATA_TYPE_PICTURE = 6,
        FLAC__METADATA_TYPE_UNDEFINED = 7,
        FLAC__MAX_METADATA_TYPE = (126U),
    }
}
