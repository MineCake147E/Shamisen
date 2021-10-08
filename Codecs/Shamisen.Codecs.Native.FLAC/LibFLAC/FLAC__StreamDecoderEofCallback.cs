using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: NativeTypeName("FLAC__bool")]
    public unsafe delegate int FLAC__StreamDecoderEofCallback([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder, void* client_data);
}
