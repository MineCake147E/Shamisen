#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils.Intrinsics;

#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils;

using System.Diagnostics;

namespace Shamisen
{
    /// <summary>
    /// <see cref="Complex"/> like structure based on <see cref="float"/>.<br/>
    /// Mainly used for storage purposes.
    /// </summary>
#if NET5_0_OR_GREATER
    [SkipLocalsInit]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
#endif
    public readonly partial struct ComplexF : IEquatable<ComplexF>, IFormattable
    {
        private readonly Vector2 value;

        /// <summary>
        /// Gets the actual value stored in <see cref="ComplexF"/>.
        /// </summary>
        public Vector2 Value => value;

        //TODO: functions compatible to Complex

        /// <inheritdoc cref="Complex.Real"/>
        public float Real
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => value.X;
        }

        /// <inheritdoc cref="Complex.Imaginary"/>
        public float Imaginary
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => value.Y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexF"/> struct.
        /// </summary>
        /// <param name="real">The real part.</param>
        /// <param name="imaginary">The imaginary part.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
        public ComplexF(float real, float imaginary) : this(new Vector2(real, imaginary))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexF"/> struct.
        /// </summary>
        /// <param name="value">The value vector X=Real, Y=Imaginary.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
        public ComplexF(Vector2 value)
        {
            this.value = value;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
        private ComplexF(double value)
        {
            Unsafe.SkipInit(out this.value);
            Unsafe.As<Vector2, double>(ref Unsafe.AsRef(this.value)) = value;
        }

#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
        private ComplexF(in Vector128<float> value)
        {
            this.value = value.AsVector2();
        }
#endif

        #region Arithmetics
        #region Unary Arithmetics

        /// <summary>
        /// Returns the additive inverse of a specified complex number.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>
        /// The result of the <see cref="Real"/> and <see cref="Imaginary"/> components of the value parameter multiplied by -1.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF operator -(in ComplexF value) => AsComplexF(-value.value);

        #endregion Unary Arithmetics

        #region Binary Arithmetics

        /// <summary>
        /// Subtracts a complex number from another complex number.
        /// </summary>
        /// <param name="left">The value to subtract from (the minuend).</param>
        /// <param name="right">The value to subtract (the subtrahend).</param>
        /// <returns>
        /// The result of subtracting <paramref name="right"/> from <paramref name="left"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF operator -(in ComplexF left, in ComplexF right) => new(left.value - right.value);

        /// <summary>
        /// Adds two complex numbers.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>
        /// The sum of <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF operator +(in ComplexF left, in ComplexF right) => new(left.value + right.value);

        /// <summary>
        /// Multiplies two specified complex numbers.
        /// </summary>
        /// <param name="left">The first value to multiply.</param>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>
        /// The product of <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
        public static ComplexF operator *(in ComplexF left, in ComplexF right)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Avx2.IsSupported)
                {
                    var xmm0 = left.AsVector128();
                    var xmm1 = right.AsVector128();
                    var xmm2 = Avx2.BroadcastScalarToVector128(xmm0);
                    xmm2 = Sse.Multiply(xmm2, xmm1);
                    xmm0 = Sse3.MoveHighAndDuplicate(xmm0);
                    xmm1 = Avx.Permute(xmm1, 0xb1);
                    xmm0 = Sse.Multiply(xmm0, xmm1);
                    xmm0 = Sse3.AddSubtract(xmm2, xmm0);
                    return xmm0.AsComplexF();
                }
                if (Sse3.IsSupported)
                {
                    var xmm0 = left.AsVector128();
                    var xmm1 = right.AsVector128();
                    var xmm2 = Sse3.MoveLowAndDuplicate(xmm0);
                    xmm2 = Sse.Multiply(xmm2, xmm1);
                    xmm0 = Sse3.MoveHighAndDuplicate(xmm0);
                    xmm1 = Sse.Shuffle(xmm1, xmm1, 0xb1);
                    xmm0 = Sse.Multiply(xmm0, xmm1);
                    xmm0 = Sse3.AddSubtract(xmm2, xmm0);
                    return xmm0.AsComplexF();
                }
#endif
                var p = VectorUtils.ReverseElements(right.value) * left.value.Y;
                var re = right.value * left.value.X;
                return new(VectorUtils.AddSubtract(re, p));
            }
        }

        /// <summary>
        /// Multiplies specified complex number with specified real number.
        /// </summary>
        /// <param name="left">The first value to multiply.</param>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>
        /// The product of <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF operator *(in ComplexF left, float right) => AsComplexF(left.value * right);

        /// <summary>
        /// Divides a specified complex number by another specified complex number.
        /// </summary>
        /// <param name="left">The value to be divided.</param>
        /// <param name="right">The value to divide by.</param>
        /// <returns>
        /// The result of dividing <paramref name="left"/> by <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF operator /(in ComplexF left, in ComplexF right)
        {
            var div = 1.0f / right.value.LengthSquared();
            return new ComplexF(div * (left.Real * right.Real + left.Imaginary * right.Imaginary),
                div * (left.Imaginary * right.Real - left.Real * right.Imaginary));
        }

        #endregion Binary Arithmetics
        #endregion

        #region Utility Properties

        /// <inheritdoc cref="Complex.Magnitude"/>
        public float Magnitude => value.Length();

        /// <inheritdoc cref="Complex.Phase"/>
        public float Phase => (float)Math.Atan2(value.Y, value.X);

        #endregion

        #region Constants and read-only fields

        /// <summary>
        /// The inverse of ln(10)
        /// </summary>
        /// <autogeneratedoc />
        private const float InverseLog10 = 0.43429448190325f;

        /// <summary>
        /// Returns a new Complex instance with a real number equal to zero and an imaginary number equal to zero.
        /// </summary>
        public static ComplexF Zero => new(0.0f, 0.0f);

        /// <summary>
        /// Returns a new Complex instance with a real number equal to one and an imaginary number equal to zero.
        /// </summary>
        public static ComplexF One => new(1.0f, 0.0f);

        /// <summary>
        /// Returns a new Complex instance with a real number equal to zero and an imaginary number equal to one.
        /// </summary>
        public static ComplexF ImaginaryOne => new(0.0f, 1.0f);

        #endregion Constants and read-only fields

        #region Equality

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ComplexF"/> objects are equal.
        /// </summary>
        /// <param name="complex1">The first <see cref="ComplexF"/> to compare.</param>
        /// <param name="complex2">The second <see cref="ComplexF"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the value of complex1 is the same as the value of complex2; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(ComplexF complex1, ComplexF complex2) => complex1.value == complex2.value;

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ComplexF"/> objects are not equal.
        /// </summary>
        /// <param name="complex1">The first <see cref="ComplexF"/> to compare.</param>
        /// <param name="complex2">The second <see cref="ComplexF"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if complex1 and complex2 are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(ComplexF complex1, ComplexF complex2) => !(complex1 == complex2);

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override bool Equals(object? obj) => obj is ComplexF complex && value.Equals(complex.value);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override int GetHashCode() => -1584136870 + value.GetHashCode();

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool Equals(ComplexF other) => this == other;

        #endregion Equality

        #region Format

        /// <summary>
        /// Converts this instance to string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public string ToString(string? format, IFormatProvider? formatProvider) => value.ToString(format, formatProvider);

        #endregion Format

        #region Implicit Conversions

        /// <summary>
        /// Performs an implicit conversion from <see cref="ComplexF"/> to <see cref="Complex"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator Complex(ComplexF value) => new(value.Real, value.Imaginary);

        /// <summary>
        /// Performs an implicit conversion from <see cref="float"/> to <see cref="ComplexF"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator ComplexF(float value) => new(value, 0);

        #endregion Implicit Conversions

        #region Explicit Conversions

        /// <summary>
        /// Performs an explicit conversion from <see cref="Complex"/> to <see cref="ComplexF"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static explicit operator ComplexF(in Complex value)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.Arm64.IsSupported)
                {
                    var v0_2d = Unsafe.As<Complex, Vector128<double>>(ref Unsafe.AsRef(in value));
                    var v1_2s = AdvSimd.Arm64.ConvertToSingleLower(v0_2d);
                    return v1_2s.ToVector128Unsafe().AsComplexF();
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    var xmm0 = Unsafe.As<Complex, Vector128<double>>(ref Unsafe.AsRef(in value));
                    var xmm1 = Sse2.ConvertToVector128Single(xmm0);
                    return xmm1.AsComplexF();
                }
