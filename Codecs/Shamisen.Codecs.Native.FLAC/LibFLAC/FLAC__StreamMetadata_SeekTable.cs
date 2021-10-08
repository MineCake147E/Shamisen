using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamMetadata_SeekTable
    {
        [NativeTypeName("uint32_t")]
        public uint num_points;

        public FLAC__StreamMetadata_SeekPoint* points;
    }
}
