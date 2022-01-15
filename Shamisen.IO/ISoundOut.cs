using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base infrastructure of a sound output.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public interface ISoundOut : IDisposable, IPlaybackController
    {
    }
}
