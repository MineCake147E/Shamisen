using System;
using MonoAudio.Conversion;

namespace MonoAudio.Filters
{
    /// <summary>
    /// Defines a base infrastructure of an audio filter.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The format of the sample.</typeparam>
    /// <seealso cref="Conversion.IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}" />
    public interface IAudioFilter<TSample, TFormat> : IAudioConverter<TSample, TFormat, TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
    }
}
