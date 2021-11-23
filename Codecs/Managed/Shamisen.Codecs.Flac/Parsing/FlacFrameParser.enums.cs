namespace Shamisen.Codecs.Flac.Parsing
{
    public sealed partial class FlacFrameParser
    {
        internal enum SampleRateState : byte
        {
            Value,
            RespectStreamInfo,
            GetByteKHzFromEnd,
            GetUInt16HzFromEnd,
            GetUInt16TenHzFromEnd,
            SyncFooled
        }

        internal enum BlockSizeState : byte
        {
            Value,
            GetByteFromEnd,
            GetUInt16FromEnd,
            Reserved
        }

        internal enum BitDepthState : byte
        {
            Value = 0,
            RespectStreamInfo = 1,
            Reserved = 2
        }
    }
}
