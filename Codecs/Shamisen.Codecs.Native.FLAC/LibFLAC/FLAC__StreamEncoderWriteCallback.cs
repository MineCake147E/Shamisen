using System;

using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate FLAC__StreamEncoderWriteStatus FLAC__StreamEncoderWriteCallback([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder, [NativeTypeName("const FLAC__byte []")] byte* buffer, [NativeTypeName("size_t")] UIntPtr bytes, [NativeTypeName("uint32_t")] uint samples, [NativeTypeName("uint32_t")] uint current_frame, void* client_data);
}
