using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Swaps fixed-sized buffers for audio pipeline components.
    /// </summary>
    /// <typeparam name="TSample"></typeparam>
    /// <typeparam name="TFormat"></typeparam>
    public sealed class BufferSwapper<TSample, TFormat>
         where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {

    }
}
