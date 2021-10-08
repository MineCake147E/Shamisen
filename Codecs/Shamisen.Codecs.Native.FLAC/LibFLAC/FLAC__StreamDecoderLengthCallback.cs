using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate FLAC__StreamDecoderLengthStatus FLAC__StreamDecoderLengthCallback([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder, [NativeTypeName("FLAC__uint64 *")] ulong* stream_length, void* client_data);
}
