using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Primitives
{
    /// <summary>
    /// Implements a extreme-precision angle counter.
    /// </summary>
    public sealed class AngleCounter
    {
        /// <summary>
        /// Gets the current angle.
        /// </summary>
        /// <value>
        /// The angle.
        /// </value>
        public Angle128 Angle { get; private set; }

        public Angle128 Omega { get; private set; }
    }
}
