using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamMetadata_Application
    {
        [NativeTypeName("FLAC__byte [4]")]
        public fixed byte id[4];

        [NativeTypeName("FLAC__byte *")]
        public byte* data;
    }
}
