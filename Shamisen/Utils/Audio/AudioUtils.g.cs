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
        /// Interleaves and stores audio samples with 5 channels, to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="a0">The input buffer for channel No.0.</param>
		/// <param name="a1">The input buffer for channel No.1.</param>
		/// <param name="a2">The input buffer for channel No.2.</param>
		/// <param name="a3">The input buffer for channel No.3.</param>
		/// <param name="a4">The input buffer for channel No.4.</param>
        /// <param name="left">The input buffer for left channel.</param>
        /// <param name="right">The input buffer for right channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void Interleave5Channels(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics
                if(Arm.IsSupported)
                {
                    Arm.Interleave5ChannelsInt32(buffer, a0, a1, a2, a3, a4);
                    return;
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if(X86.IsSupported)
                {
                    X86.Interleave5ChannelsInt32(buffer, a0, a1, a2, a3, a4);
                    return;
                }
#endif
                Fallback.Interleave5ChannelsInt32(buffer, a0, a1, a2, a3, a4);
            }
        }
        /// <summary>
        /// Interleaves and stores audio samples with 6 channels, to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="a0">The input buffer for channel No.0.</param>
		/// <param name="a1">The input buffer for channel No.1.</param>
		/// <param name="a2">The input buffer for channel No.2.</param>
		/// <param name="a3">The input buffer for channel No.3.</param>
		/// <param name="a4">The input buffer for channel No.4.</param>
		/// <param name="a5">The input buffer for channel No.5.</param>
        /// <param name="left">The input buffer for left channel.</param>
        /// <param name="right">The input buffer for right channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void Interleave6Channels(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics
                if(Arm.IsSupported)
                {
                    Arm.Interleave6ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5);
                    return;
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if(X86.IsSupported)
                {
                    X86.Interleave6ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5);
                    return;
                }
#endif
                Fallback.Interleave6ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5);
            }
        }
        /// <summary>
        /// Interleaves and stores audio samples with 7 channels, to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="a0">The input buffer for channel No.0.</param>
		/// <param name="a1">The input buffer for channel No.1.</param>
		/// <param name="a2">The input buffer for channel No.2.</param>
		/// <param name="a3">The input buffer for channel No.3.</param>
		/// <param name="a4">The input buffer for channel No.4.</param>
		/// <param name="a5">The input buffer for channel No.5.</param>
		/// <param name="a6">The input buffer for channel No.6.</param>
        /// <param name="left">The input buffer for left channel.</param>
        /// <param name="right">The input buffer for right channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void Interleave7Channels(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5, ReadOnlySpan<int> a6)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics
                if(Arm.IsSupported)
                {
                    Arm.Interleave7ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6);
                    return;
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if(X86.IsSupported)
                {
                    X86.Interleave7ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6);
                    return;
                }
#endif
                Fallback.Interleave7ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6);
            }
        }
        /// <summary>
        /// Interleaves and stores audio samples with 8 channels, to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="a0">The input buffer for channel No.0.</param>
		/// <param name="a1">The input buffer for channel No.1.</param>
		/// <param name="a2">The input buffer for channel No.2.</param>
		/// <param name="a3">The input buffer for channel No.3.</param>
		/// <param name="a4">The input buffer for channel No.4.</param>
		/// <param name="a5">The input buffer for channel No.5.</param>
		/// <param name="a6">The input buffer for channel No.6.</param>
		/// <param name="a7">The input buffer for channel No.7.</param>
        /// <param name="left">The input buffer for left channel.</param>
        /// <param name="right">The input buffer for right channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void Interleave8Channels(Span<int> buffer, ReadOnlySpan<int> a0, ReadOnlySpan<int> a1, ReadOnlySpan<int> a2, ReadOnlySpan<int> a3, ReadOnlySpan<int> a4, ReadOnlySpan<int> a5, ReadOnlySpan<int> a6, ReadOnlySpan<int> a7)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics
                if(Arm.IsSupported)
                {
                    Arm.Interleave8ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6, a7);
                    return;
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if(X86.IsSupported)
                {
                    X86.Interleave8ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6, a7);
                    return;
                }
#endif
                Fallback.Interleave8ChannelsInt32(buffer, a0, a1, a2, a3, a4, a5, a6, a7);
            }
        }
    }
}
