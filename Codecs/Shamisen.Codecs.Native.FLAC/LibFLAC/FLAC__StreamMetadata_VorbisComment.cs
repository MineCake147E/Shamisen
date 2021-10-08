using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamMetadata_VorbisComment
    {
        public FLAC__StreamMetadata_VorbisComment_Entry vendor_string;

        [NativeTypeName("FLAC__uint32")]
        public uint num_comments;

        public FLAC__StreamMetadata_VorbisComment_Entry* comments;
    }
}
