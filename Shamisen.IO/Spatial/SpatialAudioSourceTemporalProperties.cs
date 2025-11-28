using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.IO.Spatial
{
    /// <summary>
    /// Represents the temporal properties of a spatial audio source.<br/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SpatialAudioSourceTemporalProperties : IEquatable<SpatialAudioSourceTemporalProperties>
    {
        internal readonly Vector4 positionAndGain;
        internal readonly Vector4 velocityAndPitch;
        internal readonly Vector4 directionAndOuterGain;
        internal readonly Vector2 coneAngles;

        /// <summary>
        /// Gets the current position of the audio source in meters.<br/>
        /// Positive X values are to the right of the listener and negative X values are to the left.<br/>
        /// Positive Y values are above the listener and negative Y values are below.<br/>
        /// Positive Z values are behind of the listener and negative Z values are in front.
        /// </summary>
        public Vector3 Position => positionAndGain.AsVector3();

        /// <summary>
        /// Gets the current reference gain (volume) of the audio source.<br/>
        /// Binding Implementations that have no native support for this value should emulate the effect themselves.
        /// </summary>
        public float Gain => positionAndGain.W;

        /// <summary>
        /// Gets the current velocity of the audio source in meters per second.<br/>
        /// Some implementations, like OpenAL bindings, may use this value to apply Doppler effect.<br/>
        /// Binding Implementations that have no native support for this value may ignore this value.<br/>
        /// Positive X values are to the right of the listener and negative X values are to the left.<br/>
        /// Positive Y values are above the listener and negative Y values are below.<br/>
        /// Positive Z values are behind of the listener and negative Z values are in front.
        /// </summary>
        public Vector3 Velocity => velocityAndPitch.AsVector3();

        /// <summary>
        /// Gets the current playback speed multiplier of the audio source.<br/>
        /// Binding Implementations that have no native support for this value should emulate the effect themselves.
        /// </summary>
        public float Pitch => velocityAndPitch.W;

        /// <summary>
        /// Gets the current direction of the audio source in meters.<br/>
        /// Some implementations, like OpenAL bindings, may use this value to apply directional audio effects.<br/>
        /// Binding Implementations that have no native support for this value may ignore this value.<br/>
        /// Positive X values are to the right of the listener and negative X values are to the left.<br/>
        /// Positive Y values are above the listener and negative Y values are below.<br/>
        /// Positive Z values are behind of the listener and negative Z values are in front.
        /// </summary>
        public Vector3 Direction => directionAndOuterGain.AsVector3();

        /// <summary>
        /// Gets the current gain outside of the sound cone of the audio source.<br/>
        /// Some implementations, like OpenAL bindings, may use this value to apply directional audio effects.<br/>
        /// Binding Implementations that have no native support for this value may ignore this value.
        /// </summary>
        public float ConeOuterGain => directionAndOuterGain.W;

        /// <summary>
        /// Gets the current inside angle of the sound cone of the audio source, in degrees.<br/>
        /// Some implementations, like OpenAL bindings, may use this value to apply directional audio effects.<br/>
        /// Binding Implementations that have no native support for this value may ignore this value.
        /// </summary>
        public float ConeInnerAngle => coneAngles.X;

        /// <summary>
        /// Gets the current outside angle of the sound cone of the audio source, in degrees.<br/>
        /// Some implementations, like OpenAL bindings, may use this value to apply directional audio effects.<br/>
        /// Binding Implementations that have no native support for this value may ignore this value.
        /// </summary>
        public float ConeOuterAngle => coneAngles.Y;

        /// <summary>
        /// The default gain value.
        /// </summary>
        public const float DefaultGain = 1f;

        /// <summary>
        /// The default pitch value.
        /// </summary>
        public const float DefaultPitch = 1f;

        /// <summary>
        /// The default value for <see cref="ConeInnerAngle"/> and <see cref="ConeOuterAngle"/> properties.
        /// </summary>
        public const float DefaultConeAngle = 360.0f;

        internal SpatialAudioSourceTemporalProperties(Vector4 positionAndGain, Vector4 velocityAndPitch, Vector4 directionAndOuterGain, Vector2 coneAngles)
        {
            this.positionAndGain = positionAndGain;
            this.velocityAndPitch = velocityAndPitch;
            this.directionAndOuterGain = directionAndOuterGain;
            this.coneAngles = coneAngles;
        }

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/>.
        /// <paramref name="coneInnerAngle"/>
        /// </summary>
        /// <param name="position">The current position of the audio source.</param>
        /// <param name="gain">The current gain of the audio source.</param>
        /// <param name="velocity">The current velocity of the audio source.</param>
        /// <param name="pitch">The current pitch of the audio source.</param>
        /// <param name="direction">The current direction of the audio source.</param>
        /// <param name="coneOuterGain">The current gain outside of the sound cone of the audio source.</param>
        /// <param name="coneInnerAngle">The current inside angle of the sound cone of the audio source.</param>
        /// <param name="coneOuterAngle">The current outside angle of the sound cone of the audio source.</param>
        public SpatialAudioSourceTemporalProperties(Vector3 position, float gain, Vector3 velocity, float pitch, Vector3 direction, float coneOuterGain, float coneInnerAngle, float coneOuterAngle)
        {
            positionAndGain = new Vector4(position, gain);
            velocityAndPitch = new Vector4(velocity, pitch);
            directionAndOuterGain = new Vector4(direction, coneOuterGain);
            coneAngles = new Vector2(coneInnerAngle, coneOuterAngle);
        }

        /// <summary>
        /// The default temporal properties of a spatial audio source.
        /// </summary>
        public static SpatialAudioSourceTemporalProperties Default => new(Vector4.UnitW, Vector4.UnitW, Vector4.UnitW, new Vector2(DefaultConeAngle));

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> from position only, with other properties set to their default values.
        /// </summary>
        /// <param name="position">The current position of the audio source.</param>
        /// <returns>The created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public static SpatialAudioSourceTemporalProperties FromPosition(Vector3 position) => new(new Vector4(position, DefaultGain), Vector4.UnitW, Vector4.UnitW, new Vector2(DefaultConeAngle));

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> from position and gain, with other properties set to their default values.
        /// </summary>
        /// <param name="position">The current position of the audio source.</param>
        /// <param name="gain">The current gain of the audio source.</param>
        /// <returns>The created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public static SpatialAudioSourceTemporalProperties FromPositionAndGain(Vector3 position, float gain) => new(new Vector4(position, gain), Vector4.UnitW, Vector4.UnitW, new Vector2(DefaultConeAngle));

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified position, keeping other properties unchanged.
        /// </summary>
        /// <param name="position">The new position of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithPosition(Vector3 position) => new(new Vector4(position, Gain), velocityAndPitch, directionAndOuterGain, coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified gain, keeping other properties unchanged.
        /// </summary>
        /// <param name="gain">The new gain of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithGain(float gain) => new(new Vector4(positionAndGain.AsVector3(), gain), velocityAndPitch, directionAndOuterGain, coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified position and gain, keeping other properties unchanged.
        /// </summary>
        /// <param name="position">The new position of the audio source.</param>
        /// <param name="gain">The new gain of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithPositionAndGain(Vector3 position, float gain) => new(new Vector4(position, gain), velocityAndPitch, directionAndOuterGain, coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified position and gain, keeping other properties unchanged.
        /// </summary>
        /// <param name="positionAndGain">The new position(XYZ) and gain(W) of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithPositionAndGain(Vector4 positionAndGain) => new(positionAndGain, velocityAndPitch, directionAndOuterGain, coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified velocity, keeping other properties unchanged.
        /// </summary>
        /// <param name="velocity">The new velocity of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithVelocity(Vector3 velocity) => new(positionAndGain, new Vector4(velocity, Pitch), directionAndOuterGain, coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified pitch, keeping other properties unchanged.
        /// </summary>
        /// <param name="pitch">The new pitch of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithPitch(float pitch) => new(positionAndGain, new Vector4(velocityAndPitch.AsVector3(), pitch), directionAndOuterGain, coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified velocity and pitch, keeping other properties unchanged.
        /// </summary>
        /// <param name="velocity">The new velocity of the audio source.</param>
        /// <param name="pitch">The new pitch of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithVelocityAndPitch(Vector3 velocity, float pitch) => new(positionAndGain, new Vector4(velocity, pitch), directionAndOuterGain, coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified velocity and pitch, keeping other properties unchanged.
        /// </summary>
        /// <param name="velocityAndPitch">The new velocity(XYZ) and pitch(W) of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithVelocityAndPitch(Vector4 velocityAndPitch) => new(positionAndGain, velocityAndPitch, directionAndOuterGain, coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified direction, keeping other properties unchanged.
        /// </summary>
        /// <param name="direction">The new direction of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithDirection(Vector3 direction) => new(positionAndGain, velocityAndPitch, new Vector4(direction, ConeOuterGain), coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified outer gain, keeping other properties unchanged.
        /// </summary>
        /// <param name="coneOuterGain">The new gain outside of the sound cone of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithOuterGain(float coneOuterGain) => new(positionAndGain, velocityAndPitch, new Vector4(directionAndOuterGain.AsVector3(), coneOuterGain), coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified direction and outer gain, keeping other properties unchanged.
        /// </summary>
        /// <param name="direction">The new direction of the audio source.</param>
        /// <param name="coneOuterGain">The new gain outside of the sound cone of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithDirectionAndOuterGain(Vector3 direction, float coneOuterGain) => new(positionAndGain, velocityAndPitch, new Vector4(direction, coneOuterGain), coneAngles);

        /// <summary>
        /// Creates a new instance of <see cref="SpatialAudioSourceTemporalProperties"/> with the specified direction and outer gain, keeping other properties unchanged.
        /// </summary>
        /// <param name="directionAndOuterGain">The new direction(XYZ) and gain(W) outside of the sound cone of the audio source.</param>
        /// <returns>The newly created <see cref="SpatialAudioSourceTemporalProperties"/>.</returns>
        public readonly SpatialAudioSourceTemporalProperties WithDirectionAndOuterGain(Vector4 directionAndOuterGain) => new(positionAndGain, velocityAndPitch, directionAndOuterGain, coneAngles);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpatialAudioSourceTemporalProperties properties && Equals(properties);

        /// <inheritdoc/>
        public bool Equals(SpatialAudioSourceTemporalProperties other) => positionAndGain.Equals(other.positionAndGain) && velocityAndPitch.Equals(other.velocityAndPitch) && directionAndOuterGain.Equals(other.directionAndOuterGain) && coneAngles.Equals(other.coneAngles);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(positionAndGain, velocityAndPitch, directionAndOuterGain, coneAngles);

        /// <summary>
        /// Determines whether two <see cref="SpatialAudioSourceTemporalProperties"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="SpatialAudioSourceTemporalProperties"/> instance to compare.</param>
        /// <param name="right">The second <see cref="SpatialAudioSourceTemporalProperties"/> instance to compare.</param>
        /// <returns><see langword="true"/> if the two instances are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(SpatialAudioSourceTemporalProperties left, SpatialAudioSourceTemporalProperties right) => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="SpatialAudioSourceTemporalProperties"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="SpatialAudioSourceTemporalProperties"/> instance to compare.</param>
        /// <param name="right">The second <see cref="SpatialAudioSourceTemporalProperties"/> instance to compare.</param>
        /// <returns><see langword="true"/> if the two instances are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(SpatialAudioSourceTemporalProperties left, SpatialAudioSourceTemporalProperties right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CompareAndConvertToMasks012And3(Vector128<float> old, Vector128<float> value)
        {
            var vecResult = ~Vector128.Equals(old, value);
            var mask = vecResult.ExtractMostSignificantBits();
            var mh = (mask & 0b1000) >> 2;
            return mh | (Unsafe.BitCast<bool, byte>((mask & 0b0000_0111) > 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CompareAndConvertToMasks0And1(Vector128<float> old, Vector128<float> value)
        {
            var vecResult = ~Vector128.Equals(old, value);
            var mask = vecResult.ExtractMostSignificantBits();
            return mask & 0b11;
        }

        /// <summary>
        /// Compares two <see cref="SpatialAudioSourceTemporalProperties"/> instances and returns the masks of the properties that have been updated.
        /// </summary>
        /// <param name="old">The reference to the old value of <see cref="SpatialAudioSourceTemporalProperties"/>.</param>
        /// <param name="value">The reference to the new value of <see cref="SpatialAudioSourceTemporalProperties"/>.</param>
        /// <returns>The masks of the properties that have been updated.</returns>
        public static TemporalPropertiesUpdatedMasks Compare(scoped ref readonly SpatialAudioSourceTemporalProperties old, scoped ref readonly SpatialAudioSourceTemporalProperties value)
        {
            var result = TemporalPropertiesUpdatedMasks.None;
            result |= (TemporalPropertiesUpdatedMasks)CompareAndConvertToMasks012And3(old.positionAndGain.AsVector128(), value.positionAndGain.AsVector128());
            result |= (TemporalPropertiesUpdatedMasks)(CompareAndConvertToMasks012And3(old.velocityAndPitch.AsVector128(), value.velocityAndPitch.AsVector128()) << 2);
            result |= (TemporalPropertiesUpdatedMasks)(CompareAndConvertToMasks012And3(old.directionAndOuterGain.AsVector128(), value.directionAndOuterGain.AsVector128()) << 4);
            result |= (TemporalPropertiesUpdatedMasks)(CompareAndConvertToMasks0And1(old.coneAngles.AsVector128(), value.coneAngles.AsVector128()) << 6);
            return result;
        }
    }
}
