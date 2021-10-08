using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__Subframe_LPC
    {
        public FLAC__EntropyCodingMethod entropy_coding_method;

        [NativeTypeName("uint32_t")]
        public uint order;

        [NativeTypeName("uint32_t")]
        public uint qlp_coeff_precision;

        public int quantization_level;

        [NativeTypeName("FLAC__int32 [32]")]
        public fixed int qlp_coeff[32];

        [NativeTypeName("FLAC__int32 [32]")]
        public fixed int warmup[32];

        [NativeTypeName("const FLAC__int32 *")]
        public int* residual;
    }
}
