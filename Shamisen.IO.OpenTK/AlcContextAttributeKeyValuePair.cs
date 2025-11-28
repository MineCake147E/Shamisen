using System.Runtime.InteropServices;

using OpenTK.Audio.OpenAL.ALC;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Stores a key-value pair for context attributes.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct AlcContextAttributeKeyValuePair
    {
        /// <summary>
        /// The key for this context attribute.
        /// </summary>
        [field: FieldOffset(0)]
        public ContextAttribute Key { get; }
        /// <summary>
        /// The value for this context attribute.
        /// </summary>
        [field: FieldOffset(sizeof(int))]
        public int Value { get; }
    }
}
