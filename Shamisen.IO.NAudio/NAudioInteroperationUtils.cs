using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using NPlaybackState = NAudio.Wave.PlaybackState;

namespace Shamisen.IO
{
    internal static class NAudioInteroperationUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static NPlaybackState ConvertPlaybackState(PlaybackState playbackState)
            => playbackState switch
            {
                PlaybackState.Stopped or PlaybackState.NotInitialized => NPlaybackState.Stopped,
                PlaybackState.Playing => NPlaybackState.Playing,
                PlaybackState.Paused => NPlaybackState.Paused,
                _ => Unsafe.As<PlaybackState, NPlaybackState>(ref playbackState),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PlaybackState ConvertPlaybackState(NPlaybackState playbackState)
            => playbackState switch
            {
                NPlaybackState.Stopped => PlaybackState.Stopped,
                NPlaybackState.Playing => PlaybackState.Playing,
                NPlaybackState.Paused => PlaybackState.Paused,
                _ => Unsafe.As<NPlaybackState, PlaybackState>(ref playbackState),
            };
    }
}
