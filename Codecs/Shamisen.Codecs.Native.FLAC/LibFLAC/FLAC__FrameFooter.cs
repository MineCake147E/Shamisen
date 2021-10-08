using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public partial struct FLAC__FrameFooter
    {
        [NativeTypeName("FLAC__uint16")]
        public ushort crc;
    }
}
