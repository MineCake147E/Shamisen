using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data.Parsing
{
    public sealed partial class BufferedBinaryParser : IBinaryParser
    {
        IDataSource<byte> IBinaryParser.Source => Source;

        private void Advance(int length)
        {
            remainingData = remainingData.Slice(length);
            CheckRefill();
        }

        /// <summary>
        /// Reads single <see cref="byte" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseByte(out byte read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = !remainingData.IsEmpty;
            if (h)
            {
                read = MemoryMarshal.GetReference(remainingData.Span);
                Advance(sizeof(byte));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="short" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseInt16BigEndian(out short read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadInt16BigEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(short));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="short" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseInt16LittleEndian(out short read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadInt16LittleEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(short));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="int" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseInt32BigEndian(out int read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadInt32BigEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(int));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="int" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseInt32LittleEndian(out int read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadInt32LittleEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(int));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="long" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseInt64BigEndian(out long read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadInt64BigEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(long));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="long" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseInt64LittleEndian(out long read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadInt64LittleEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(long));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single <see cref="sbyte" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseSignedByte(out sbyte read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = !remainingData.IsEmpty;
            if (h)
            {
                read = Unsafe.As<byte, sbyte>(ref MemoryMarshal.GetReference(remainingData.Span));
                Advance(sizeof(sbyte));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="ushort" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseUInt16BigEndian(out ushort read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadUInt16BigEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(ushort));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="ushort" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseUInt16LittleEndian(out ushort read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadUInt16LittleEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(ushort));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="uint" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseUInt32BigEndian(out uint read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadUInt32BigEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(uint));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="uint" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseUInt32LittleEndian(out uint read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadUInt32LittleEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(uint));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="ulong" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseUInt64BigEndian(out ulong read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadUInt64BigEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(ulong));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="ulong" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseUInt64LittleEndian(out ulong read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryPrimitives.TryReadUInt64LittleEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(ulong));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="double" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseDoubleLittleEndian(out double read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryExtensions.TryReadDoubleLittleEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(double));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="double" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseDoudleBigEndian(out double read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryExtensions.TryReadDoubleBigEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(double));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="float" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseSingleLittleEndian(out float read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryExtensions.TryReadSingleLittleEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(float));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="float" /> from <see cref="Source" />.
        /// </summary>
        /// <param name="read"></param>
        /// <returns></returns>
        public ReadResult TryParseSingleBigEndian(out float read)
        {
            if (isEof)
            {
                read = default;
                return ReadResult.EndOfStream;
            }
            var h = BinaryExtensions.TryReadSingleBigEndian(remainingData.Span, out read);
            if (h)
            {
                Advance(sizeof(float));
                return 1;
            }
            else
            {
                read = default;
                return ReadResult.WaitingForSource;
            }
        }
    }
}
