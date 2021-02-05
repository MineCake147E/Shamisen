using System.Numerics;

using Shamisen.Data;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure that contains skip support of <see cref="IAudioSource{TSample, TFormat}"/> or <see cref="IDataSource{TSample}"/>.
    /// </summary>
    public interface ISkipSupport
    {
        /// <summary>
        /// Skips the source the specified step in frames.
        /// </summary>
        /// <param name="step">The number of frames to skip.</param>
        void Skip(ulong step);
    }
}
