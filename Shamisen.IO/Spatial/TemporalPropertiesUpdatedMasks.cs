using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.IO.Spatial
{
    /// <summary>
    /// Represents the masks for updated temporal properties of a spatial audio source.
    /// </summary>
    [Flags]
    public enum TemporalPropertiesUpdatedMasks : uint
    {
        /// <summary>
        /// No properties are updated.
        /// </summary>
        None = 0,
        /// <summary>
        /// The mask for <see cref="SpatialAudioSourceTemporalProperties.Position"/> property.
        /// </summary>
        Position = 1,
        /// <summary>
        /// The mask for <see cref="SpatialAudioSourceTemporalProperties.Gain"/> property.
        /// </summary>
        Gain = 1 << 1,
        /// <summary>
        /// The mask for <see cref="SpatialAudioSourceTemporalProperties.Velocity"/> property.
        /// </summary>
        Velocity = 1 << 2,
        /// <summary>
        /// The mask for <see cref="SpatialAudioSourceTemporalProperties.Pitch"/> property.
        /// </summary>
        Pitch = 1 << 3,
        /// <summary>
        /// The mask for <see cref="SpatialAudioSourceTemporalProperties.Direction"/> property.
        /// </summary>
        Direction = 1 << 4,
        /// <summary>
        /// The mask for <see cref="SpatialAudioSourceTemporalProperties.ConeInnerAngle"/> property.
        /// </summary>
        ConeInnerAngle = 1 << 5,
        /// <summary>
        /// The mask for <see cref="SpatialAudioSourceTemporalProperties.ConeOuterAngle"/> property.
        /// </summary>
        ConeOuterAngle = 1 << 6,
    }
}
