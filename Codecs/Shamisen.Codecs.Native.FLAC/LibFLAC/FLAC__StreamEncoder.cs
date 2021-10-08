using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamEncoder
    {
        [NativeTypeName("struct FLAC__StreamEncoderProtected *")]
        public FLAC__StreamEncoderProtected* protected_;

        [NativeTypeName("struct FLAC__StreamEncoderPrivate *")]
        public FLAC__StreamEncoderPrivate* private_;
    }
}
