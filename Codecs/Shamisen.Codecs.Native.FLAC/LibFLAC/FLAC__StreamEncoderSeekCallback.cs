using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate FLAC__StreamEncoderSeekStatus FLAC__StreamEncoderSeekCallback([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__uint64")] ulong absolute_byte_offset, void* client_data);
}
