using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public enum FLAC__StreamDecoderErrorStatus
    {
        FLAC__STREAM_DECODER_ERROR_STATUS_LOST_SYNC,
        FLAC__STREAM_DECODER_ERROR_STATUS_BAD_HEADER,
        FLAC__STREAM_DECODER_ERROR_STATUS_FRAME_CRC_MISMATCH,
        FLAC__STREAM_DECODER_ERROR_STATUS_UNPARSEABLE_STREAM,
    }
}