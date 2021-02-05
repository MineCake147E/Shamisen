using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

namespace Shamisen.Data
{
    /// <summary>
    /// The exception that is thrown when a reader of <see cref="IDataSource{TSample}"/> has ran out of buffered data.
    /// </summary>
    /// <seealso cref="IOException" />
    [Serializable]
    public class BufferingException : IOException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingException"/> class.
        /// </summary>
        public BufferingException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingException"/> class.
        /// </summary>
        /// <param name="message">A <see cref="string"></see> that describes the error. The content of message is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
        public BufferingException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public BufferingException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingException"/> class.
        /// </summary>
        /// <param name="info">The data for serializing or deserializing the object.</param>
        /// <param name="context">The source and destination for the object.</param>
        protected BufferingException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}
