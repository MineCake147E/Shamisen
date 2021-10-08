using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamMetadata_CueSheet
    {
        [NativeTypeName("char [129]")]
        public fixed sbyte media_catalog_number[129];

        [NativeTypeName("FLAC__uint64")]
        public ulong lead_in;

        [NativeTypeName("FLAC__bool")]
        public int is_cd;

        [NativeTypeName("uint32_t")]
        public uint num_tracks;

        public FLAC__StreamMetadata_CueSheet_Track* tracks;
    }
}
