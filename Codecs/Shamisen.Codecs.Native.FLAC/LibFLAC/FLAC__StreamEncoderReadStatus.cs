using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public enum FLAC__StreamEncoderReadStatus
    {
        FLAC__STREAM_ENCODER_READ_STATUS_CONTINUE,
        FLAC__STREAM_ENCODER_READ_STATUS_END_OF_STREAM,
        FLAC__STREAM_ENCODER_READ_STATUS_ABORT,
        FLAC__STREAM_ENCODER_READ_STATUS_UNSUPPORTED,
    }
}
