using System.Runtime.CompilerServices;
using CPlaybackState = CSCore.SoundOut.PlaybackState;

namespace Shamisen.IO
{
    internal static class CSCoreInteroperationUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CPlaybackState ConvertPlaybackState(PlaybackState playbackState)
        {
            switch (playbackState)
            {
                case PlaybackState.Stopped:
                    return CPlaybackState.Stopped;
                case PlaybackState.Playing:
                    return CPlaybackState.Playing;
                case PlaybackState.Paused:
                    return CPlaybackState.Paused;
                default:
                    return Unsafe.As<PlaybackState, CPlaybackState>(ref playbackState);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PlaybackState ConvertPlaybackState(CPlaybackState playbackState)
        {
            switch (playbackState)
            {
                case CPlaybackState.Stopped:
                    return PlaybackState.Stopped;
                case CPlaybackState.Playing:
                    return PlaybackState.Playing;
                case CPlaybackState.Paused:
                    return PlaybackState.Paused;
                default:
                    return Unsafe.As<CPlaybackState, PlaybackState>(ref playbackState);
            }
        }
    }
}
