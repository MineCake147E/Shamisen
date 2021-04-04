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
                X86.InterleaveStereoInt32(buffer, left, right);
#else
                Fallback.InterleaveStereoInt32(buffer, left, right);
#endif
            }
        }
    }
}
