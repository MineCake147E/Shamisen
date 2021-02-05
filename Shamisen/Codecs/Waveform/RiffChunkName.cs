using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen.Codecs.Waveform
{
    /// <summary>
    /// Represents a name of a RIFF chunk.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct RiffChunkName : IEquatable<RiffChunkName>
    {
        [FieldOffset(0)]
        private readonly byte b0;

        [FieldOffset(1)]
        private readonly byte b1;

        [FieldOffset(2)]
        private readonly byte b2;

        [FieldOffset(3)]
        private readonly byte b3;

        /// <summary>
        /// Initializes a new instance of the <see cref="RiffChunkName"/> struct.
        /// </summary>
        /// <param name="b0">The first byte.</param>
        /// <param name="b1">The second byte.</param>
        /// <param name="b2">The third byte.</param>
        /// <param name="b3">The fourth byte.</param>
        public RiffChunkName(byte b0, byte b1, byte b2, byte b3)
        {
            this.b0 = b0;
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RiffChunkName"/> struct.
        /// </summary>
        /// <param name="valueLittleEndian">The value in little endian.</param>
        public RiffChunkName(uint valueLittleEndian)
        {
            b0 = (byte)valueLittleEndian;
            valueLittleEndian >>= 1;
            b1 = (byte)valueLittleEndian;
            valueLittleEndian >>= 1;
            b2 = (byte)valueLittleEndian;
            valueLittleEndian >>= 1;
            b3 = (byte)valueLittleEndian;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value => Encoding.UTF8.GetString(new byte[] { b0, b1, b2, b3 });

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object? obj) => obj is RiffChunkName name && Equals(name);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(RiffChunkName other) => b0 == other.b0 && b1 == other.b1 && b2 == other.b2 && b3 == other.b3;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = 2137603762;
            hashCode = (hashCode * -1521134295) + b0.GetHashCode();
            hashCode = (hashCode * -1521134295) + b1.GetHashCode();
            hashCode = (hashCode * -1521134295) + b2.GetHashCode();
            hashCode = (hashCode * -1521134295) + b3.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// The fully qualified type name.
        /// </returns>
        public override string ToString() => Value;

        /// <summary>
        /// Indicates whether the values of two specified <see cref="RiffChunkName"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="RiffChunkName"/> to compare.</param>
        /// <param name="right">The second <see cref="RiffChunkName"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(RiffChunkName left, RiffChunkName right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="RiffChunkName"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="RiffChunkName"/> to compare.</param>
        /// <param name="right">The second  <see cref="RiffChunkName"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(RiffChunkName left, RiffChunkName right) => !(left == right);
    }
}
