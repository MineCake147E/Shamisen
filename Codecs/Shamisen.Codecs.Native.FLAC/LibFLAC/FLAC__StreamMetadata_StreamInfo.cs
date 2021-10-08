using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamMetadata_StreamInfo
    {
        [NativeTypeName("uint32_t")]
        public uint min_blocksize;

        [NativeTypeName("uint32_t")]
        public uint max_blocksize;

        [NativeTypeName("uint32_t")]
        public uint min_framesize;

        [NativeTypeName("uint32_t")]
        public uint max_framesize;

        [NativeTypeName("uint32_t")]
        public uint sample_rate;

        [NativeTypeName("uint32_t")]
        public uint channels;

        [NativeTypeName("uint32_t")]
        public uint bits_per_sample;

        [NativeTypeName("FLAC__uint64")]
        public ulong total_samples;

        [NativeTypeName("FLAC__byte [16]")]
        public fixed byte md5sum[16];
    }
}
