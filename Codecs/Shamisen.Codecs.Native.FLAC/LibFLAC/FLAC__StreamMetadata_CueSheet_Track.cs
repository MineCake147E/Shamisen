using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamMetadata_CueSheet_Track
    {
        [NativeTypeName("FLAC__uint64")]
        public ulong offset;

        [NativeTypeName("FLAC__byte")]
        public byte number;

        [NativeTypeName("char [13]")]
        public fixed sbyte isrc[13];

        public uint _bitfield;

        [NativeTypeName("uint32_t : 1")]
        public uint type
        {
            get
            {
                return _bitfield & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~0x1u) | (value & 0x1u);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint pre_emphasis
        {
            get
            {
                return (_bitfield >> 1) & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1);
            }
        }

        [NativeTypeName("FLAC__byte")]
        public byte num_indices;

        public FLAC__StreamMetadata_CueSheet_Index* indices;
    }
}
