using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamMetadata_Picture
    {
        public FLAC__StreamMetadata_Picture_Type type;

        [NativeTypeName("char *")]
        public sbyte* mime_type;

        [NativeTypeName("FLAC__byte *")]
        public byte* description;

        [NativeTypeName("FLAC__uint32")]
        public uint width;

        [NativeTypeName("FLAC__uint32")]
        public uint height;

        [NativeTypeName("FLAC__uint32")]
        public uint depth;

        [NativeTypeName("FLAC__uint32")]
        public uint colors;

        [NativeTypeName("FLAC__uint32")]
        public uint data_length;

        [NativeTypeName("FLAC__byte *")]
        public byte* data;
    }
}
