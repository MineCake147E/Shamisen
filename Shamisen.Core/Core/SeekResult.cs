using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Core
{
    /// <summary>
    /// Represents a result of seek operation.
    /// </summary>
    public readonly struct SeekResult
    {
        [Flags]
        private enum SeekError
        {
            None = 0,
            OutOfRange = 1,
        }
    }
}
