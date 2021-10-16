using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Numerics
{
    /// <summary>
    /// Represents a vector that has 7 IEEE754-single-precision-floating-point numbers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = sizeof(float) * 7)]
    public readonly struct Vector7 : IEquatable<Vector7>
    {
        /// <summary>
        /// The first value
        /// </summary>
        public float Value1 => front.X;

        /// <summary>
        /// The second value
        /// </summary>
        public float Value2 => front.Y;

        /// <summary>
        /// The third value
        /// </summary>
        public float Value3 => front.Z;

        /// <summary>
        /// The fourth value
        /// </summary>
        public float Value4 => front.W;

        /// <summary>
        /// The fifth value
        /// </summary>
        public float Value5 => back.X;

        /// <summary>
        /// The sixth value
        /// </summary>
        public float Value6 => back.Y;

        /// <summary>
        /// The seventh value
        /// </summary>
        public float Value7 => back.Z;

        /// <summary>
        /// The front 3 values
        /// </summary>
        private readonly Vector4 front;

        /// <summary>
        /// The back 4 values
        /// </summary>
        private readonly Vector3 back;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector7"/> struct.
        /// </summary>
        /// <param name="value">The value to fill with.</param>
        public Vector7(float value)
        {
            front = new Vector4(value);
            back = new Vector3(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector7"/> struct.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="v3">The v3.</param>
        /// <param name="v4">The v4.</param>
        /// <param name="v5">The v5.</param>
        /// <param name="v6">The v6.</param>
        /// <param name="v7">The v7.</param>
        public Vector7(float v1, float v2, float v3, float v4, float v5, float v6, float v7)
        {
            front = new Vector4(v1, v2, v3, v4);
            back = new Vector3(v5, v6, v7);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector7"/> struct.
        /// </summary>
        /// <param name="front">The front four values.</param>
        /// <param name="back">The back three values.</param>
        public Vector7(Vector4 front, Vector3 back)
        {
            this.front = front;
            this.back = back;
        }

        /// <summary>
        /// Negates the specified vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>
        /// The negated vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector7 operator -(Vector7 value) => new Vector7(-value.front, -value.back);

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>
        /// The summed vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector7 operator +(Vector7 left, Vector7 right) => new Vector7(left.front + right.front, left.back + right.back);

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>
        /// The vector that results from subtracting <paramref name="right"/> from <paramref name="left"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector7 operator -(Vector7 left, Vector7 right) => new Vector7(left.front - right.front, left.back - right.back);

        /// <summary>
        /// Returns a new vector whose values are the product of each pair of elements in two specified vectors.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>
        /// The element-wise product vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector7 operator *(Vector7 left, Vector7 right) => new Vector7(left.front * right.front, left.back * right.back);

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>
        /// The vector that results from dividing left by right.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector7 operator /(Vector7 left, Vector7 right) => new Vector7(left.front / right.front, left.back / right.back);

        /// <summary>
        /// Multiples the specified vector by the specified scalar value.
        /// </summary>
        /// <param name="left">The vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>
        /// The scaled vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector7 operator *(Vector7 left, float right) => new Vector7(left.front * right, left.back * right);

        /// <summary>
        /// Multiples the specified vector by the specified scalar value.
        /// </summary>
        /// <param name="left">The scalar value.</param>
        /// <param name="right">The vector.</param>
        /// <returns>
        /// The scaled vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector7 operator *(float left, Vector7 right) => new Vector7(right.front * left, right.back * left);

        /// <summary>
        /// Divides the specified vector by a specified scalar value.
        /// </summary>
        /// <param name="left">The vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>
        /// The result of the division.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector7 operator /(Vector7 left, float right) => new Vector7(left.front / right, left.back / right);

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified vectors are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Vector7"/> to compare.</param>
        /// <param name="right">The second <see cref="Vector7"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector7 left, Vector7 right) => left.front == right.front && left.back == right.back;

        /// <summary>
        /// Returns a value that indicates whether two specified vectors are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Vector7"/> to compare.</param>
        /// <param name="right">The second <see cref="Vector7"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector7 left, Vector7 right) => !(left == right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is Vector7 vector && Equals(vector);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Vector7 other) => front.Equals(other.front) &&
                   back.Equals(other.back);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(front, back);
    }
}
