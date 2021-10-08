using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__EntropyCodingMethod_PartitionedRiceContents
    {
        [NativeTypeName("uint32_t *")]
        public uint* parameters;

        [NativeTypeName("uint32_t *")]
        public uint* raw_bits;

        [NativeTypeName("uint32_t")]
        public uint capacity_by_order;
    }
}
