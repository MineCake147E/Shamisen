using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Filters.BiQuad.Cascaded
{
    /// <summary>
    /// Represents a set of parameters cascaded in single <see cref="CascadedBiQuadFilter"/> in array-of-structure form.
    /// </summary>
    public readonly struct CascadedBiQuadParameters
    {
        private readonly BiQuadParameter[,] parameters;

        /// <summary>
        /// Initializes a new instance of <see cref="CascadedBiQuadParameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters arranged in [channels, order].</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CascadedBiQuadParameters(BiQuadParameter[,] parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);
            this.parameters = parameters;
        }

        /// <summary>
        /// Gets the number of channels associated with this <see cref="CascadedBiQuadParameters"/>.
        /// </summary>
        public int Channels => parameters.GetLength(0);
        /// <summary>
        /// Gets the number of cascaded filters of this <see cref="CascadedBiQuadParameters"/>.
        /// </summary>
        public int Order => parameters.GetLength(1);

        /// <summary>
        /// Gets the number of <see cref="BiQuadParameter"/>s in this <see cref="CascadedBiQuadParameters"/>.
        /// </summary>
        public int Length => parameters.Length;
    }
}
