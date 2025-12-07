using System.Runtime.CompilerServices;

namespace Shamisen.Codecs.Flac.Utils
{
    /// <summary>
    /// A 32-element fixed array of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    [InlineArray(32)]
    public struct FixedArray32<T>
    {
        private T element0;
    }

    /// <summary>
    /// A 16-element fixed array of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    [InlineArray(16)]
    public struct FixedArray16<T>
    {
        private T element0;
    }
}
