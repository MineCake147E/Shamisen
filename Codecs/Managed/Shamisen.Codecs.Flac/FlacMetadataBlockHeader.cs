using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Represents a FLAC metadata block header.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public readonly struct FlacMetadataBlockHeader : IEquatable<FlacMetadataBlockHeader>
    {
        [FieldOffset(0)]
        private readonly byte head;

        [FieldOffset(1)]
        private readonly byte sizeHigh;

        [FieldOffset(2)]
        private readonly byte sizeMid;

        [FieldOffset(3)]
        private readonly byte sizeLow;

        private uint Value
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.As<FlacMetadataBlockHeader, uint>(ref Unsafe.AsRef(in this));
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.As<FlacMetadataBlockHeader, uint>(ref Unsafe.AsRef(in this)) = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacMetadataBlockHeader"/> struct.
        /// </summary>
        /// <param name="head">The head.</param>
        /// <param name="sizeHigh">The size high.</param>
        /// <param name="sizeMid">The size mid.</param>
        /// <param name="sizeLow">The size low.</param>
        public FlacMetadataBlockHeader(byte head, byte sizeHigh, byte sizeMid, byte sizeLow)
        {
            this.head = head;
            this.sizeHigh = sizeHigh;
            this.sizeMid = sizeMid;
            this.sizeLow = sizeLow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacMetadataBlockHeader"/> struct.
        /// </summary>
        /// <param name="isLastBlock">if set to <c>true</c> [is last block].</param>
        /// <param name="type">The type.</param>
        /// <param name="length">The length in 24-bits.</param>
        public FlacMetadataBlockHeader(bool isLastBlock, FlacMetadataBlockType type, uint length)
        {
            head = (byte)((isLastBlock ? 0x80 : 0) | ((byte)type & 0x7f));
            sizeLow = (byte)length;
            length >>= 8;
            sizeMid = (byte)length;
            length >>= 8;
            sizeHigh = (byte)length;
        }

        /// <summary>
        /// Gets a value indicating whether the block is last metadata block.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block is last metadata block; otherwise, <c>false</c>.
        /// </value>
        public bool IsLastMetadataBlock
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (head & 0x80) > 0;
        }

        /// <summary>
        /// Gets the type of the metadata block.
        /// </summary>
        /// <value>
        /// The type of the metadata block.
        /// </value>
        public FlacMetadataBlockType MetadataBlockType
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (FlacMetadataBlockType)(head & 0x7f);
        }

        /// <summary>
        /// Gets the size of the metadata block.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public uint Size
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => BinaryExtensions.ConvertToBigEndian(Value) & 0xff_ffff;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is FlacMetadataBlockHeader header && Equals(header);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(FlacMetadataBlockHeader other) => Value == other.Value;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacMetadataBlockHeader"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacMetadataBlockHeader"/> to compare.</param>
        /// <param name="right">The second <see cref="FlacMetadataBlockHeader"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(FlacMetadataBlockHeader left, FlacMetadataBlockHeader right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacMetadataBlockHeader"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacMetadataBlockHeader"/> to compare.</param>
        /// <param name="right">The second  <see cref="FlacMetadataBlockHeader"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(FlacMetadataBlockHeader left, FlacMetadataBlockHeader right) => !(left == right);
    }
}
