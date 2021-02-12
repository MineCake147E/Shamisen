using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Optimization
{
    /// <summary>
    /// Represents how Shamisen component is allowed to utilize Hardware Intrinsics.
    /// </summary>
    public readonly struct IntrinsicsOptions : IEquatable<IntrinsicsOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntrinsicsOptions"/> struct.
        /// </summary>
        /// <param name="isIntrinsicsEnabled">if set to <c>true</c> [is intrinsics enabled].</param>
        /// <param name="enabledX86Intrinsics">The enabled X86 intrinsics.</param>
        /// <param name="enabledArmIntrinsics">The enabled arm intrinsics.</param>
        public IntrinsicsOptions(bool isIntrinsicsEnabled, X86Intrinsics enabledX86Intrinsics, ArmIntrinsics enabledArmIntrinsics)
        {
            IsIntrinsicsEnabled = isIntrinsicsEnabled;
            EnabledX86Intrinsics = enabledX86Intrinsics;
            EnabledArmIntrinsics = enabledArmIntrinsics;
        }

        /// <summary>
        /// Gets a value indicating whether the intrinsics is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the intrinsics is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIntrinsicsEnabled { get; }

        /// <summary>
        /// Gets the enabled X86 intrinsics.
        /// </summary>
        /// <value>
        /// The enabled X86 intrinsics.
        /// </value>
        public X86Intrinsics EnabledX86Intrinsics { get; }

        /// <summary>
        /// Gets the enabled arm intrinsics.
        /// </summary>
        /// <value>
        /// The enabled arm intrinsics.
        /// </value>
        public ArmIntrinsics EnabledArmIntrinsics { get; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is IntrinsicsOptions options && Equals(options);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(IntrinsicsOptions other) => IsIntrinsicsEnabled == other.IsIntrinsicsEnabled && EnabledX86Intrinsics == other.EnabledX86Intrinsics && EnabledArmIntrinsics == other.EnabledArmIntrinsics;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(IsIntrinsicsEnabled, EnabledX86Intrinsics, EnabledArmIntrinsics);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="IntrinsicsOptions"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="IntrinsicsOptions"/> to compare.</param>
        /// <param name="right">The second <see cref="IntrinsicsOptions"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(IntrinsicsOptions left, IntrinsicsOptions right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="IntrinsicsOptions"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="IntrinsicsOptions"/> to compare.</param>
        /// <param name="right">The second  <see cref="IntrinsicsOptions"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(IntrinsicsOptions left, IntrinsicsOptions right) => !(left == right);
    }
}
