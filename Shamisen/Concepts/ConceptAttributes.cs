using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Concepts
{
    /// <summary>
    /// Represents a kind of concepts.
    /// </summary>
    public enum ConceptKind
    {
        /// <summary>
        /// Invalid.
        /// </summary>
        None,

        /// <summary>
        /// The concept of Units.
        /// </summary>
        Unit,
    }

    /// <summary>
    /// Defines a concept.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class ConceptAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ConceptAttribute"/>.
        /// </summary>
        /// <param name="kind">The kind of concept.</param>
        public ConceptAttribute(ConceptKind kind) => Kind = kind;

        /// <summary>
        /// The kind of concept.
        /// </summary>
        public ConceptKind Kind { get; }
    }
}
