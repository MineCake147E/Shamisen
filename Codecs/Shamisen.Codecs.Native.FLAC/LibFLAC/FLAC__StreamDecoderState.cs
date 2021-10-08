using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public enum FLAC__StreamDecoderState
    {
        FLAC__STREAM_DECODER_SEARCH_FOR_METADATA = 0,
        FLAC__STREAM_DECODER_READ_METADATA,
        FLAC__STREAM_DECODER_SEARCH_FOR_FRAME_SYNC,
        FLAC__STREAM_DECODER_READ_FRAME,
        FLAC__STREAM_DECODER_END_OF_STREAM,
        FLAC__STREAM_DECODER_OGG_ERROR,
        FLAC__STREAM_DECODER_SEEK_ERROR,
        FLAC__STREAM_DECODER_ABORTED,
        FLAC__STREAM_DECODER_MEMORY_ALLOCATION_ERROR,
        FLAC__STREAM_DECODER_UNINITIALIZED,
    }
}
