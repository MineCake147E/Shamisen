using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public partial struct FLAC__Frame
    {
        public FLAC__FrameHeader header;

        [NativeTypeName("FLAC__Subframe [8]")]
        public _subframes_e__FixedBuffer subframes;

        public FLAC__FrameFooter footer;

        public partial struct _subframes_e__FixedBuffer
        {
            public FLAC__Subframe e0;
            public FLAC__Subframe e1;
            public FLAC__Subframe e2;
            public FLAC__Subframe e3;
            public FLAC__Subframe e4;
            public FLAC__Subframe e5;
            public FLAC__Subframe e6;
            public FLAC__Subframe e7;

            public unsafe ref FLAC__Subframe this[int index]
            {
                get
                {
                    fixed (FLAC__Subframe* pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }
}
