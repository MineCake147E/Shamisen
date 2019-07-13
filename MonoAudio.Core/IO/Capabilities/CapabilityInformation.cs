using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.IO.Capabilities
{
    /// <summary>
    /// Represents an information of the capability.
    /// </summary>
    /// <typeparam name="T">The type of containing information.</typeparam>
    public readonly struct CapabilityInformation<T>
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="CapabilityInformation{T}"/> is available.
        /// </summary>
        /// <value>
        ///   <c>true</c> if available; otherwise, <c>false</c>.
        /// </value>
        public bool Available { get; }

        /// <summary>
        /// Gets a value indicating whether [needs trial and error].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [needs trial and error]; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsTrialAndError { get; }
    }
}
