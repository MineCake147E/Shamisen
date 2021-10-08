using System;

using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate FLAC__StreamDecoderReadStatus FLAC__StreamDecoderReadCallback([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder, [NativeTypeName("FLAC__byte []")] byte* buffer, [NativeTypeName("size_t *")] UIntPtr* bytes, void* client_data);
}
