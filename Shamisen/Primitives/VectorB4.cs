using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Represents a vector of four <see cref="byte"/> elements.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct VectorB4 : IEquatable<VectorB4>
    {
        [FieldOffset(0)]
        private readonly byte v0;

        [FieldOffset(1)]
        private readonly byte v1;

        [FieldOffset(2)]
        private readonly byte v2;

        [FieldOffset(3)]
        private readonly byte v3;

        /// <summary>
        /// Gets the 0th value.
        /// </summary>
        /// <value>
        /// The value0.
        /// </value>
        public byte Value0 => v0;

        /// <summary>
        /// Gets the 1st value.
        /// </summary>
        /// <value>
        /// The value1.
        /// </value>
        public byte Value1 => v1;

        /// <summary>
        /// Gets the 2nd value.
        /// </summary>
        /// <value>
        /// The value2.
        /// </value>
        public byte Value2 => v2;

        /// <summary>
        /// Gets the 3rd value.
        /// </summary>
        /// <value>
        /// The value3.
        /// </value>
        public byte Value3 => v3;

        private string GetString() =>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
            Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<VectorB4, byte>(ref Unsafe.AsRef(in this)), 4));

#else
            Encoding.UTF8.GetString(new byte[] { v0, v1, v2, v3 });
#endif

        /// <summary>
        /// Gets the string view.
        /// </summary>
        /// <value>
        /// The string view.
        /// </value>
        public string StringView => $"{v0:X02}{v1:X02}{v2:X02}{v3:X02} (\"{GetString()}\")";

        /// <summary>
        /// Gets the string view.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => StringView;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is VectorB4 b && Equals(b);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(VectorB4 other) => v0 == other.v0 && v1 == other.v1 && v2 == other.v2 && v3 == other.v3;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(v0, v1, v2, v3);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="VectorB4"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="VectorB4"/> to compare.</param>
        /// <param name="right">The second <see cref="VectorB4"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(VectorB4 left, VectorB4 right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="VectorB4"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="VectorB4"/> to compare.</param>
        /// <param name="right">The second  <see cref="VectorB4"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(VectorB4 left, VectorB4 right) => !(left == right);

        /// <summary>
        /// Gets the debugger display.
        /// </summary>
        /// <returns></returns>
        private string GetDebuggerDisplay() => StringView;
    }
}
