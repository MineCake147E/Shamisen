using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shamisen.Utils
{
    /// <summary>
    /// Contains some utility functions for manipulating audio data.
    /// </summary>
    public static partial class AudioUtils
    {
        #region Interleave

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
        #endregion

        #region DuplicateMonauralToStereo


        /// <summary>
        /// Duplicates specified monaural signal <paramref name="source"/> to the specified stereo <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public static void DuplicateMonauralToStereo(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 2);
            source = source.SliceWhileIfLongerThan(destination.Length / 2);
#if NETCOREAPP3_1_OR_GREATER
            if (X86.IsSupported)
            {
                X86.DuplicateMonauralToStereo(destination, source);
                return;
            }
#endif
            Fallback.DuplicateMonauralToStereo(destination, source);
        }


        #endregion

        #region DuplicateMonauralToChannels

        /// <summary>
        /// Duplicates specified monaural samples <paramref name="source"/> to <paramref name="channels"/>, and writes to <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The multi-channel destination.</param>
        /// <param name="source">The monaural source.</param>
        /// <param name="channels">The number of channels.</param>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public static void DuplicateMonauralToChannels(Span<float> destination, ReadOnlySpan<float> source, int channels)
        {
            switch (channels)
            {
                case < 1:
                case 1:
                    return;
                case 2:
                    {
                        if (Vector<float>.Count == 2) goto default;
                        DuplicateMonauralToStereo(destination, source);
                        return;
                    }
                case 3:
                    {
                        destination = destination.SliceWhileIfLongerThan(source.Length * 3);
                        source = source.SliceWhileIfLongerThanWithLazyDivide(destination.Length, 3);
                        var bispan = MemoryMarshal.Cast<float, int>(source);
                        InterleaveThree(MemoryMarshal.Cast<float, int>(destination), bispan, bispan, bispan);
                        return;
                    }
                case 4:
                    {
                        if (Vector<float>.Count == 4) goto default;
                        DuplicateMonauralToQuad(destination, source);
                        return;
                    }
                case 8:
                    {
                        if (Vector<float>.Count == 8) goto default;
                        DuplicateMonauralToOctaple(destination, source);
                        return;
                    }
                case 12:
                    {
                        DuplicateMonauralTo12Channels(destination, source);
                        return;
                    }
                case 16:
                    {
                        if (Vector<float>.Count == channels) goto default;
                        DuplicateMonauralTo16Channels(destination, source);
                        return;
                    }
                case < 16:
                    {
                        if (Vector<float>.Count == channels) goto default;
                        int h = 0;
                        ref var dst = ref MemoryMarshal.GetReference(destination);
                        for (int i = 0; i < source.Length; i++, h += channels)
                        {
                            var v4v = new Vector4(source[i]);
                            int j = 0;
                            var olen = channels - 3;
                            for (; j < olen; j += 4)
                            {
                                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + j)) = v4v;
                            }
                            for (; j < channels; j++)
                            {
                                Unsafe.Add(ref dst, h + j) = v4v.X;
                            }
                        }
                        return;
                    }
                default:
                    {
                        if (Vector<float>.Count == channels)
                        {
                            int h = 0;
                            ref var dst = ref MemoryMarshal.GetReference(destination);
                            for (int i = 0; i < source.Length; i++, h += channels)
                            {
                                var v4v = new Vector<float>(source[i]);
                                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref dst, h)) = v4v;
                            }
                            return;
                        }
                        else
                        {
                            int h = 0;
                            for (int i = 0; i < source.Length; i++, h += channels)
                            {
                                var value = source[i];
                                destination.Slice(h, channels).FastFill(value);
                            }
                            return;
                        }
                    }
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void DuplicateMonauralTo16Channels(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 16);
            source = source.SliceWhileIfLongerThan(destination.Length / 16);
            int h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (int i = 0; i < source.Length; i++, h += 16)
            {
                var v4v = new Vector4(source[i]);
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 4)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 8)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 12)) = v4v;
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void DuplicateMonauralTo12Channels(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 12);
            source = source.SliceWhileIfLongerThanWithLazyDivide(destination.Length, 12);
            int h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (int i = 0; i < source.Length; i++, h += 12)
            {
                var v4v = new Vector4(source[i]);
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 4)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 8)) = v4v;
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void DuplicateMonauralToOctaple(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 8);
            source = source.SliceWhileIfLongerThan(destination.Length / 8);
            int h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (int i = 0; i < source.Length; i++, h += 8)
            {
                var v4v = new Vector4(source[i]);
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 4)) = v4v;
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void DuplicateMonauralToQuad(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 4);
            source = source.SliceWhileIfLongerThan(destination.Length / 4);
            int h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (int i = 0; i < source.Length; i++, h += 4)
            {
                var v4v = new Vector4(source[i]);
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h)) = v4v;
            }
        }

        #endregion
    }
}
