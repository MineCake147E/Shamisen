using System;

using System;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    public partial struct FLAC__IOCallbacks
    {
        [NativeTypeName("FLAC__IOCallback_Read")]
        public IntPtr read;

        [NativeTypeName("FLAC__IOCallback_Write")]
        public IntPtr write;

        [NativeTypeName("FLAC__IOCallback_Seek")]
        public IntPtr seek;

        [NativeTypeName("FLAC__IOCallback_Tell")]
        public IntPtr tell;

        [NativeTypeName("FLAC__IOCallback_Eof")]
        public IntPtr eof;

        [NativeTypeName("FLAC__IOCallback_Close")]
        public IntPtr close;
    }
}
