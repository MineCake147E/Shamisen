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
#if NET5_0_OR_GREATER
        internal static partial class Arm
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave5ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4)
            {
                Fallback.Interleave5ChannelsInt32(buffer, a0, a1, a2, a3, a4);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave6ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5)
            {
                Fallback.Interleave6ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave7ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5, ReadOnlySpan<int> a6)
            {
                Fallback.Interleave7ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave8ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5, ReadOnlySpan<int> a6, ReadOnlySpan<int> a7)
            {
                Fallback.Interleave8ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6, a7);
            }

        }
#endif
#if NETCOREAPP3_1_OR_GREATER
        internal static partial class X86
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave5ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4)
            {
                Fallback.Interleave5ChannelsInt32(buffer, a0, a1, a2, a3, a4);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave6ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5)
            {
                Fallback.Interleave6ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave7ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5, ReadOnlySpan<int> a6)
            {
                Fallback.Interleave7ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Interleave8ChannelsInt32(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5, ReadOnlySpan<int> a6, ReadOnlySpan<int> a7)
            {
                Fallback.Interleave8ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6, a7);
            }

        }
#endif
    }
}
