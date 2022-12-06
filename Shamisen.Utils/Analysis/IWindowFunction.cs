using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Analysis
{
    /// <summary>
    /// Defines a base infrastructure of a Window function for FFT.
    /// </summary>
    public interface IWindowFunction
    {
        /// <summary>
        /// Generates a cache for this <see cref="IWindowFunction"/>.
        /// </summary>
        /// <param name="destination">The destination.</param>
        void Generate(Span<float> destination);

        /// <summary>
        /// Generates a cache for this <see cref="IWindowFunction"/>.
        /// </summary>
        /// <param name="destination">The destination.</param>
        void Generate(Span<double> destination);
    }
}
