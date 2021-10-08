using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamMetadata_Unknown
    {
        [NativeTypeName("FLAC__byte *")]
        public byte* data;
    }
}
