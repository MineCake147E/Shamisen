﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure of a wave format.
    /// </summary>
    public interface IWaveFormat : IInterleavedAudioFormat<byte>
    {
        /// <summary>
        /// Gets the value indicates how the samples are encoded.
        /// </summary>
        /// <value>
        /// The sample encoding.
        /// </value>
        AudioEncoding Encoding { get; }

        /// <summary>
        /// Gets the extra data.
        /// </summary>
        /// <value>
        /// The extra data.
        /// </value>
        ReadOnlyMemory<byte> ExtraData { get; }
    }
}
