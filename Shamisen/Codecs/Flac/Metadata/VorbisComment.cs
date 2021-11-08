using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Metadata
{
    /// <summary>
    /// Represents a Vorbis comment without framing bit.
    /// </summary>
    public readonly struct VorbisComment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VorbisComment"/> struct.
        /// </summary>
        /// <param name="vendor">The vendor.</param>
        /// <param name="userComments">The user comments.</param>
        public VorbisComment(ReadOnlyMemory<byte> vendor, ReadOnlyMemory<ReadOnlyMemory<byte>> userComments)
        {
            RawVendor = vendor;
            RawUserComments = userComments;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
            Vendor = Encoding.UTF8.GetString(RawVendor.Span);

#else
            Vendor = Encoding.UTF8.GetString(RawVendor.ToArray());

#endif
            var uc = new string[RawUserComments.Length];
            var span = RawUserComments.Span;
            for (var i = 0; i < RawUserComments.Length; i++)
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
                uc[i] = Encoding.UTF8.GetString(span[i].Span);
#else
                uc[i] = Encoding.UTF8.GetString(span[i].ToArray());
#endif
            }
            UserComments = uc;
        }

        /// <summary>
        /// Gets the raw data of <see cref="Vendor"/>.
        /// </summary>
        /// <value>
        /// The vendor.
        /// </value>
        public ReadOnlyMemory<byte> RawVendor { get; }

        /// <summary>
        /// Gets the raw data of user comments.
        /// </summary>
        /// <value>
        /// The user comments.
        /// </value>
        public ReadOnlyMemory<ReadOnlyMemory<byte>> RawUserComments { get; }

        /// <summary>
        /// Gets the vendor.
        /// </summary>
        /// <value>
        /// The vendor.
        /// </value>
        public string Vendor { get; }

        /// <summary>
        /// Gets the user comments.
        /// </summary>
        /// <value>
        /// The user comments.
        /// </value>
        public string[] UserComments { get; }

        /// <summary>
        /// Reads the <see cref="VorbisComment"/> from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static VorbisComment ReadFrom(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 8)
            {
                throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
            }
            var vendorLength = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            if (vendorLength > buffer.Length - 8 || vendorLength > UInt24.MaxValue)
            {
                throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
            }
            var vendor = new byte[vendorLength];
            buffer.Slice(4, (int)vendorLength).CopyTo(vendor);
            var uclength = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(4 + (int)vendorLength));
            var br = buffer.Slice(8 + (int)vendorLength);
            var userComments = new ReadOnlyMemory<byte>[uclength];
            for (var i = 0; i < userComments.Length; i++)
            {
                if (br.Length < 4)
                {
                    throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
                }
                var clen = (int)BinaryPrimitives.ReadUInt32LittleEndian(br);
                var comment = new byte[clen];
                br = br.Slice(4);
                if (br.Length < clen)
                {
                    throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
                }
                br.SliceWhile(clen).CopyTo(comment);
                br = br.Slice(clen);
                userComments[i] = comment;
            }
            return new(vendor, userComments);
        }
    }
}
