using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Audio.OpenAL;

namespace MonoAudio.IO
{
    /// <summary>
    /// Represents an <see cref="AL"/> Attribute Key and Value pair.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = sizeof(int) * 2)]
    public readonly struct ALAttribute<TKey> where TKey : unmanaged
    {
        [FieldOffset(0)]
        private readonly int keyInternal;

        /// <summary>
        /// The key of this <see cref="ALAttribute{TKey}"/>.
        /// </summary>
        [FieldOffset(0)]
        public readonly TKey Key;

        /// <summary>
        /// The value of this <see cref="ALAttribute{TKey}"/>.
        /// </summary>
        [FieldOffset(4)]
        public readonly int Value;
    }
}
