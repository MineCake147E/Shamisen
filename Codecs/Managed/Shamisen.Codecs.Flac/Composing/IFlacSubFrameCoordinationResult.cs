using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Composing
{
    internal interface IFlacSubFrameCoordinationResult
    {
        byte SubFrameType { get; }

        public FlacSubFrameKind SubFrameKind
        {
            get
            {
                var type = SubFrameType;
                return type switch
                {
                    < 2 => (FlacSubFrameKind)type,
                    < 0b1000 => FlacSubFrameKind.Unknown,
                    < 0b1101 => FlacSubFrameKind.Fixed,
                    < 0b100000 => FlacSubFrameKind.Unknown,
                    <= 0b111111 => FlacSubFrameKind.LinearPrediction,
                    _ => FlacSubFrameKind.Unknown
                };
            }
        }

        public int Order
        {
            get
            {
                var type = SubFrameType;
                return type switch
                {
                    < 0b1000 => -1,
                    < 0b1101 => type & 0x7,
                    < 0b100000 => -1,
                    <= 0b111111 => (type & 0x1f) + 1,
                    _ => -1
                };
            }
        }

        /// <summary>
        /// The total stored bits per sample. Formula: Size - Wasted
        /// </summary>
        byte StoredBitsPerSample { get; }

        /// <summary>
        /// The block's size for best compression.<br/>
        /// Negative value indicates it's best to have least possible samples for maximum compression, which means upcoming samples cannot be compressed at all for -<see cref="BestCompressionBlockSize"/> samples.
        /// </summary>
        int BestCompressionBlockSize { get; }

        /// <summary>
        /// The block's size with maximum compromise possible without altering <see cref="SubFrameType"/>.<br/>
        /// The value indicates there's no more possibility for extending block size without altering <see cref="SubFrameType"/> for some reasons like:<br/>
        /// - The <see cref="SubFrameKind"/> is <see cref="FlacSubFrameKind.Constant"/>, and the next sample is not the same value with previous samples.<br/>
        /// - The <see cref="SubFrameKind"/> is either <see cref="FlacSubFrameKind.Fixed"/> or <see cref="FlacSubFrameKind.LinearPrediction"/>, and the next sample exceeds limit of calculating intermediate value of linear prediction.
        /// </summary>
        int MaximumBlockSize { get; }
    }
}
