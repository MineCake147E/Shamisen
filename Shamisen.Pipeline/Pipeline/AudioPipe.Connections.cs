using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AudioPipe
    {
        /// <summary>
        /// Produces audio data to <see cref="IAudioPipelineConsumer{TSample, TFormat}"/>.
        /// </summary>
        public class Faucet
        {
        }

        /// <summary>
        /// Consumes audio data from <see cref="IAudioPipelineProducer{TSample, TFormat}"/>.
        /// </summary>
        public class Inlet
        {
        }
    }
}
