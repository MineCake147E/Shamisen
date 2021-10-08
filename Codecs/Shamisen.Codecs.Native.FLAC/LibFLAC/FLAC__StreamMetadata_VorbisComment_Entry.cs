using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamMetadata_VorbisComment_Entry
    {
        [NativeTypeName("FLAC__uint32")]
        public uint length;

        [NativeTypeName("FLAC__byte *")]
        public byte* entry;
    }
}
