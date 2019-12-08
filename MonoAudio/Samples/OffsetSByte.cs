using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Represents a value that is offset 128 inside 8-bit PCM.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    public readonly struct OffsetSByte : IEquatable<OffsetSByte>
    {
        private const byte Inverter = 0x80;

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
                byte vp = (byte)value;

                this.value = (byte)(vp ^ Inverter);
            }
        }

        public override bool Equals(object obj) => obj is OffsetSByte @byte && Equals(@byte);

        public bool Equals(OffsetSByte other) => value == other.value;

        public override int GetHashCode() => -1584136870 + value.GetHashCode();

        public static bool operator ==(OffsetSByte left, OffsetSByte right) => left.Equals(right);

        public static bool operator !=(OffsetSByte left, OffsetSByte right) => !(left == right);

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
                return (sbyte)(value.value ^ Inverter);
            }
        }
    }
}