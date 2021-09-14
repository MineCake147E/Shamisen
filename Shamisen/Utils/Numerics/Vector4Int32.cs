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
    [StructLayout(LayoutKind.Explicit, Size = 4 * sizeof(int))]
    public readonly struct Vector4Int32
    {
#if NETCOREAPP3_1_OR_GREATER
        [FieldOffset(0)]
        private readonly Vector128<int> values;
        /// <summary>
        /// Gets the X value of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int X
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.GetElement(0);
        }
        /// <summary>
        /// Gets the Y value of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int Y
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.GetElement(1);
        }
        /// <summary>
        /// Gets the Z value of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int Z
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.GetElement(2);
        }
        /// <summary>
        /// Gets the W value of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int W
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => values.GetElement(3);
        }

#else   
        [FieldOffset(0)]
        private readonly int x;
        [FieldOffset(4)]
        private readonly int y;
        [FieldOffset(8)]
        private readonly int z;
        [FieldOffset(12)]
        private readonly int w;
        /// <summary>
        /// Gets the X value of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int X
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => x;
        }
        /// <summary>
        /// Gets the Y value of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int Y
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => y;
        }
        /// <summary>
        /// Gets the Z value of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int Z
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => z;
        }
        /// <summary>
        /// Gets the W value of this <see cref="Vector4Int32"/>.
        /// </summary>
        public int W
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => w;
        }
#endif
    }
}
