using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif

namespace System.Numerics
{
    /// <summary>
    /// Represents a vector that has 8 IEEE754-single-precision-floating-point numbers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = sizeof(float) * 8)]
    public readonly struct Vector8 : IEquatable<Vector8>
    {
        /// <summary>
        /// The first value
        /// </summary>
        public float Value1 => Front.X;

        /// <summary>
        /// The second value
        /// </summary>
        public float Value2 => Front.Y;

        /// <summary>
        /// The third value
        /// </summary>
        public float Value3 => Front.Z;

        /// <summary>
        /// The fourth value
        /// </summary>
        public float Value4 => Front.W;

        /// <summary>
        /// The fifth value
        /// </summary>
        public float Value5 => Back.X;

        /// <summary>
        /// The sixth value
        /// </summary>
        public float Value6 => Back.Y;

        /// <summary>
        /// The seventh value
        /// </summary>
        public float Value7 => Back.Z;

        /// <summary>
        /// The eighth value
        /// </summary>
        public float Value8 => Back.W;

#if NETCOREAPP3_1_OR_GREATER

        private readonly Vector256<float> value;

        /// <summary>
        /// The front 4 values
        /// </summary>
        private Vector4 Front
        {
            get
            {
                var h = value.GetUpper();
                return Unsafe.As<Vector128<float>, Vector4>(ref h);
            }
        }

        /// <summary>
        /// The back 4 values
        /// </summary>
        private Vector4 Back
        {
            get
            {
                var h = value.GetLower();
                return Unsafe.As<Vector128<float>, Vector4>(ref h);
            }
        }

#else
        /// <summary>
        /// The front 4 values
        /// </summary>
        private Vector4 Front { get; }

        /// <summary>
        /// The back 4 values
        /// </summary>
        private Vector4 Back { get; }

#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector8"/> struct.
        /// </summary>
        /// <param name="value">The value to fill with.</param>
        public Vector8(float value) : this()
        {
#if NETCOREAPP3_1_OR_GREATER
            this.value = Vector256.Create(value);
#else
            Front = new Vector4(value);
            Back = new Vector4(value);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector8"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Vector8(Vector256<float> value) : this()
        {
            this.value = value;
        }

#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector8"/> struct.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="v3">The v3.</param>
        /// <param name="v4">The v4.</param>
        /// <param name="v5">The v5.</param>
        /// <param name="v6">The v6.</param>
        /// <param name="v7">The v7.</param>
        /// <param name="v8">The v8.</param>
        public Vector8(float v1, float v2, float v3, float v4, float v5, float v6, float v7, float v8) : this()
        {
#if NETCOREAPP3_1_OR_GREATER
            value = Vector256.Create(v1, v2, v3, v4, v5, v6, v7, v8);
#else
            Front = new Vector4(v1, v2, v3, v4);
            Back = new Vector4(v5, v6, v7, v8);
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector8"/> struct.
        /// </summary>
        /// <param name="front">The front four values.</param>
        /// <param name="back">The back four values.</param>
        public Vector8(Vector4 front, Vector4 back) : this()
        {
#if NETCOREAPP3_1_OR_GREATER
            value = default;
#else
            this.Front = front;
            this.Back = back;
#endif
        }

        /// <summary>
        /// Negates the specified vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>
        /// The negated vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 operator -(Vector8 value)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Avx.IsSupported)
            {
                var flags = Vector256.Create(0x8000_0000u).AsSingle();
                return new Vector8(Avx.Xor(value.value, flags));
            }
            else if (Vector<float>.Count == 8)
            {
                var v = value;
                ref var j = ref Unsafe.As<Vector8, Vector<float>>(ref v);
#pragma warning disable S1854 // Unused assignments should be removed
                j = -j;
#pragma warning restore S1854 // Unused assignments should be removed
                return v;
            }
            else
            {
                return new Vector8(-value.Front, -value.Back);
            }
#else
            if (Vector<float>.Count == 8)
            {
                var v = value;
                ref var j = ref Unsafe.As<Vector8, Vector<float>>(ref v);
#pragma warning disable S1854 // Unused assignments should be removed
                j = -j;
#pragma warning restore S1854 // Unused assignments should be removed
                return v;
            }
            else
            {
                return new Vector8(-value.Front, -value.Back);
            }
#endif
        }

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>
        /// The summed vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 operator +(Vector8 left, Vector8 right) => new Vector8(left.Front + right.Front, left.Back + right.Back);

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>
        /// The vector that results from subtracting <paramref name="right"/> from <paramref name="left"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 operator -(Vector8 left, Vector8 right) => new Vector8(left.Front - right.Front, left.Back - right.Back);

        /// <summary>
        /// Returns a new vector whose values are the product of each pair of elements in two specified vectors.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>
        /// The element-wise product vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 operator *(Vector8 left, Vector8 right) => new Vector8(left.Front * right.Front, left.Back * right.Back);

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>
        /// The vector that results from dividing left by right.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 operator /(Vector8 left, Vector8 right) => new Vector8(left.Front / right.Front, left.Back / right.Back);

        /// <summary>
        /// Multiples the specified vector by the specified scalar value.
        /// </summary>
        /// <param name="left">The vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>
        /// The scaled vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 operator *(Vector8 left, float right) => new Vector8(left.Front * right, left.Back * right);

        /// <summary>
        /// Multiples the specified vector by the specified scalar value.
        /// </summary>
        /// <param name="left">The scalar value.</param>
        /// <param name="right">The vector.</param>
        /// <returns>
        /// The scaled vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 operator *(float left, Vector8 right) => new Vector8(right.Front * left, right.Back * left);

        /// <summary>
        /// Divides the specified vector by a specified scalar value.
        /// </summary>
        /// <param name="left">The vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>
        /// The result of the division.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 operator /(Vector8 left, float right) => new Vector8(left.Front / right, left.Back / right);

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified vectors are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Vector8"/> to compare.</param>
        /// <param name="right">The second <see cref="Vector8"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector8 left, Vector8 right) => left.Front == right.Front && left.Back == right.Back;

        /// <summary>
        /// Returns a value that indicates whether two specified vectors are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Vector8"/> to compare.</param>
        /// <param name="right">The second <see cref="Vector8"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector8 left, Vector8 right) => !(left == right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is Vector8 vector && Equals(vector);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Vector8 other) => Front.Equals(other.Front) &&
                   Back.Equals(other.Back);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(Front, Back);
    }
}
