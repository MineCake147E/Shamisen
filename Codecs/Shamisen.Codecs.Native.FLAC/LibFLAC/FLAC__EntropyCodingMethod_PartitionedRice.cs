using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__EntropyCodingMethod_PartitionedRice
    {
        [NativeTypeName("uint32_t")]
        public uint order;

        [NativeTypeName("const FLAC__EntropyCodingMethod_PartitionedRiceContents *")]
        public FLAC__EntropyCodingMethod_PartitionedRiceContents* contents;
    }
}
