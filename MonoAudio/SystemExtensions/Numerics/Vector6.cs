using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Numerics
{
    /// <summary>
    /// Represents a vector that has 6 IEEE754-single-precision-floating-point numbers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = sizeof(float) * 6)]
    public readonly struct Vector6 : IEquatable<Vector6>
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
        public float Value4 => Back.X;

        /// <summary>
        /// The fifth value
        /// </summary>
        public float Value5 => Back.Y;

        /// <summary>
        /// The sixth value
        /// </summary>
        public float Value6 => Back.Z;

        /// <summary>
        /// The front 3 values
        /// </summary>
        private readonly Vector3 Front;

        /// <summary>
        /// The back 3 values
        /// </summary>
        private readonly Vector3 Back;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector6"/> struct.
        /// </summary>
        /// <param name="value">The value to fill with.</param>
        public Vector6(float value)
        {
            Front = new Vector3(value);
            Back = new Vector3(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector6"/> struct.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="v3">The v3.</param>
        /// <param name="v4">The v4.</param>
        /// <param name="v5">The v5.</param>
        /// <param name="v6">The v6.</param>
        public Vector6(float v1, float v2, float v3, float v4, float v5, float v6)
        {
            Front = new Vector3(v1, v2, v3);
            Back = new Vector3(v4, v5, v6);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector6"/> struct.
        /// </summary>
        /// <param name="front">The front three values.</param>
        /// <param name="back">The back three values.</param>
        public Vector6(Vector3 front, Vector3 back)
        {
            Front = front;
            Back = back;
        }

        /// <summary>
        /// Negates the specified vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>
        /// The negated vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator -(Vector6 value) => new Vector6(-value.Front, -value.Back);

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>
        /// The summed vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator +(Vector6 left, Vector6 right) => new Vector6(left.Front + right.Front, left.Back + right.Back);

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>
        /// The vector that results from subtracting <paramref name="right"/> from <paramref name="left"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator -(Vector6 left, Vector6 right) => new Vector6(left.Front - right.Front, left.Back - right.Back);

        /// <summary>
        /// Returns a new vector whose values are the product of each pair of elements in two specified vectors.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>
        /// The element-wise product vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator *(Vector6 left, Vector6 right) => new Vector6(left.Front * right.Front, left.Back * right.Back);

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>
        /// The vector that results from dividing left by right.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator /(Vector6 left, Vector6 right) => new Vector6(left.Front / right.Front, left.Back / right.Back);

        /// <summary>
        /// Multiples the specified vector by the specified scalar value.
        /// </summary>
        /// <param name="left">The vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>
        /// The scaled vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator *(Vector6 left, float right) => new Vector6(left.Front * right, left.Back * right);

        /// <summary>
        /// Multiples the specified vector by the specified scalar value.
        /// </summary>
        /// <param name="left">The scalar value.</param>
        /// <param name="right">The vector.</param>
        /// <returns>
        /// The scaled vector.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator *(float left, Vector6 right) => new Vector6(right.Front * left, right.Back * left);

        /// <summary>
        /// Divides the specified vector by a specified scalar value.
        /// </summary>
        /// <param name="left">The vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>
        /// The result of the division.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator /(Vector6 left, float right) => new Vector6(left.Front / right, left.Back / right);

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified vectors are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Vector6"/> to compare.</param>
        /// <param name="right">The second <see cref="Vector6"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector6 left, Vector6 right) => left.Front == right.Front && left.Back == right.Back;

        /// <summary>
        /// Returns a value that indicates whether two specified vectors are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Vector6"/> to compare.</param>
        /// <param name="right">The second <see cref="Vector6"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector6 left, Vector6 right) => !(left == right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Vector6 vector && Equals(vector);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Vector6 other)
        {
            return Front.Equals(other.Front) &&
                   Back.Equals(other.Back);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = -1890742534;
            hashCode = hashCode * -1521134295 + Front.GetHashCode();
            hashCode = hashCode * -1521134295 + Back.GetHashCode();
            return hashCode;
        }
    }
}
