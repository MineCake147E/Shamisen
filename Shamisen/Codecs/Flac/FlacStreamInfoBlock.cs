using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Represents a FLAC Stream Info block.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 34)]
    public readonly struct FlacStreamInfoBlock : IEquatable<FlacStreamInfoBlock>
    {
        [FieldOffset(0)]
        private readonly ushort blockMinSize;

        [FieldOffset(2)]
        private readonly ushort blockMaxSize;

        [FieldOffset(4)]
        private readonly UInt24 frameMinSize;

        [FieldOffset(7)]
        private readonly UInt24 frameMaxSize;

        [FieldOffset(10)]
        private readonly ulong field4;

        [FieldOffset(18)]
        private readonly ulong md5Head;

        [FieldOffset(26)]
        private readonly ulong md5Tail;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacStreamInfoBlock"/> struct directly.
        /// </summary>
        /// <param name="blockMinSize">Minimum size of the block.</param>
        /// <param name="blockMaxSize">Maximum size of the block.</param>
        /// <param name="frameMinSize">Minimum size of the frame.</param>
        /// <param name="frameMaxSize">Maximum size of the frame.</param>
        /// <param name="field4">The 5th field which contains informations of <see cref="SampleRate"/>, <see cref="Channels"/>, <see cref="BitDepth"/>, and <see cref="TotalSamples"/>.</param>
        /// <param name="md5head">The md5 head.</param>
        /// <param name="md5tail">The md5 tail.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public FlacStreamInfoBlock(ushort blockMinSize, ushort blockMaxSize, UInt24 frameMinSize, UInt24 frameMaxSize, ulong field4, ulong md5head, ulong md5tail)
        {
            this.blockMinSize = blockMinSize;
            this.blockMaxSize = blockMaxSize;
            this.frameMinSize = frameMinSize;
            this.frameMaxSize = frameMaxSize;
            this.field4 = field4;
            md5Head = md5head;
            md5Tail = md5tail;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacStreamInfoBlock"/> struct.<br/>
        /// Suitable for encoding purpose.
        /// </summary>
        /// <param name="minimumBlockSize">Minimum size of the block.</param>
        /// <param name="maximumBlockSize">Maximum size of the block.</param>
        /// <param name="minimumFrameSize">Minimum size of the frame.</param>
        /// <param name="maximumFrameSize">Maximum size of the frame.</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="bitDepth">The bit depth.</param>
        /// <param name="totalSamples">The total samples.</param>
        /// <param name="md5Head">The MD5 head.</param>
        /// <param name="md5Tail">The MD5 tail.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public FlacStreamInfoBlock(ushort minimumBlockSize, ushort maximumBlockSize, UInt24 minimumFrameSize, UInt24 maximumFrameSize, uint sampleRate, byte channels, byte bitDepth, ulong totalSamples, ulong md5Head, ulong md5Tail)
        {
            blockMinSize = minimumBlockSize;
            blockMaxSize = maximumBlockSize;
            frameMinSize = minimumFrameSize;
            frameMaxSize = maximumFrameSize;
            if (sampleRate > 655350 || sampleRate == 0) throw new ArgumentOutOfRangeException(nameof(sampleRate), "sampleRate must be between 1 and 655360!");
            if (channels > 8 || channels == 0) throw new ArgumentOutOfRangeException(nameof(sampleRate), "channels must be between 1 and 8!");
            if (bitDepth > 32 || bitDepth < 4) throw new ArgumentOutOfRangeException(nameof(bitDepth), "bitDepth must be between 4 and 32!");
            if (totalSamples >= 1ul << 36) throw new ArgumentOutOfRangeException(nameof(totalSamples), $"totalSamples must be between 0 and {(1ul << 36) - 1}!");
            sampleRate &= (uint)(~0ul >> 44);
            channels &= 0b0111;
            bitDepth &= 0x1f;
            totalSamples &= ~(~0ul << 36);
            channels--;
            bitDepth--;
            field4 = ((ulong)sampleRate << 44) | ((ulong)channels << 41) | ((ulong)bitDepth << 36) | totalSamples;
            this.md5Head = md5Head;
            this.md5Tail = md5Tail;
        }

        /// <summary>
        /// Gets the minimum size of the block in samples.
        /// </summary>
        /// <value>
        /// The minimum size of the block.
        /// </value>
        public ushort MinimumBlockSize
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => blockMinSize;
        }

        /// <summary>
        /// Gets the maximum size of the block.
        /// </summary>
        /// <value>
        /// The maximum size of the block.
        /// </value>
        public ushort MaximumBlockSize
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => blockMaxSize;
        }

        /// <summary>
        /// Gets the minimum size of the FLAC frame in bytes.
        /// </summary>
        /// <value>
        /// The minimum size of the frame.
        /// </value>
        public UInt24 MinimumFrameSize
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => frameMinSize;
        }

        /// <summary>
        /// Gets the maximum size of the FLAC frame in bytes.
        /// </summary>
        /// <value>
        /// The maximum size of the frame.
        /// </value>
        public UInt24 MaximumFrameSize
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => frameMaxSize;
        }

        /// <summary>
        /// Gets the sample rate.
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        public uint SampleRate
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (uint)(field4 >> 44);
        }

        /// <summary>
        /// Gets the channels.
        /// </summary>
        /// <value>
        /// The channels.
        /// </value>
        public int Channels
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (int)((field4 >> 41) & 0x7) + 1;
        }

        /// <summary>
        /// Gets the bit depth.
        /// </summary>
        /// <value>
        /// The bit depth.
        /// </value>
        public int BitDepth
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (int)((field4 >> 36) & 0b1_1111) + 1;
        }

        /// <summary>
        /// Gets the total inter-channel samples(aka "Frames" in Shamisen) in this FLAC stream.
        /// </summary>
        /// <value>
        /// The total samples.
        /// </value>
        public ulong TotalSamples
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => field4 & ~(~0ul << 36);
        }

        /// <summary>
        /// Gets the MD5 signature.
        /// </summary>
        /// <value>
        /// The MD5 signature.
        /// </value>
        public Memory<byte> MD5Signature
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get
            {
                var h = new byte[16];
                BinaryPrimitives.WriteUInt64BigEndian(h, md5Head);
                BinaryPrimitives.WriteUInt64BigEndian(h.AsSpan(8), md5Head);
                return h;
            }
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> read directly from <see cref="Span{T}"/> of <see cref="byte"/>, to readable value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacStreamInfoBlock ToReadableValue(FlacStreamInfoBlock value)
            => !BitConverter.IsLittleEndian ? value
                : ReverseEndianness(value);

        /// <summary>
        /// Converts the specified <paramref name="value"/> constructed manually in user code, to writable value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacStreamInfoBlock ToWritableValue(FlacStreamInfoBlock value) => ToReadableValue(value);

        /// <summary>
        /// Reverses the endianness of every field in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to convert endianness.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacStreamInfoBlock ReverseEndianness(FlacStreamInfoBlock value)
            => new(
                    BinaryPrimitives.ReverseEndianness(value.blockMinSize),
                    BinaryPrimitives.ReverseEndianness(value.blockMaxSize),
                    UInt24.ReverseEndianness(value.frameMinSize),
                    UInt24.ReverseEndianness(value.frameMaxSize),
                    BinaryPrimitives.ReverseEndianness(value.field4),
                    BinaryPrimitives.ReverseEndianness(value.md5Head),
                    BinaryPrimitives.ReverseEndianness(value.md5Tail));

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is FlacStreamInfoBlock block && Equals(block);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(FlacStreamInfoBlock other) => blockMinSize == other.blockMinSize && blockMaxSize == other.blockMaxSize && frameMinSize.Equals(other.frameMinSize) && frameMaxSize.Equals(other.frameMaxSize) && field4 == other.field4 && md5Head == other.md5Head && md5Tail == other.md5Tail;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(blockMinSize, blockMaxSize, frameMinSize, frameMaxSize, field4, md5Head, md5Tail);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacStreamInfoBlock"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacStreamInfoBlock"/> to compare.</param>
        /// <param name="right">The second <see cref="FlacStreamInfoBlock"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(FlacStreamInfoBlock left, FlacStreamInfoBlock right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacStreamInfoBlock"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacStreamInfoBlock"/> to compare.</param>
        /// <param name="right">The second  <see cref="FlacStreamInfoBlock"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(FlacStreamInfoBlock left, FlacStreamInfoBlock right) => !(left == right);
    }
}
