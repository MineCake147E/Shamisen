using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate int FLAC__IOCallback_Eof([NativeTypeName("FLAC__IOHandle")] void* handle);
}
