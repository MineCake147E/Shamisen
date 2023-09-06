using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Buffers
{
    /// <summary>
    /// Represents a pool-based, array-backed output sink into which <typeparamref name="T"/> data can be written.<br/>
    /// Heavily inspired by <see cref="ArrayBufferWriter{T}"/>.
    /// </summary>
    public sealed class PooledArrayBufferWriter<T> : IBufferWriter<T>, IDisposable
    {
        private PooledArray<T>? buffer;

        private int index;
        private bool disposedValue;

        /// <inheritdoc cref="ArrayBufferWriter{T}.WrittenCount"/>
        public int WrittenCount => index;

        /// <inheritdoc cref="ArrayBufferWriter{T}.Capacity"/>
        public int Capacity => buffer?.Length ?? 0;

        /// <inheritdoc cref="ArrayBufferWriter{T}.FreeCapacity"/>
        public int FreeCapacity => Capacity - WrittenCount;

        /// <inheritdoc cref="ArrayBufferWriter{T}.WrittenSpan"/>
        public ReadOnlySpan<T> WrittenSpan => IsEmpty ? ReadOnlySpan<T>.Empty : buffer.Span.SliceWhile(index);

        /// <inheritdoc cref="ArrayBufferWriter{T}.WrittenMemory"/>
        public ReadOnlyMemory<T> WrittenMemory => IsEmpty ? ReadOnlyMemory<T>.Empty : buffer.Memory.SliceWhile(index);

        /// <summary>
        /// Gets the value which indicates whether the <see cref="PooledArrayBufferWriter{T}"/> is empty or not.
        /// </summary>
        [MemberNotNullWhen(false, nameof(buffer))]
        public bool IsEmpty => buffer is null || index < 1;

        /// <summary>
        /// 
        /// </summary>
        public PooledArrayBufferWriter()
        {
            buffer = null;
            index = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialCapacity"></param>
        public PooledArrayBufferWriter(int initialCapacity)
        {
            buffer = initialCapacity > 0 ? new(initialCapacity) : null;
            index = 0;
        }
        /// <inheritdoc cref="ArrayBufferWriter{T}.Advance(int)"/>
        public void Advance(int count)
        {
            if (count < 0) throw new ArgumentException(null, nameof(count));
            if (buffer is null || FreeCapacity < count)
            {
                throw new InvalidOperationException($"{nameof(count)} should be less than {nameof(FreeCapacity)}!");
            }
            index += count;
        }

        /// <inheritdoc cref="ArrayBufferWriter{T}.GetMemory(int)"/>
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            if (!CheckAndResizeBuffer(sizeHint))
            {
                Debug.Fail("CheckAndResizeBuffer somehow failed!");
                throw null;
            }
            return buffer.Memory.Slice(index);
        }
        /// <inheritdoc cref="ArrayBufferWriter{T}.GetSpan(int)"/>
        public Span<T> GetSpan(int sizeHint = 0)
        {
            if (!CheckAndResizeBuffer(sizeHint))
            {
                Debug.Fail("CheckAndResizeBuffer somehow failed!");
                throw null;
            }
            return buffer.Span.Slice(index);
        }

        [MemberNotNullWhen(true, nameof(buffer))]
        private bool CheckAndResizeBuffer(int sizeHint)
        {
            if (sizeHint < 0) throw new ArgumentException(null, nameof(sizeHint));
            if (sizeHint == 0) sizeHint = 1;

            if (FreeCapacity < sizeHint)
            {
                ResizeBuffer(sizeHint);
            }
            Debug.Assert(FreeCapacity >= sizeHint);
            return buffer is not null;
        }

        private void ResizeBuffer(int sizeHint)
        {
            var capacity = Capacity;
            var growth = MathI.Max(sizeHint, capacity);

            if (capacity == 0)
            {
                growth = (int)BitOperations.RoundUpToPowerOf2((uint)growth);
            }

            var newSize = capacity + growth;
            if (newSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeHint), "Maximum allocation size exceeded!");
            }
            var newBuffer = new PooledArray<T>(newSize);
            if (!IsEmpty)
            {
                var current = WrittenSpan;
                current.CopyTo(newBuffer.Span);
                buffer.Dispose();
                buffer = null;
            }
            buffer = newBuffer;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    buffer?.Dispose();
                }
                buffer = null;
                index = 0;
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
