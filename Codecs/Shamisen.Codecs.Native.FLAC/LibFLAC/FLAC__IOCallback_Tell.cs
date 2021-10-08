using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: NativeTypeName("FLAC__int64")]
    public unsafe delegate long FLAC__IOCallback_Tell([NativeTypeName("FLAC__IOHandle")] void* handle);
}
