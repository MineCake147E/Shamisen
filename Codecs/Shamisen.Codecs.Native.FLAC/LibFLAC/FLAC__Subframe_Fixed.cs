using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__Subframe_Fixed
    {
        public FLAC__EntropyCodingMethod entropy_coding_method;

        [NativeTypeName("uint32_t")]
        public uint order;

        [NativeTypeName("FLAC__int32 [4]")]
        public fixed int warmup[4];

        [NativeTypeName("const FLAC__int32 *")]
        public int* residual;
    }
}
