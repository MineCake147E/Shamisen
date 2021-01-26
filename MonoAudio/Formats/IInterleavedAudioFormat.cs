using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base structure of audio formats.<br/>
    /// <typeparamref name="TSample"/> must not be affected by the number of <see cref="IAudioFormat{TSample}.Channels"/>.
    /// </summary>
    public interface IInterleavedAudioFormat<TSample> : IAudioFormat<TSample>
        where TSample : unmanaged
    {
        /// <summary>
        /// Gets the value indicates how many <typeparamref name="TSample"/>s are required per whole frame.<br/>
        /// It depends on <see cref="IAudioFormat{TSample}.Channels"/>.
        /// </summary>
        /// <value>
        /// The size of frame.
        /// </value>
        int BlockSize { [MethodImpl(MethodImplOptions.AggressiveInlining)]get; }

        /// <summary>
        /// Gets the value indicates how many <typeparamref name="TSample"/>s are required per 1-channel sample.<br/>
        /// Does not depend on the number of <see cref="IAudioFormat{TSample}.Channels"/>.<br/>
        /// </summary>
        /// <value>
        /// The size of a sample in <typeparamref name="TSample"/>s.
        /// </value>
        int SampleSize { get; }
    }
}
