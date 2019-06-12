using System;
namespace MonoAudio.Conversion.Resampling
{
    public interface IResampler<TSample> : IAudioConverter<TSample, TSample>
    {
    }
}
