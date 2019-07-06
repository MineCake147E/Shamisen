using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MonoAudio.IO;

namespace MonoAudio.SoundOut
{
    internal static class PlaybackStateUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CSCore.SoundOut.PlaybackState ConvertPlaybackState(PlaybackState playbackState)
        {
            switch (playbackState)
            {
                case PlaybackState.Stopped:
                    return CSCore.SoundOut.PlaybackState.Stopped;
                case PlaybackState.Playing:
                    return CSCore.SoundOut.PlaybackState.Playing;
                case PlaybackState.Paused:
                    return CSCore.SoundOut.PlaybackState.Paused;
                default:
                    return Unsafe.As<PlaybackState, CSCore.SoundOut.PlaybackState>(ref playbackState);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PlaybackState ConvertPlaybackState(CSCore.SoundOut.PlaybackState playbackState)
        {
            switch (playbackState)
            {
                case CSCore.SoundOut.PlaybackState.Stopped:
                    return PlaybackState.Stopped;
                case CSCore.SoundOut.PlaybackState.Playing:
                    return PlaybackState.Playing;
                case CSCore.SoundOut.PlaybackState.Paused:
                    return PlaybackState.Paused;
                default:
                    return Unsafe.As<CSCore.SoundOut.PlaybackState, PlaybackState>(ref playbackState);
            }
        }
    }
}
