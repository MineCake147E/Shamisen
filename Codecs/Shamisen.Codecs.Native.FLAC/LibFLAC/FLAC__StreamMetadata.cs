using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public partial struct FLAC__StreamMetadata
    {
        public FLAC__MetadataType type;

        [NativeTypeName("FLAC__bool")]
        public int is_last;

        [NativeTypeName("uint32_t")]
        public uint length;

        [NativeTypeName("union (anonymous union at flac/include/FLAC/format.h:846:2)")]
        public _data_e__Union data;

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _data_e__Union
        {
            [FieldOffset(0)]
            public FLAC__StreamMetadata_StreamInfo stream_info;

            [FieldOffset(0)]
            public FLAC__StreamMetadata_Padding padding;

            [FieldOffset(0)]
            public FLAC__StreamMetadata_Application application;

            [FieldOffset(0)]
            public FLAC__StreamMetadata_SeekTable seek_table;

            [FieldOffset(0)]
            public FLAC__StreamMetadata_VorbisComment vorbis_comment;

            [FieldOffset(0)]
            public FLAC__StreamMetadata_CueSheet cue_sheet;

            [FieldOffset(0)]
            public FLAC__StreamMetadata_Picture picture;

            [FieldOffset(0)]
            public FLAC__StreamMetadata_Unknown unknown;
        }
    }
}
