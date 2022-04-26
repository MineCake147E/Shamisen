using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Numerics
{
    /// <summary>
    /// Represents a vector with 2 <see cref="short"/> values.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Vector2Int16
    {
        [FieldOffset(0)]
        private readonly short x;
        [FieldOffset(sizeof(short))]
        private readonly short y;

        /// <summary>
        /// Gets the x value of this <see cref="Vector2Int16"/>.
        /// </summary>
        /// <value>
        /// The x value of this <see cref="Vector2Int16"/>.
        /// </value>
        public short X => x;
        /// <summary>
        /// Gets the y value of this <see cref="Vector2Int16"/>.
        /// </summary>
        /// <value>
        /// The y value of this <see cref="Vector2Int16"/>.
        /// </value>
        public short Y => y;
    }
}
