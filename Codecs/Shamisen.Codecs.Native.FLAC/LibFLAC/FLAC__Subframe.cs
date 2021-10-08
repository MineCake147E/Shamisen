using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public partial struct FLAC__Subframe
    {
        public FLAC__SubframeType type;

        [NativeTypeName("union (anonymous union at flac/include/FLAC/format.h:346:2)")]
        public _data_e__Union data;

        [NativeTypeName("uint32_t")]
        public uint wasted_bits;

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _data_e__Union
        {
            [FieldOffset(0)]
            public FLAC__Subframe_Constant constant;

            [FieldOffset(0)]
            public FLAC__Subframe_Fixed @fixed;

            [FieldOffset(0)]
            public FLAC__Subframe_LPC lpc;

            [FieldOffset(0)]
            public FLAC__Subframe_Verbatim verbatim;
        }
    }
}
