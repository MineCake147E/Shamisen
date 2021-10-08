using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public enum FLAC__StreamDecoderReadStatus
    {
        FLAC__STREAM_DECODER_READ_STATUS_CONTINUE,
        FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM,
        FLAC__STREAM_DECODER_READ_STATUS_ABORT,
    }
}
