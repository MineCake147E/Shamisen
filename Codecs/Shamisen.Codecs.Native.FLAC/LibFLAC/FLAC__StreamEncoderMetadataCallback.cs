using System;

using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void FLAC__StreamEncoderMetadataCallback([NativeTypeName("const FLAC__StreamEncoder *")] FLAC__StreamEncoder* encoder, [NativeTypeName("const FLAC__StreamMetadata *")] FLAC__StreamMetadata* metadata, void* client_data);
}
