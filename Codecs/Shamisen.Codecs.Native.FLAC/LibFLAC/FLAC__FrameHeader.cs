using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public partial struct FLAC__FrameHeader
    {
        [NativeTypeName("uint32_t")]
        public uint blocksize;

        [NativeTypeName("uint32_t")]
        public uint sample_rate;

        [NativeTypeName("uint32_t")]
        public uint channels;

        public FLAC__ChannelAssignment channel_assignment;

        [NativeTypeName("uint32_t")]
        public uint bits_per_sample;

        public FLAC__FrameNumberType number_type;

        [NativeTypeName("union (anonymous union at flac/include/FLAC/format.h:432:2)")]
        public _number_e__Union number;

        [NativeTypeName("FLAC__uint8")]
        public byte crc;

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _number_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("FLAC__uint32")]
            public uint frame_number;

            [FieldOffset(0)]
            [NativeTypeName("FLAC__uint64")]
            public ulong sample_number;
        }
    }
}
