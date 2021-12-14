using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure for writing audio data.
    /// </summary>
    /// <typeparam name="TSample">The type of sample.</typeparam>
    /// <typeparam name="TFormat">The type of audio format.</typeparam>
    public interface IAudioSink<TSample, out TFormat> : IDisposable, IWriteSupport<TSample> where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        TFormat Format { get; }
    }
}
