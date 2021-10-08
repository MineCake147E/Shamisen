using System;

using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: NativeTypeName("size_t")]
    public unsafe delegate UIntPtr FLAC__IOCallback_Read(void* ptr, [NativeTypeName("size_t")] UIntPtr size, [NativeTypeName("size_t")] UIntPtr nmemb, [NativeTypeName("FLAC__IOHandle")] void* handle);
}
