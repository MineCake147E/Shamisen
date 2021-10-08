using System;

using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate FLAC__StreamEncoderReadStatus FLAC__StreamEncoderReadCallback([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__byte []")] byte* buffer, [NativeTypeName("size_t *")] UIntPtr* bytes, void* client_data);
}
