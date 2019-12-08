using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;

namespace MonoAudio
{
    /// <summary>
    /// Provides some functions that helps you about binary things.
    /// </summary>
    public static class BinaryExtensions
    {
        /// <summary>
        /// Reverses internal primitive values by performing an endianness swap of the specified <see cref="Guid"/> <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static Guid ReverseEndianness(Guid value)
        {
            var q = Unsafe.As<Guid, (uint, ushort, ushort, byte, byte, byte, byte, byte, byte, byte, byte)>(ref value);
            q.Item1 = BinaryPrimitives.ReverseEndianness(q.Item1);
            q.Item2 = BinaryPrimitives.ReverseEndianness(q.Item2);
            q.Item3 = BinaryPrimitives.ReverseEndianness(q.Item3);
            return new Guid(q.Item1, q.Item2, q.Item3, q.Item4, q.Item5, q.Item6, q.Item7, q.Item8, q.Item9, q.Item10, q.Item11);
        }
    }
}
