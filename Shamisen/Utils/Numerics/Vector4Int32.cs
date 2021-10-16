using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
#endif

namespace Shamisen.Utils.Numerics
{
    /// <summary>
    /// Represents a vector of 4 <see cref="int"/> numbers.
    /// </summary>
    public readonly struct Vector4Int32 : IEquatable<Vector4Int32>
    {
        #region Values
        #region ValuesConcrete
#if NETCOREAPP3_1_OR_GREATER
        private readonly Vector128<int> values;
        private int V0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.GetElement(0);
        }

        private int V1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.GetElement(1);
        }

        private int V2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.GetElement(2);
        }

        private int V3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.GetElement(3);
        }
#else
        private readonly (int x, int y, int z, int w) values;
        private int V0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.x;
        }

        private int V1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.y;
        }

        private int V2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.z;
        }

        private int V3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.w;
        }
#endif
        #endregion
        /// <summary>
        /// Gets the value of 1st component of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int X
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => V0;
        }

        /// <summary>
        /// Gets the value of 2nd component of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int Y
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => V1;
        }

        /// <summary>
        /// Gets the value of 3rd component of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int Z
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => V2;
        }
        /// <summary>
        /// Gets the value of 4th component of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int W
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => V3;
        }

        #endregion
        /// <summary>
        /// Initializes a new instance of <see cref="Vector4Int32"/>.
        /// </summary>
        /// <param name="x">The 1st value of the vector.</param>
        /// <param name="y">The 2nd value of the vector.</param>
        /// <param name="z">The 3rd value of the vector.</param>
        /// <param name="w">The 4th value of the vector.</param>
        public Vector4Int32(int x, int y, int z, int w)
        {
#if NETCOREAPP3_1_OR_GREATER
            values = Vector128.Create(x, y, z, w);
#else
            values = (x, y, z, w);
#endif
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override bool Equals(object? obj) => obj is Vector4Int32 @int && Equals(@int);
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool Equals(Vector4Int32 other) => values.Equals(other.values);
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override int GetHashCode() => HashCode.Combine(values);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Vector4Int32"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Vector4Int32"/> to compare.</param>
        /// <param name="right">The second <see cref="Vector4Int32"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(Vector4Int32 left, Vector4Int32 right) => left.Equals(right);
        /// <summary>
        /// Indicates whether the values of two specified <see cref="Vector4Int32"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Vector4Int32"/> to compare.</param>
        /// <param name="right">The second  <see cref="Vector4Int32"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(Vector4Int32 left, Vector4Int32 right) => !(left == right);
    }
}
