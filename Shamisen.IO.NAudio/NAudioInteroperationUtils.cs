using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using NPlaybackState = NAudio.Wave.PlaybackState;
using NWaveFormat = NAudio.Wave.WaveFormat;
using NDataFlow = NAudio.CoreAudioApi.DataFlow;

namespace Shamisen.IO
{
    /// <summary>
    /// Contains useful methods for interoperating with NAudio.
    /// </summary>
    public static class NAudioInteroperationUtils
    {
        #region Shamisen->NAudio
        /// <summary>
        /// Converts <see cref="PlaybackState"/> instance to <see cref="NPlaybackState"/>.
        /// </summary>
        /// <param name="playbackState">The <see cref="PlaybackState"/> getting converted.</param>
        /// <returns>
        /// The converted <see cref="NPlaybackState"/> instance.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NPlaybackState AsNAudioPlaybackState(this PlaybackState playbackState)
            => playbackState switch
            {
                PlaybackState.Stopped or PlaybackState.NotInitialized => NPlaybackState.Stopped,
                PlaybackState.Playing => NPlaybackState.Playing,
                PlaybackState.Paused => NPlaybackState.Paused,
                _ => Unsafe.As<PlaybackState, NPlaybackState>(ref playbackState),
            };

        #endregion
        #region NAudio->Shamisen
        /// <summary>
        /// Converts <see cref="NPlaybackState"/> instance to <see cref="PlaybackState"/>.
        /// </summary>
        /// <param name="playbackState">The <see cref="NPlaybackState"/> getting converted.</param>
        /// <returns>
        /// The converted <see cref="PlaybackState"/> instance.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PlaybackState AsShamisenPlaybackState(this NPlaybackState playbackState)
            => playbackState switch
            {
                NPlaybackState.Stopped => PlaybackState.Stopped,
                NPlaybackState.Playing => PlaybackState.Playing,
                NPlaybackState.Paused => PlaybackState.Paused,
                _ => Unsafe.As<NPlaybackState, PlaybackState>(ref playbackState),
            };

        /// <summary>
        /// Converts <see cref="NWaveFormat"/> instance to <see cref="WaveFormat"/>.
        /// </summary>
        /// <param name="sourceFormat">The <see cref="WaveFormat"/> getting converted.</param>
        /// <returns>
        /// The converted <see cref="WaveFormat"/> instance.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaveFormat AsShamisenWaveFormat(this NWaveFormat sourceFormat) => new(sourceFormat.SampleRate, sourceFormat.BitsPerSample, sourceFormat.Channels,
                        (AudioEncoding)(short)sourceFormat.Encoding, sourceFormat.ExtraSize);

        /// <summary>
        /// Converts <see cref="NAudio.CoreAudioApi.DataFlow"/> value to <see cref="DataFlow"/>.
        /// </summary>
        /// <param name="sourceFlow">The <see cref="NAudio.CoreAudioApi.DataFlow"/> to convert.</param>
        /// <returns>The converted <see cref="DataFlow"/> value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static DataFlow AsShamisenDataFlow(this NDataFlow sourceFlow) => sourceFlow switch
        {
            NDataFlow.Render => DataFlow.Render,
            NDataFlow.Capture => DataFlow.Capture,
            NDataFlow.All => DataFlow.Render | DataFlow.Capture,
            _ => DataFlow.None,
        };
        #endregion
    }
}
