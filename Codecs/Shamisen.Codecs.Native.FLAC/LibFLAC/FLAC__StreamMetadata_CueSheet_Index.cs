using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public partial struct FLAC__StreamMetadata_CueSheet_Index
    {
        [NativeTypeName("FLAC__uint64")]
        public ulong offset;

        [NativeTypeName("FLAC__byte")]
        public byte number;
    }
}
