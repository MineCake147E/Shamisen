using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Composing
{
    internal interface IFlacSubFrameCoordinator
    {
        IFlacSubFrameCoordinationResult FindBestSubFrameConfiguration(Span<int> samples);

    }
}