#endif
                return new ComplexF((float)value.Real, (float)value.Imaginary);
            }
        }

        #endregion Explicit Conversions

        #region Functions

        /// <summary>
        /// Gets the absolute value (or magnitude) of a complex number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The absolute value of <paramref name="value"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Abs(ComplexF value) => value.value.Length();

        /// <summary>
        /// Computes the conjugate of a complex number and returns the result.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The conjugate of <paramref name="value"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF Conjugate(ComplexF value)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {

                }
#endif
                return new(value.Real, -value.Imaginary);
            }
        }

        /// <summary>
        /// Returns the multiplicative inverse of a complex number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The reciprocal of <paramref name="value"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF Reciprocal(ComplexF value) => One / value;

        /// <summary>
        /// Adds two complex numbers and returns the result.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>
        /// The sum of <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF Add(ComplexF left, ComplexF right) => left + right;

        /// <summary>
        /// Subtracts a complex number from another complex number and returns the result.
        /// </summary>
        /// <param name="left">The value to subtract from (the minuend).</param>
        /// <param name="right">The value to subtract (the subtrahend).</param>
        /// <returns>
        /// The result of subtracting <paramref name="right"/> from <paramref name="left"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF Subtract(ComplexF left, ComplexF right) => left - right;

        /// <summary>
        /// Multiplies two specified complex numbers.
        /// </summary>
        /// <param name="left">The first value to multiply.</param>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>
        /// The product of <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF Multiply(ComplexF left, ComplexF right) => left * right;

        /// <summary>
        /// Divides a specified complex number by another specified complex number.
        /// </summary>
        /// <param name="left">The value to be divided.</param>
        /// <param name="right">The value to divide by.</param>
        /// <returns>
        /// The result of dividing <paramref name="left"/> by <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF Divide(ComplexF left, ComplexF right) => left / right;

        /// <summary>
        /// Returns the additive inverse of a specified complex number.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>
        /// The result of the <see cref="Real"/> and <see cref="Imaginary"/> components of the value parameter multiplied by -1.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF Negate(ComplexF value) => -value;

        /// <summary>
        /// Creates a complex number from a point's polar coordinates.
        /// </summary>
        /// <param name="magnitude">The magnitude, which is the distance from the origin (the intersection of the x-axis and the y-axis) to the number.</param>
        /// <param name="phase">The phase, which is the angle from the line to the horizontal axis, measured in radians.</param>
        /// <returns>A complex number.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF FromPolarCoordinates(double magnitude, double phase) => (ComplexF)Complex.FromPolarCoordinates(magnitude, phase);

        /// <summary>
        /// Creates a complex number from a point's polar coordinates.
        /// </summary>
        /// <param name="magnitude">The magnitude, which is the distance from the origin (the intersection of the x-axis and the y-axis) to the number.</param>
        /// <param name="phase">The phase, which is the angle from the line to the horizontal axis, measured in radians.</param>
        /// <returns>A complex number.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF FromPolarCoordinates(float magnitude, float phase) => (ComplexF)Complex.FromPolarCoordinates(magnitude, phase);

        /// <summary>
        /// Returns the square root of a specified complex number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The square root of <paramref name="value"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ComplexF Sqrt(ComplexF value) => (ComplexF)Complex.Sqrt(value);

        #endregion Functions
        #region Conversion
#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static Vector128<float> AsVector128(in ComplexF value) => value.value.AsVector128Unsafe();
#endif
#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Reinterprets a <see cref="Vector128{T}"/> as a new <see cref="ComplexF"/>.
        /// </summary>
        /// <param name="value">The vector to reinterpret.</param>
        /// <returns><paramref name="value"/> reinterpreted as a new <see cref="Vector4"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static ComplexF AsComplexF(in Vector128<float> value) => new(value);
#endif
        /// <summary>
        /// Reinterprets a <see cref="Vector2"/> as a new <see cref="ComplexF"/>.
        /// </summary>
        /// <param name="value">The vector to reinterpret.</param>
        /// <returns><paramref name="value"/> reinterpreted as a new <see cref="ComplexF"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static ComplexF AsComplexF(in Vector2 value) => new(value);

        private string GetDebuggerDisplay() => $"<{Real:R}, {Imaginary:R}>";
        #endregion
    }
}
