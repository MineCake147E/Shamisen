using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Transfers audio buffers between two components.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public sealed class AudioPipe<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
    }
}
