using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public partial struct FLAC__EntropyCodingMethod
    {
        public FLAC__EntropyCodingMethodType type;

        [NativeTypeName("union (anonymous union at flac/include/FLAC/format.h:257:2)")]
        public _data_e__Union data;

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _data_e__Union
        {
            [FieldOffset(0)]
            public FLAC__EntropyCodingMethod_PartitionedRice partitioned_rice;
        }
    }
}
