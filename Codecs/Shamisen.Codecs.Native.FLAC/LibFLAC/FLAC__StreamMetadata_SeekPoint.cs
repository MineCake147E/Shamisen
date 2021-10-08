using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public partial struct FLAC__StreamMetadata_SeekPoint
    {
        [NativeTypeName("FLAC__uint64")]
        public ulong sample_number;

        [NativeTypeName("FLAC__uint64")]
        public ulong stream_offset;

        [NativeTypeName("uint32_t")]
        public uint frame_samples;
    }
}
