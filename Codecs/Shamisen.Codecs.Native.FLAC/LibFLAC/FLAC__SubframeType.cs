using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public enum FLAC__SubframeType
    {
        FLAC__SUBFRAME_TYPE_CONSTANT = 0,
        FLAC__SUBFRAME_TYPE_VERBATIM = 1,
        FLAC__SUBFRAME_TYPE_FIXED = 2,
        FLAC__SUBFRAME_TYPE_LPC = 3,
    }
}
