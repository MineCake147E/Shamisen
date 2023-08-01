using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils;

namespace Shamisen.Memory
{
    /// <summary>
    /// Represents a native array, that can hold more data than <see cref="Array"/>.
    /// </summary>
    /// <typeparam name="T">The type of items in the <see cref="NativeArray{T}"/>.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe sealed class NativeArray<T> : IDisposable
    {
        private void* head;
        /// <summary>
        /// The length of this <see cref="NativeArray{T}"/>.
        /// </summary>
        public nint Length
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get;
            private set;
        }
        private NativeArrayConfig flags;
        [Flags]
        private enum NativeArrayConfig : byte
        {
            None = 0,
            AlignmentExponent0 = 0x01,
            AlignmentExponent1 = 1 << 1,
            AlignmentExponent2 = 1 << 2,
            AlignmentExponent3 = 1 << 3,
            AlignmentExponent4 = 1 << 4,
            AlignmentExponent5 = 1 << 5,
            AlignmentExponent6 = 1 << 6,
            MemoryPressure = 1 << 7,
            Alignment = AlignmentExponent0 | AlignmentExponent1 | AlignmentExponent2 | AlignmentExponent3 | AlignmentExponent4 | AlignmentExponent5 | AlignmentExponent6
        }

        /// <summary>
        /// Returns the value that indicates whether the current <see cref="NativeArray{T}"/> is disposed or not.
        /// </summary>
        public bool IsDisposed => Length < 0;

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="NativeArray{T}"/> should perform <see cref="GC.RemoveMemoryPressure(long)"/> when freeing.
        /// </summary>
        public bool MemoryPressure
        {
            get => (flags & NativeArrayConfig.MemoryPressure) != NativeArrayConfig.None;
            init
            {
                var f = flags;
                f &= ~NativeArrayConfig.MemoryPressure;
                f |= NativeArrayConfig.MemoryPressure & (NativeArrayConfig)(-Unsafe.As<bool, byte>(ref value));
                flags = f;
            }
        }

        /// <summary>
        /// Gets the current alignment in bytes.
        /// </summary>
        public nint CurrentAlignment => (nint)1 << BitOperations.TrailingZeroCount((nuint)head);

        /// <summary>
        /// Gets the initially desired alignment in bytes.
        /// </summary>
        public nint RequestedAlignment => (nint)1 << (byte)(flags & NativeArrayConfig.Alignment);

        /// <summary>
        /// Creates a new <see cref="NativeSpan{T}"/> object over the entirety of the current <see cref="NativeArray{T}"/>.
        /// </summary>
        public NativeSpan<T> NativeSpan => new(head, Length);

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="NativeArray{T}"/> is empty.
        /// </summary>
        public bool IsEmpty => Length <= 0;

        /// <summary>
        /// Initializes a new empty instance of the <see cref="NativeArray{T}"/> struct.
        /// </summary>
        public NativeArray()
        {
            head = null;
            Length = -1;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeArray{T}"/> struct.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="alignmentExponent">The number of zeros to be at lowest significant bits.</param>
        /// <param name="memoryPressure">The value which indicates whether the <see cref="NativeArray{T}"/> should perform <see cref="GC.AddMemoryPressure(long)"/> when allocating.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public NativeArray(nint length, byte alignmentExponent = 0, bool memoryPressure = true)
        {
            if (!UnsafeUtils.ValidateAllocationRequest<T>(length, out var lengthInBytes, alignmentExponent, (byte)NativeArrayConfig.Alignment))
            {
                head = null;
                Length = -1;
                GC.SuppressFinalize(this);
                return;
            }
            flags = (NativeArrayConfig)alignmentExponent & NativeArrayConfig.Alignment;
            MemoryPressure = memoryPressure;
            head = UnsafeUtils.AllocateNativeMemoryInternal(lengthInBytes, alignmentExponent, memoryPressure);
            Length = length;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
#pragma warning disable VSSpell001 // Spell Check
        public ref T GetPinnableReference() => ref Unsafe.AsRef<T>(head);
#pragma warning restore VSSpell001 // Spell Check

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
#pragma warning disable RCS1233 // Use short-circuiting operator.
                if (head is not null & Length > 0)
                {
                    if (MemoryPressure)
                        UnsafeUtils.RemoveMemoryPressure((nuint)Length * (nuint)Unsafe.SizeOf<T>());
                    if (RequestedAlignment > 1)
                        NativeMemory.AlignedFree(head);
                    else
                    {
                        NativeMemory.Free(head);
                    }
                    head = null;
                }
#pragma warning restore RCS1233 // Use short-circuiting operator.
                Length = -1;
            }
        }

        /// <inheritdoc cref="object.Finalize()"/>
        ~NativeArray()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
