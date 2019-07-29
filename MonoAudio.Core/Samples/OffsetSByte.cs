using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    /// <summary>
    /// Represents a value that is offset 128 inside 8-bit PCM.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    public readonly struct OffsetSByte
    {
        private const byte inverter = 0x80;

        [FieldOffset(0)]
        private readonly byte value;

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetSByte"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public OffsetSByte(sbyte value)
        {
            unchecked
            {
                byte vp = Unsafe.As<sbyte, byte>(ref value);
                this.value = (byte)(vp ^ inverter);
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="OffsetSByte"/> to <see cref="sbyte"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator sbyte(OffsetSByte value)
        {
            unchecked
            {
                return (sbyte)(value.value ^ inverter);
            }
        }
    }
}
