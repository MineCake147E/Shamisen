using System;

namespace Shamisen.Utils
{
    /// <summary>
    ///
    /// </summary>
    public static class EndiannessExtensions
    {
        /// <summary>
        /// Gets the environment's endianness.
        /// </summary>
        /// <value>
        /// The environment endianness.
        /// </value>
        public static readonly Endianness EnvironmentEndianness = BitConverter.IsLittleEndian ? Endianness.Little : Endianness.Big;
    }
}
