using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Mathematics
{
    /// <summary>
    /// Represents the orientation of a listener in 3D space.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct OrientationVectorPair : IEquatable<OrientationVectorPair>
    {
        /// <summary>
        /// Gets the forward direction of the listener.<br/>
        /// </summary>
        public Vector3 Forward { get; }

        /// <summary>
        /// Gets the upper direction of the listener.<br/>
        /// </summary>
        public Vector3 Up { get; }

        /// <summary>
        /// Creates a new instance of <see cref="OrientationVectorPair"/>.
        /// </summary>
        /// <param name="forward">The forward direction vector of the listener.</param>
        /// <param name="up">The upper direction vector of the listener.</param>
        internal OrientationVectorPair(Vector3 forward, Vector3 up)
        {
            Forward = forward;
            Up = up;
        }

        /// <summary>
        /// The default listener orientation, facing towards negative Z and with positive Y as up direction.
        /// </summary>
        public static OrientationVectorPair Default => new(-Vector3.UnitZ, Vector3.UnitY);

        /// <summary>
        /// Creates a new instance of <see cref="OrientationVectorPair"/> from pre-normalized direction vectors.
        /// </summary>
        /// <param name="forward">The forward direction vector of the listener.</param>
        /// <param name="up">The upper direction vector of the listener.</param>
        /// <returns>The newly created orientation.</returns>
        public static OrientationVectorPair CreateFromPreNormalized(Vector3 forward, Vector3 up) => new(forward, up);

        /// <summary>
        /// Creates a new instance of <see cref="OrientationVectorPair"/> from direction vectors.
        /// </summary>
        /// <param name="forward">The forward direction vector of the listener.</param>
        /// <param name="up">The upper direction vector of the listener.</param>
        /// <returns>The newly created orientation.</returns>
        public static OrientationVectorPair CreateFromDirection(Vector3 forward, Vector3 up) => new(Vector3.Normalize(forward), Vector3.Normalize(up));

        /// <summary>
        /// Creates a new instance of <see cref="OrientationVectorPair"/> from a quaternion representing the listener's orientation.
        /// </summary>
        /// <param name="quaternion">The quaternion representing the listener's orientation.</param>
        /// <returns>The newly created orientation.</returns>
        public static OrientationVectorPair CreateFromQuaternion(Quaternion quaternion) => new(Vector3.Transform(-Vector3.UnitZ, quaternion), Vector3.Transform(Vector3.UnitY, quaternion));

        /// <summary>
        /// Creates a new instance of <see cref="OrientationVectorPair"/> from a rotation matrix representing the listener's orientation.
        /// </summary>
        /// <param name="matrix">The rotation matrix representing the listener's orientation.</param>
        /// <returns>The newly created orientation.</returns>
        public static OrientationVectorPair CreateFromRotationMatrix(Matrix4x4 matrix) => new(Vector3.Transform(-Vector3.UnitZ, matrix), Vector3.Transform(Vector3.UnitY, matrix));

        /// <summary>
        /// Transforms the given <see cref="OrientationVectorPair"/> by the specified quaternion.
        /// </summary>
        /// <param name="orientation">The original orientation.</param>
        /// <param name="quaternion">The quaternion representing the desired rotation.</param>
        /// <returns>The newly created orientation.</returns>
        public static OrientationVectorPair Transform(OrientationVectorPair orientation, Quaternion quaternion)
            => new(Vector3.Transform(orientation.Forward, quaternion), Vector3.Transform(orientation.Up, quaternion));

        /// <summary>
        /// Transforms the given <see cref="OrientationVectorPair"/> by the specified matrix.
        /// </summary>
        /// <param name="orientation">The original orientation.</param>
        /// <param name="matrix">The matrix representing the desired rotation.</param>
        /// <returns>The newly created orientation.</returns>
        public static OrientationVectorPair Transform(OrientationVectorPair orientation, Matrix4x4 matrix)
            => new(Vector3.Transform(orientation.Forward, matrix), Vector3.Transform(orientation.Up, matrix));

        /// <summary>
        /// Converts the given <see cref="OrientationVectorPair"/> to a rotation matrix.
        /// </summary>
        /// <param name="orientation">The original orientation.</param>
        /// <returns>The view matrix.</returns>
        public static Matrix4x4 ToMatrix(OrientationVectorPair orientation) => Matrix4x4.CreateLookTo(Vector3.Zero, orientation.Forward, orientation.Up);

        /// <summary>
        /// Converts the given <see cref="OrientationVectorPair"/> to a rotation matrix.
        /// </summary>
        /// <param name="orientation">The original orientation.</param>
        /// <returns>The view matrix.</returns>
        public static Matrix4x4 ToMatrixLeftHanded(OrientationVectorPair orientation) => Matrix4x4.CreateLookToLeftHanded(Vector3.Zero, orientation.Forward, orientation.Up);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is OrientationVectorPair orientation && Equals(orientation);
        /// <inheritdoc/>
        public bool Equals(OrientationVectorPair other) => Forward.Equals(other.Forward) && Up.Equals(other.Up);
        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Forward, Up);

        /// <summary>
        /// Determines whether two <see cref="OrientationVectorPair"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="OrientationVectorPair"/> instance to compare.</param>
        /// <param name="right">The second <see cref="OrientationVectorPair"/> instance to compare.</param>
        /// <returns><see langword="true"/> if the two instances are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(OrientationVectorPair left, OrientationVectorPair right) => left.Equals(right);
        /// <summary>
        /// Determines whether two <see cref="OrientationVectorPair"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="OrientationVectorPair"/> instance to compare.</param>
        /// <param name="right">The second <see cref="OrientationVectorPair"/> instance to compare.</param>
        /// <returns><see langword="true"/> if the two instances are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(OrientationVectorPair left, OrientationVectorPair right) => !(left == right);
    }
}
