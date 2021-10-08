using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate FLAC__StreamDecoderWriteStatus FLAC__StreamDecoderWriteCallback([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder, [NativeTypeName("const FLAC__Frame *")] FLAC__Frame* frame, [NativeTypeName("const FLAC__int32 *const []")] int** buffer, void* client_data);
}
