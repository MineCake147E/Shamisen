using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void FLAC__StreamEncoderProgressCallback([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder, [NativeTypeName("FLAC__uint64")] ulong bytes_written, [NativeTypeName("FLAC__uint64")] ulong samples_written, [NativeTypeName("uint32_t")] uint frames_written, [NativeTypeName("uint32_t")] uint total_frames_estimate, void* client_data);
}
