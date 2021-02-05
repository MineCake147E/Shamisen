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
        {
            switch (playbackState)
            {
                case PlaybackState.Stopped:
                    return NPlaybackState.Stopped;
                case PlaybackState.Playing:
                    return NPlaybackState.Playing;
                case PlaybackState.Paused:
                    return NPlaybackState.Paused;
                default:
                    return Unsafe.As<PlaybackState, NPlaybackState>(ref playbackState);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PlaybackState ConvertPlaybackState(NPlaybackState playbackState)
        {
            switch (playbackState)
            {
                case NPlaybackState.Stopped:
                    return PlaybackState.Stopped;
                case NPlaybackState.Playing:
                    return PlaybackState.Playing;
                case NPlaybackState.Paused:
                    return PlaybackState.Paused;
                default:
                    return Unsafe.As<NPlaybackState, PlaybackState>(ref playbackState);
            }
        }
    }
}
