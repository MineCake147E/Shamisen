using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Represents an "extensible" wave format.
    /// </summary>
    public readonly struct ExtensibleWaveFormat
    {
        private readonly StandardWaveFormat format;
        private readonly ushort extensionSize;
        private readonly ushort validBitsPerSample;
        private readonly uint channelMask;
        private readonly Guid subFormat;
    }
}
