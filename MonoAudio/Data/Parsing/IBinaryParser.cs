using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Data.Parsing
{
    /// <summary>
    /// Defines a base infrastructure to parse binary data from <see cref="IDataSource{TSample}"/>.
    /// </summary>
    public interface IBinaryParser
    {
        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        IDataSource<byte> Source { get; }

        #region TryFunctions

        #region byte and sbyte

        /// <summary>
        /// Reads single <see cref="byte"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseByte(out byte read);

        /// <summary>
        /// Reads single <see cref="sbyte"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseSignedByte(out sbyte read);

        #endregion byte and sbyte

        #region ushort and short

        /// <summary>
        /// Reads single little-endianed <see cref="ushort"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseUInt16LittleEndian(out ushort read);

        /// <summary>
        /// Reads single big-endianed <see cref="ushort"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseUInt16BigEndian(out ushort read);

        /// <summary>
        /// Reads single little-endianed <see cref="short"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseInt16LittleEndian(out short read);

        /// <summary>
        /// Reads single big-endianed <see cref="short"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseInt16BigEndian(out short read);

        #endregion ushort and short

        #region uint and int

        /// <summary>
        /// Reads single little-endianed <see cref="uint"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseUInt32LittleEndian(out uint read);

        /// <summary>
        /// Reads single big-endianed <see cref="uint"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseUInt32BigEndian(out uint read);

        /// <summary>
        /// Reads single little-endianed <see cref="int"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseInt32LittleEndian(out int read);

        /// <summary>
        /// Reads single big-endianed <see cref="int"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseInt32BigEndian(out int read);

        #endregion uint and int

        #region ulong and long

        /// <summary>
        /// Reads single little-endianed <see cref="ulong"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseUInt64LittleEndian(out ulong read);

        /// <summary>
        /// Reads single big-endianed <see cref="ulong"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseUInt64BigEndian(out ulong read);

        /// <summary>
        /// Reads single little-endianed <see cref="long"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseInt64LittleEndian(out long read);

        /// <summary>
        /// Reads single big-endianed <see cref="long"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseInt64BigEndian(out long read);

        #endregion ulong and long

        #region float and double

        /// <summary>
        /// Reads single little-endianed <see cref="double"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseDoubleLittleEndian(out double read);

        /// <summary>
        /// Reads single big-endianed <see cref="double"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseDoudleBigEndian(out double read);

        /// <summary>
        /// Reads single little-endianed <see cref="float"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseSingleLittleEndian(out float read);

        /// <summary>
        /// Reads single big-endianed <see cref="float"/> from <see cref="Source"/>.
        /// </summary>
        /// <returns></returns>
        ReadResult TryParseSingleBigEndian(out float read);

        #endregion float and double

        #endregion TryFunctions
    }
}
