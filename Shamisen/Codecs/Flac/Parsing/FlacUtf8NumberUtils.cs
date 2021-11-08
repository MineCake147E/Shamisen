using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data.Binary;

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Provides some functions for parsing UTF-8 encoded values.
    /// </summary>
    public static class FlacUtf8NumberUtils
    {
        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static uint? ReadUtf8EncodedShortNumber(IReadableDataSource<byte> source)
        {
            var first = source.ReadByte();
            var locnt = MathI.LeadingZeroCount(~((uint)first << 24));
            switch (locnt)
            {
                case 0:
                    return first;
                case 1:
                    return null;
                case < 8:
                    var bytesToRead = locnt - 1;
                    var res = MathI.ExtractBitField(first, 0, (byte)(7 - locnt)) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    source.ReadAll(q);
                    for (var i = 0; i < q.Length; i++)
                    {
                        res |= ((uint)q[i] & 0x3f) << (6 * (bytesToRead - i - 1));
                    }
                    return res;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="crc">The current state of <see cref="FlacCrc8"/>.</param>
        /// <returns></returns>
        public static uint? ReadUtf8EncodedShortNumber(IReadableDataSource<byte> source, ref FlacCrc8 crc)
        {
            var first = source.ReadByte();
            crc *= first;
            var locnt = MathI.LeadingZeroCount(~((uint)first << 24));
            switch (locnt)
            {
                case 0:
                    return first;
                case 1:
                    return null;
                case < 8:
                    var bytesToRead = locnt - 1;
                    var res = MathI.ExtractBitField(first, 0, (byte)(7 - locnt)) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    source.ReadAll(q);
                    for (var i = 0; i < q.Length; i++)
                    {
                        res |= ((uint)q[i] & 0x3f) << (6 * (bytesToRead - i - 1));
                    }
                    crc *= q;
                    return res;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="crc">The current state of <see cref="FlacCrc16"/>.</param>
        /// <returns></returns>
        public static uint? ReadUtf8EncodedShortNumber(IReadableDataSource<byte> source, ref FlacCrc16 crc)
        {
            var first = source.ReadByte();
            crc *= first;
            var locnt = MathI.LeadingZeroCount(~((uint)first << 24));
            switch (locnt)
            {
                case 0:
                    return first;
                case 1:
                    return null;
                case < 8:
                    var bytesToRead = locnt - 1;
                    var res = MathI.ExtractBitField(first, 0, (byte)(7 - locnt)) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    source.ReadAll(q);
                    for (var i = 0; i < q.Length; i++)
                    {
                        res |= ((uint)q[i] & 0x3f) << (6 * (bytesToRead - i - 1));
                    }
                    crc *= q;
                    return res;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="crc8">The current state of <see cref="FlacCrc8"/>.</param>
        /// <param name="crc16">The current state of <see cref="FlacCrc16"/>.</param>
        /// <returns></returns>
        public static uint? ReadUtf8EncodedShortNumber(IReadableDataSource<byte> source, ref FlacCrc8 crc8, ref FlacCrc16 crc16)
        {
            var first = source.ReadByte();
            crc8 *= first;
            crc16 *= first;
            var locnt = MathI.LeadingZeroCount(~((uint)first << 24));
            switch (locnt)
            {
                case 0:
                    return first;
                case 1:
                    return null;
                case < 8:
                    var bytesToRead = locnt - 1;
                    var res = MathI.ExtractBitField(first, 0, (byte)(7 - locnt)) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    source.ReadAll(q);
                    for (var i = 0; i < q.Length; i++)
                    {
                        res |= ((uint)q[i] & 0x3f) << (6 * (bytesToRead - i - 1));
                    }
                    crc8 *= q;
                    crc16 *= q;
                    return res;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ulong? ReadUtf8EncodedLongNumber(IReadableDataSource<byte> source)
        {
            var first = source.ReadByte();
            var locnt = MathI.LeadingZeroCount(~((uint)first << 24));
            switch (locnt)
            {
                case 0:
                    return first;
                case 1:
                    return null;
                case < 8:
                    var bytesToRead = locnt - 1;
                    var res = (ulong)MathI.ExtractBitField(first, 0, (byte)(7 - locnt)) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    source.ReadAll(q);
                    for (var i = 0; i < q.Length; i++)
                    {
                        res |= ((ulong)q[i] & 0x3f) << (6 * (bytesToRead - i - 1));
                    }
                    return res;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="crc">The current state of <see cref="FlacCrc8"/>.</param>
        /// <returns></returns>
        public static ulong? ReadUtf8EncodedLongNumber(IReadableDataSource<byte> source, ref FlacCrc8 crc)
        {
            var first = source.ReadByte();
            crc *= first;
            var locnt = MathI.LeadingZeroCount(~((uint)first << 24));
            switch (locnt)
            {
                case 0:
                    return first;
                case 1:
                    return null;
                case < 8:
                    var bytesToRead = locnt - 1;
                    var res = (ulong)MathI.ExtractBitField(first, 0, (byte)(7 - locnt)) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    source.ReadAll(q);
                    for (var i = 0; i < q.Length; i++)
                    {
                        res |= ((ulong)q[i] & 0x3f) << (6 * (bytesToRead - i - 1));
                    }
                    crc *= q;
                    return res;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="crc8">The current state of <see cref="FlacCrc8"/>.</param>
        /// <param name="crc16">The current state of <see cref="FlacCrc16"/>.</param>
        /// <returns></returns>
        public static ulong? ReadUtf8EncodedLongNumber(IReadableDataSource<byte> source, ref FlacCrc8 crc8, ref FlacCrc16 crc16)
        {
            var first = source.ReadByte();
            crc8 *= first;
            crc16 *= first;
            var locnt = MathI.LeadingZeroCount(~((uint)first << 24));
            switch (locnt)
            {
                case 0:
                    return first;
                case 1:
                    return null;
                case < 8:
                    var bytesToRead = locnt - 1;
                    ulong res = MathI.ExtractBitField(first, 0, (byte)(7 - locnt)) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    source.ReadAll(q);
                    for (var i = 0; i < q.Length; i++)
                    {
                        res |= ((ulong)q[i] & 0x3f) << (6 * (bytesToRead - i - 1));
                    }
                    crc8 *= q;
                    crc16 *= q;
                    return res;
                default:
                    return null;
            }
        }
    }
}
