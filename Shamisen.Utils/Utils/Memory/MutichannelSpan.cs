using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Memory
{
    /// <summary>
    /// Represents a tuple of multiple contiguous memory regions of the same length.
    /// </summary>
    public readonly ref struct MutichannelSpan<T> where T : unmanaged
    {
        private readonly ref T head;
    }
}
