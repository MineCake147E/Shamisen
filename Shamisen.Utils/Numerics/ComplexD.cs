#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils.Intrinsics;

#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// <see cref="Complex"/> like structure based on <see cref="double"/>, which is sometimes hardware-accelerated.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = sizeof(double) * 2)]
    public readonly struct ComplexD : IEquatable<ComplexD>
    {
#if NETCOREAPP3_1_OR_GREATER
        [FieldOffset(0)]
        private readonly Vector128<double> vector;

        /// <summary>
        /// Gets the actual value as <see cref="Complex"/>.
        /// </summary>
        public Complex Value
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.As<Vector128<double>, Complex>(ref Unsafe.AsRef(in vector));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ComplexD"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        public ComplexD(Complex value) : this()
        {
            this.vector = Unsafe.As<Complex, Vector128<double>>(ref value);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ComplexD"/>.
        /// </summary>
        /// <param name="value">The value.</param>
#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
        public ComplexD(Vector128<double> value) : this()
        {
            this.vector = value;
        }

#else
        [FieldOffset(0)]
        private readonly Complex value;

        /// <summary>
        /// Gets the actual value as <see cref="Complex"/>.
        /// </summary>
        public Complex Value
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ComplexD"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        public ComplexD(Complex value) : this()
        {
            this.value = value;
        }
#endif
        #region Equality
        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is ComplexD d && Equals(d);
        /// <inheritdoc/>
        public bool Equals(ComplexD other)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    var xmm0 = Sse2.CompareEqual(vector, other.vector);
                    return Sse2.MoveMask(xmm0) == 0b11;
                }
#if NET5_0_OR_GREATER
                if (AdvSimd.Arm64.IsSupported)
                {
                    var v0 = AdvSimd.Arm64.CompareEqual(vector, other.vector);
                    var v1 = AdvSimd.Arm64.DuplicateSelectedScalarToVector128(v0, 1);
                    v0 = AdvSimd.And(v0, v1);
                    var x0 = v0.GetElement(0);
                    return x0 == -1;
                }
#endif
                return vector.Equals(other.vector);
#else
                return Value.Equals(other.Value);
#endif
            }
        }
        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Value);

        /// <inheritdoc/>
        public static bool operator ==(ComplexD left, ComplexD right) => left.Equals(right);
        /// <inheritdoc/>
        public static bool operator !=(ComplexD left, ComplexD right) => !(left == right);
        #endregion

        #region Arithmetics
        #region Unary Arithmetics
        /// <inheritdoc cref="Complex.operator -(Complex)"/>
        public static ComplexD operator -(ComplexD value)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.Arm64.IsSupported)
                {
                    return new(AdvSimd.Arm64.Negate(value.vector));
                }
                if (AdvSimd.IsSupported)
                {
                    return new(AdvSimd.Xor(value.vector.AsSingle(), Vector128.Create(0f, -0f, 0f, -0f)).AsDouble());
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    //Some CPUs execute "vpxor" ~3x faster than "vxorps"
                    return new(Sse2.Xor(value.vector.AsInt32(), Vector128.Create(0f, -0f, 0f, -0f).AsInt32()).AsDouble());
                }
                if (Sse.IsSupported)
                {
                    return new(Sse.Xor(value.vector.AsSingle(), Vector128.Create(0f, -0f, 0f, -0f)).AsDouble());
                }
#endif
                return new(-value.Value);

            }
        }
        #endregion
        #region Binary Arithmetics
        /// <summary>
        /// Adds two complex numbers.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>
        /// The sum of <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexD operator +(ComplexD left, ComplexD right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.Arm64.IsSupported)
                {
                    return new(AdvSimd.Arm64.Add(left.vector, right.vector));
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return new(Sse2.Add(left.vector, right.vector));
                }
#endif
                return new(left.Value + right.Value);
            }
        }

        /// <summary>
        /// Subtracts a complex number from another complex number.
        /// </summary>
        /// <param name="left">The value to subtract from (the minuend).</param>
        /// <param name="right">The value to subtract (the subtrahend).</param>
        /// <returns>
        /// The result of subtracting <paramref name="right"/> from <paramref name="left"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexD operator -(ComplexD left, ComplexD right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.Arm64.IsSupported)
                {
                    return new(AdvSimd.Arm64.Subtract(left.vector, right.vector));
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    return new(Sse2.Subtract(left.vector, right.vector));
                }
#endif
                return new(left.Value - right.Value);
            }
        }
        #endregion
        #endregion

        #region Functions
        /// <summary>
        /// Computes the conjugate of a complex number and returns the result.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The conjugate of <paramref name="value"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexD Conjugate(ComplexD value)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.Arm64.IsSupported)
                {
                    var v0_2d = value.vector;
                    var v1_2d = AdvSimd.Arm64.Negate(v0_2d);
                    v0_2d = AdvSimd.Arm64.InsertSelectedScalar(v0_2d, 1, v1_2d, 1);
                    return new(v0_2d);
                }
                if (AdvSimd.IsSupported)
                {
                    return new(AdvSimd.Xor(value.vector.AsSingle(), Vector128.Create(0f, 0f, 0f, -0f)).AsDouble());
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    //Some CPUs execute "vpxor" ~3x faster than "vxorps"
                    return new(Sse2.Xor(value.vector.AsInt32(), Vector128.Create(0f, 0f, 0f, -0f).AsInt32()).AsDouble());
                }
                if (Sse.IsSupported)
                {
                    return new(Sse.Xor(value.vector.AsSingle(), Vector128.Create(0f, 0f, 0f, -0f)).AsDouble());
                }
#endif
                return new(Complex.Conjugate(value.Value));
            }
        }
        #endregion
    }
}
