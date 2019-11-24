﻿using System;
using System.Collections.Generic;
using System.Text;
using static MonoAudio.Units;

namespace MonoAudio
{
    /// <summary>
    /// Provides some utilities for <see cref="IAudioFormat{TSample}"/>.
    /// </summary>
    public static class FormatUtils
    {
        /// <summary>
        /// Gets the value which indicates how long the <see cref="Frame"/> is, in bytes.
        /// </summary>
        /// <typeparam name="TSample">The type of sample.</typeparam>
        /// <param name="format">The format to calculate the length of frame.</param>
        /// <returns><c>sizeof(TSample) * <see cref="IAudioFormat{TSample}.Channels"/></c></returns>
        public static int GetFrameSize<TSample>(this IAudioFormat<TSample> format) => format.Channels * (format.BitDepth / 8);

        /// <summary>
        /// Gets the value which indicates how long the <see cref="byte"/>[] buffer should be.
        /// </summary>
        /// <typeparam name="TSample">The type of the sample.</typeparam>
        /// <param name="format">The format.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static int GetBufferSizeRequired<TSample>(this IAudioFormat<TSample> format, TimeSpan length)
            => (int)Math.Ceiling(format.SampleRate * length.TotalSeconds) * format.Channels * (format.BitDepth / 8);
    }
}
