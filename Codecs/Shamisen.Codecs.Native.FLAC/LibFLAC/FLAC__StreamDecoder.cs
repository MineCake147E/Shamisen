using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__StreamDecoder
    {
        [NativeTypeName("struct FLAC__StreamDecoderProtected *")]
        public FLAC__StreamDecoderProtected* protected_;

        [NativeTypeName("struct FLAC__StreamDecoderPrivate *")]
        public FLAC__StreamDecoderPrivate* private_;
    }
}
