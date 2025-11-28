using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Filters;

namespace Shamisen.IO.Spatial
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISpatialSoundOut<TSource, TSample, TFormat, TListener> : IMultipleSourceAudioTarget<TSource, TSample, TFormat>
        where TSource : IReadableAudioSource<TSample, TFormat>, ISpatializedAudioSource, IEquatable<TSource>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
        where TListener : ISpatialSoundListener
    {
        /// <summary>
        /// Gets or sets the listener of this spatial sound output.
        /// </summary>
        TListener Listener { get; init; }
    }
}
