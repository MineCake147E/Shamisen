using System;
using MonoAudio.Formats;

namespace MonoAudio.Conversion.Resampling
{
    /// <summary>
    /// Defines a base infrastructure of an resampler.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The format of the sample.</typeparam>
    /// <seealso cref="MonoAudio.Conversion.IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}" />
    public interface IResampler<TSample, TFormat> : IAudioConverter<TSample, TFormat, TSample, TFormat> where TFormat : IAudioFormat<TSample>
    {
    }
}
