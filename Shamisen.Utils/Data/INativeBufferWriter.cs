using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data
{
    public interface INativeBufferWriter<T> : IBufferWriter<T>
    {
        /// <summary>
        /// Notifies the <see cref="INativeBufferWriter{T}"/> that <paramref name="count"/> data items were written to the output <see cref="NativeSpan{T}"/> or <see cref="Memory{T}"/>.
        /// </summary>
        /// <param name="count">The number of data items written to the <see cref="NativeSpan{T}"/> or <see cref="Memory{T}"/>.</param>
        /// <remarks>You must request a new buffer after calling <see cref="Advance(nint)"/> to continue writing more data; you cannot write to a previously acquired buffer.</remarks>
        void Advance(nint count);

        /// <summary>
        /// Returns a <see cref="NativeSpan{T}"/> to write to that is at least the requested size (specified by <paramref name="sizeHint"/>).
        /// </summary>
        /// <param name="sizeHint">The minimum length of the returned <see cref="NativeSpan{T}"/>. If 0, a non-empty buffer is returned.</param>
        /// <returns>A <see cref="NativeSpan{T}"/> of at least the size <paramref name="sizeHint"/>. If <paramref name="sizeHint"/> is 0, returns a non-empty buffer.</returns>
        /// <remarks>
        /// There is no guarantee that successive calls will return the same buffer or the same-sized buffer.<br/>
        /// Unlike <see cref="IBufferWriter{T}.GetSpan(int)"/>, this method must ALWAYS return <see cref="NativeSpan{T}.Empty"/> if the requested buffer size is not available.<br/>
        /// You must request a new buffer after calling <see cref="Advance(nint)"/> to continue writing more data; you cannot write to a previously acquired buffer.
        /// </remarks>
        NativeSpan<T> GetNativeSpan(nint sizeHint = 0);
    }
}
