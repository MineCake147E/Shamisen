using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils
{
    /// <summary>
    /// Contains some utility functions for manipulating audio data.
    /// </summary>
    public static partial class AudioUtils
    {
        /// <summary>
        /// Interleaves and stores Stereo samples to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="left">The input buffer for left channel.</param>
        /// <param name="right">The input buffer for right channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void InterleaveStereo(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics

#endif
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.InterleaveStereoInt32(buffer, left, right);
                    return;
                }
#endif
                Fallback.InterleaveStereoInt32(buffer, left, right);
            }
        }

        /// <summary>
        /// Interleaves and stores Stereo samples to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="left">The input buffer for left channel.</param>
        /// <param name="right">The input buffer for right channel.</param>
        /// <param name="center">The input buffer for center channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void InterleaveThree(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.InterleaveThreeInt32(buffer, left, right, center);
                    return;
                }
#endif
                Fallback.InterleaveThreeInt32(buffer, left, right, center);
            }
        }

        /// <summary>
        /// Interleaves and stores Stereo samples to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="frontLeft">The input buffer for front left channel.</param>
        /// <param name="frontRight">The input buffer for front right channel.</param>
        /// <param name="rearLeft">The input buffer for rear left channel.</param>
        /// <param name="rearRight">The input buffer for rear right channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void InterleaveQuad(Span<int> buffer, ReadOnlySpan<int> frontLeft, ReadOnlySpan<int> frontRight, ReadOnlySpan<int> rearLeft, ReadOnlySpan<int> rearRight)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.InterleaveQuadInt32(buffer, frontLeft, frontRight, rearLeft, rearRight);
                    return;
                }
#endif
                Fallback.InterleaveQuadInt32(buffer, frontLeft, frontRight, rearLeft, rearRight);
            }
        }
    }
}
