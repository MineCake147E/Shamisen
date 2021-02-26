using System;

namespace Shamisen.IO
{
    /// <summary>
    /// Represents errors that occur during OpenAL operation.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class OpenALException : Exception
    {
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

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected OpenALException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
