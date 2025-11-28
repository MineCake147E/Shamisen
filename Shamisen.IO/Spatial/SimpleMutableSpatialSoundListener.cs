using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Mathematics;

namespace Shamisen.IO.Spatial
{
    /// <summary>
    /// Represents a simple mutable implementation of <see cref="ISpatialSoundListener"/>.
    /// </summary>
    public struct SimpleMutableSpatialSoundListener : ISpatialSoundListener
    {
        /// <inheritdoc/>
        public Vector3 Position { get; set; }
        /// <inheritdoc/>
        public float Gain { get; set; }
        /// <inheritdoc/>
        public Vector3 Velocity { get; set; }
        /// <inheritdoc/>
        public OrientationVectorPair Orientation { get; set; }
    }
}
