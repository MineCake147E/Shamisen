using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils
{
    /// <summary>
    /// Wraps the <typeparamref name="T"/>* pointer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly unsafe struct Pointer<T>(T* raw) where T : unmanaged
    {
        /// <summary>
        /// Gets the raw pointer to the <typeparamref name="T"/> type.
        /// </summary>
        public unsafe T* Raw { get; } = raw;

        /// <summary>
        /// Performs an implicit conversion from <typeparamref name="T"/>* to <see cref="Pointer{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator Pointer<T>(T* value) => new(value);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Pointer{T}"/> to <typeparamref name="T"/>*.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator T*(Pointer<T> value) => value.Raw;
    }

    /// <summary>
    /// Utility class for creating <see cref="Pointer{T}"/> instances.
    /// </summary>
    public static class Pointer
    {
        /// <summary>
        /// Wraps the <typeparamref name="T"/>* pointer value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="raw"></param>
        /// <returns></returns>
        public static unsafe Pointer<T> Create<T>(T* raw) where T : unmanaged => new(raw);
    }
}
