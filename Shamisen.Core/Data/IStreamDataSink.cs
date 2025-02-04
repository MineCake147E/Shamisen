using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data
{
    public interface IStreamDataSink<T> : IDisposable, IBufferWriter where T : unmanaged
    {

    }
}
