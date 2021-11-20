using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Defines a base infrastructure of a processor being managed by <see cref="AudioPipelineProcessorComponent{TSample, TFormat}"/>.
    /// </summary>
    /// <typeparam name="TSample"></typeparam>
    /// <typeparam name="TFormat"></typeparam>
    public interface IAudioPipelineProcessor<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets the source block size required to produce certain number of frames specified by <paramref name="outputBlockSize"/>.<br/>
        /// This function is getting called each time before <see cref="AudioPipelineProcessorComponent{TSample, TFormat}"/> tries to read the source samples.<br/>
        /// e.g. The SplineResampler-based processor can return variable results even if the <paramref name="outputBlockSize"/> is same at all time.
        /// </summary>
        /// <param name="outputBlockSize">The required size of output block, in frames.</param>
        /// <returns>The number of frames required to produce certain number of frames specified by <paramref name="outputBlockSize"/></returns>
        int GetRequiredSourceBlockSize(int outputBlockSize);
        /// <summary>
        /// Processes the <paramref name="source"/> block and write to <paramref name="output"/>.
        /// </summary>
        /// <param name="output">The output block.</param>
        /// <param name="source">The source block.</param>
        void Process(Span<TSample> output, ReadOnlySpan<TSample> source);
    }
}
