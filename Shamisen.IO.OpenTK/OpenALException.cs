using System;

using OpenTK.Audio.OpenAL.ALC;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Represents errors that occur during OpenAL operation.
    /// </summary>
    /// <seealso cref="Exception" />
    public class OpenALException : Exception
    {
        /// <summary>
        /// The error code.
        /// </summary>
        public ErrorCode ErrorCode { get; init; }
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALException"/> class.
        /// </summary>
        public OpenALException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OpenALException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public OpenALException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Represents errors that occur during OpenAL operation.
    /// </summary>
    /// <seealso cref="Exception" />
    public class OpenALContextException : Exception
    {
        /// <summary>
        /// The error code.
        /// </summary>
        public ErrorCode ErrorCode { get; init; }
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALContextException"/> class.
        /// </summary>
        public OpenALContextException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALContextException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OpenALContextException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALContextException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public OpenALContextException(string message, Exception inner) : base(message, inner) { }
    }
}
