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
        /// <summary>
        /// Initializes the <see cref="ISoundOut"/> for playing a <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to play.</param>
        void Initialize(IWaveSource source);

    }
}
