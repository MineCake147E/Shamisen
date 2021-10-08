using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public unsafe partial struct FLAC__Subframe_Verbatim
    {
        [NativeTypeName("const FLAC__int32 *")]
        public int* data;
    }
}
