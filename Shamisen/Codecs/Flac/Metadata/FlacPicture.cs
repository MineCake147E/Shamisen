using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Metadata
{
    /// <summary>
    /// Represents a picture in FLAC files.
    /// </summary>
    public readonly struct FlacPicture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlacPicture"/> struct.
        /// </summary>
        /// <param name="pictureType">Type of the picture.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="description">The description.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="colorDepth">The color depth.</param>
        /// <param name="indexedColors">The indexed colors.</param>
        /// <param name="data">The data.</param>
        public FlacPicture(FlacPictureType pictureType, ReadOnlyMemory<byte> mimeType, ReadOnlyMemory<byte> description, uint width, uint height, uint colorDepth, uint indexedColors, ReadOnlyMemory<byte> data)
        {
            PictureType = pictureType;
            MimeType = mimeType;
            Description = description;
            Width = width;
            Height = height;
            ColorDepth = colorDepth;
            IndexedColors = indexedColors;
            Data = data;
        }

        /// <summary>
        /// Gets the type of the picture.
        /// </summary>
        /// <value>
        /// The type of the picture.
        /// </value>
        public FlacPictureType PictureType { get; }

        /// <summary>
        /// Gets the MIME type string.
        /// </summary>
        /// <value>
        /// The MIME type string.
        /// </value>
        public ReadOnlyMemory<byte> MimeType { get; }

        /// <summary>
        /// Gets the description of the picture.
        /// </summary>
        /// <value>
        /// The description of the picture.
        /// </value>
        public ReadOnlyMemory<byte> Description { get; }

        /// <summary>
        /// Gets the width of the picture.
        /// </summary>
        /// <value>
        /// The width of the picture.
        /// </value>
        public uint Width { get; }

        /// <summary>
        /// Gets the height of the picture.
        /// </summary>
        /// <value>
        /// The height of the picture.
        /// </value>
        public uint Height { get; }

        /// <summary>
        /// Gets the color depth of the picture in bits per pixel.
        /// </summary>
        /// <value>
        /// The color depth.
        /// </value>
        public uint ColorDepth { get; }

        /// <summary>
        /// Gets the number of indexed colors. 0 for non-indexed pictures.
        /// </summary>
        /// <value>
        /// The number of indexed colors.
        /// </value>
        public uint IndexedColors { get; }

        /// <summary>
        /// Gets the binary picture data.
        /// </summary>
        /// <value>
        /// The binary picture data.
        /// </value>
        public ReadOnlyMemory<byte> Data { get; }

        /// <summary>
        /// Reads the <see cref="FlacPicture"/> from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static FlacPicture ReadFrom(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < sizeof(uint) * 8)
            {
                throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
            }
            uint pictype = BinaryPrimitives.ReadUInt32BigEndian(buffer);
            var br = buffer.Slice(4);
            int mimeLength = (int)BinaryPrimitives.ReadUInt32BigEndian(br);
            if ((uint)mimeLength > br.Length - sizeof(uint) * 6) throw new FlacException("Invalid Picture!");
            br = br.Slice(4);
            byte[]? mime = new byte[mimeLength];
            br.SliceWhile(mimeLength).CopyTo(mime);
            br = br.Slice(mimeLength);
            int descLength = (int)BinaryPrimitives.ReadUInt32BigEndian(br);
            br = br.Slice(4);
            if ((uint)descLength > br.Length - sizeof(uint) * 5) throw new FlacException("Invalid Picture!");
            byte[]? desc = new byte[descLength];
            br.SliceWhile(descLength).CopyTo(desc);
            br = br.Slice(descLength);
            //ValueTuples of equal-typed variables deconstructs sequentially.
            var a = MemoryMarshal.Read<FlacPictureMetadataFiveIntegers>(br);
            var (w, h, bpp, indLen, binSize) = (BinaryExtensions.ConvertToBigEndian(a.width),
                BinaryExtensions.ConvertToBigEndian(a.height),
                BinaryExtensions.ConvertToBigEndian(a.bpp),
                BinaryExtensions.ConvertToBigEndian(a.palleteSize),
                BinaryExtensions.ConvertToBigEndian(a.binaryLength));
            br = br.Slice(sizeof(int) * 5);
            if (br.Length < binSize)
            {
                throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
            }
            byte[]? bin = new byte[binSize];
            br.SliceWhile(bin.Length).CopyTo(bin);
            return new FlacPicture((FlacPictureType)pictype, mime, desc, w, h, bpp, indLen, bin);
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = sizeof(uint) * 5)]
        private readonly struct FlacPictureMetadataFiveIntegers
        {
            [FieldOffset(0)]
            internal readonly uint width;

            [FieldOffset(sizeof(uint) * 1)]
            internal readonly uint height;

            [FieldOffset(sizeof(uint) * 2)]
            internal readonly uint bpp;

            [FieldOffset(sizeof(uint) * 3)]
            internal readonly uint palleteSize;

            [FieldOffset(sizeof(uint) * 4)]
            internal readonly uint binaryLength;
        }
    }
}
