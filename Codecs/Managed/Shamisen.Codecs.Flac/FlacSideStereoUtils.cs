using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Contains some utilities for side stereo samples in FLAC.
    /// </summary>
    public static partial class FlacSideStereoUtils
    {
        /// <summary>
        /// Decodes and interleaves the left side stereo samples.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <exception cref="ArgumentException">
        /// right must be as long as left! - right
        /// or
        /// buffer must be twice as long as left!
        /// </exception>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void DecodeAndInterleaveLeftSideStereo(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (X86.IsSupported)
            {
                X86.DecodeAndInterleaveLeftSideStereoInt32(buffer, left, right);
                return;
            }
#endif
            Fallback.DecodeAndInterleaveLeftSideStereoInt32(buffer, left, right);
        }

        /// <summary>
        /// Decodes and interleaves the right side stereo samples.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <exception cref="ArgumentException">
        /// right must be as long as left! - right
        /// or
        /// buffer must be twice as long as left!
        /// </exception>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void DecodeAndInterleaveRightSideStereo(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (X86.IsSupported)
            {
                X86.DecodeAndInterleaveRightSideStereoInt32(buffer, left, right);
                return;
            }
#endif
            Fallback.DecodeAndInterleaveRightSideStereoInt32(buffer, left, right);
        }

        /// <summary>
        /// Decodes and interleaves the mid side stereo samples.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <exception cref="ArgumentException">
        /// right must be as long as left! - right
        /// or
        /// buffer must be twice as long as left!
        /// </exception>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void DecodeAndInterleaveMidSideStereo(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (X86.IsSupported)
            {
                X86.DecodeAndInterleaveMidSideStereoInt32(buffer, left, right);
                return;
            }
#endif
            Fallback.DecodeAndInterleaveMidSideStereoInt32(buffer, left, right);
        }
    }
}
