using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils.Tuples;

namespace Shamisen.Utils
{
    public static partial class AudioUtils
    {
        internal static partial class Fallback
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave5ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4)
            {
                unsafe
                {
                    const int Channels = 5;
                    if (a1.Length < a0.Length) throw new ArgumentException("a1 must be as long as a0!", nameof(a1));
					if (a2.Length < a0.Length) throw new ArgumentException("a2 must be as long as a0!", nameof(a2));
					if (a3.Length < a0.Length) throw new ArgumentException("a3 must be as long as a0!", nameof(a3));
					if (a4.Length < a0.Length) throw new ArgumentException("a4 must be as long as a0!", nameof(a4));

                    //These pre-touches may avoid some range checks
                    ref var rA0 = ref MemoryMarshal.GetReference(a0);
					ref var rA1 = ref MemoryMarshal.GetReference(a1);
					ref var rA2 = ref MemoryMarshal.GetReference(a2);
					ref var rA3 = ref MemoryMarshal.GetReference(a3);
					ref var rA4 = ref MemoryMarshal.GetReference(a4);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = a0.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        var a = Unsafe.Add(ref rA0, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rA1, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
						a = Unsafe.Add(ref rA2, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
						a = Unsafe.Add(ref rA3, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
						a = Unsafe.Add(ref rA4, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rA0, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 5) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA1, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 6) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA2, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 7) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA3, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 8) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA4, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 9) = a;
                        j += Channels * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        var a = Unsafe.Add(ref rA0, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rA1, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
						a = Unsafe.Add(ref rA2, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
						a = Unsafe.Add(ref rA3, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
						a = Unsafe.Add(ref rA4, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
                        j += Channels;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave6ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5)
            {
                unsafe
                {
                    const int Channels = 6;
                    if (a1.Length < a0.Length) throw new ArgumentException("a1 must be as long as a0!", nameof(a1));
					if (a2.Length < a0.Length) throw new ArgumentException("a2 must be as long as a0!", nameof(a2));
					if (a3.Length < a0.Length) throw new ArgumentException("a3 must be as long as a0!", nameof(a3));
					if (a4.Length < a0.Length) throw new ArgumentException("a4 must be as long as a0!", nameof(a4));
					if (a5.Length < a0.Length) throw new ArgumentException("a5 must be as long as a0!", nameof(a5));

                    //These pre-touches may avoid some range checks
                    ref var rA0 = ref MemoryMarshal.GetReference(a0);
					ref var rA1 = ref MemoryMarshal.GetReference(a1);
					ref var rA2 = ref MemoryMarshal.GetReference(a2);
					ref var rA3 = ref MemoryMarshal.GetReference(a3);
					ref var rA4 = ref MemoryMarshal.GetReference(a4);
					ref var rA5 = ref MemoryMarshal.GetReference(a5);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = a0.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        var a = Unsafe.Add(ref rA0, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rA1, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
						a = Unsafe.Add(ref rA2, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
						a = Unsafe.Add(ref rA3, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
						a = Unsafe.Add(ref rA4, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
						a = Unsafe.Add(ref rA5, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 5) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rA0, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 6) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA1, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 7) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA2, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 8) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA3, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 9) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA4, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 10) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA5, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 11) = a;
                        j += Channels * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        var a = Unsafe.Add(ref rA0, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rA1, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
						a = Unsafe.Add(ref rA2, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
						a = Unsafe.Add(ref rA3, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
						a = Unsafe.Add(ref rA4, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
						a = Unsafe.Add(ref rA5, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 5) = a;
                        j += Channels;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave7ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5, ReadOnlySpan<int> a6)
            {
                unsafe
                {
                    const int Channels = 7;
                    if (a1.Length < a0.Length) throw new ArgumentException("a1 must be as long as a0!", nameof(a1));
					if (a2.Length < a0.Length) throw new ArgumentException("a2 must be as long as a0!", nameof(a2));
					if (a3.Length < a0.Length) throw new ArgumentException("a3 must be as long as a0!", nameof(a3));
					if (a4.Length < a0.Length) throw new ArgumentException("a4 must be as long as a0!", nameof(a4));
					if (a5.Length < a0.Length) throw new ArgumentException("a5 must be as long as a0!", nameof(a5));
					if (a6.Length < a0.Length) throw new ArgumentException("a6 must be as long as a0!", nameof(a6));

                    //These pre-touches may avoid some range checks
                    ref var rA0 = ref MemoryMarshal.GetReference(a0);
					ref var rA1 = ref MemoryMarshal.GetReference(a1);
					ref var rA2 = ref MemoryMarshal.GetReference(a2);
					ref var rA3 = ref MemoryMarshal.GetReference(a3);
					ref var rA4 = ref MemoryMarshal.GetReference(a4);
					ref var rA5 = ref MemoryMarshal.GetReference(a5);
					ref var rA6 = ref MemoryMarshal.GetReference(a6);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = a0.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        var a = Unsafe.Add(ref rA0, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rA1, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
						a = Unsafe.Add(ref rA2, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
						a = Unsafe.Add(ref rA3, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
						a = Unsafe.Add(ref rA4, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
						a = Unsafe.Add(ref rA5, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 5) = a;
						a = Unsafe.Add(ref rA6, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 6) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rA0, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 7) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA1, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 8) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA2, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 9) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA3, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 10) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA4, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 11) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA5, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 12) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA6, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 13) = a;
                        j += Channels * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        var a = Unsafe.Add(ref rA0, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rA1, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
						a = Unsafe.Add(ref rA2, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
						a = Unsafe.Add(ref rA3, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
						a = Unsafe.Add(ref rA4, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
						a = Unsafe.Add(ref rA5, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 5) = a;
						a = Unsafe.Add(ref rA6, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 6) = a;
                        j += Channels;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave8ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5, ReadOnlySpan<int> a6, ReadOnlySpan<int> a7)
            {
                unsafe
                {
                    const int Channels = 8;
                    if (a1.Length < a0.Length) throw new ArgumentException("a1 must be as long as a0!", nameof(a1));
					if (a2.Length < a0.Length) throw new ArgumentException("a2 must be as long as a0!", nameof(a2));
					if (a3.Length < a0.Length) throw new ArgumentException("a3 must be as long as a0!", nameof(a3));
					if (a4.Length < a0.Length) throw new ArgumentException("a4 must be as long as a0!", nameof(a4));
					if (a5.Length < a0.Length) throw new ArgumentException("a5 must be as long as a0!", nameof(a5));
					if (a6.Length < a0.Length) throw new ArgumentException("a6 must be as long as a0!", nameof(a6));
					if (a7.Length < a0.Length) throw new ArgumentException("a7 must be as long as a0!", nameof(a7));

                    //These pre-touches may avoid some range checks
                    ref var rA0 = ref MemoryMarshal.GetReference(a0);
					ref var rA1 = ref MemoryMarshal.GetReference(a1);
					ref var rA2 = ref MemoryMarshal.GetReference(a2);
					ref var rA3 = ref MemoryMarshal.GetReference(a3);
					ref var rA4 = ref MemoryMarshal.GetReference(a4);
					ref var rA5 = ref MemoryMarshal.GetReference(a5);
					ref var rA6 = ref MemoryMarshal.GetReference(a6);
					ref var rA7 = ref MemoryMarshal.GetReference(a7);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = a0.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        var a = Unsafe.Add(ref rA0, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rA1, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
						a = Unsafe.Add(ref rA2, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
						a = Unsafe.Add(ref rA3, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
						a = Unsafe.Add(ref rA4, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
						a = Unsafe.Add(ref rA5, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 5) = a;
						a = Unsafe.Add(ref rA6, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 6) = a;
						a = Unsafe.Add(ref rA7, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 7) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rA0, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 8) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA1, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 9) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA2, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 10) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA3, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 11) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA4, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 12) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA5, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 13) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA6, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 14) = a;
						a = Unsafe.Add(ref Unsafe.Add(ref rA7, i), 1);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 15) = a;
                        j += Channels * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        var a = Unsafe.Add(ref rA0, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rA1, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
						a = Unsafe.Add(ref rA2, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
						a = Unsafe.Add(ref rA3, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
						a = Unsafe.Add(ref rA4, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
						a = Unsafe.Add(ref rA5, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 5) = a;
						a = Unsafe.Add(ref rA6, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 6) = a;
						a = Unsafe.Add(ref rA7, i);
						Unsafe.Add(ref Unsafe.Add(ref rB, j), 7) = a;
                        j += Channels;
                    }
                }
            }

        }
    }
}
