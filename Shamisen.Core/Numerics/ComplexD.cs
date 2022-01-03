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
    public readonly struct ComplexD
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
    }
}
