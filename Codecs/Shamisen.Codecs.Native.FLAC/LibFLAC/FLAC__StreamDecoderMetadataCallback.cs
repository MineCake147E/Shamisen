using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void FLAC__StreamDecoderMetadataCallback([NativeTypeName("const FLAC__StreamDecoder *")] FLAC__StreamDecoder* decoder, [NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* metadata, void* client_data);
}
