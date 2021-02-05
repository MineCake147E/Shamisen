using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen
{
    /// <summary>
    /// Represents an event arguments that tells you that something has (been) stopped and holds why.<br/>
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class StoppedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoppedEventArgs"/> class.
        /// </summary>
        public StoppedEventArgs() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoppedEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public StoppedEventArgs(Exception? exception) => Exception = exception;

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has error; otherwise, <c>false</c>.
        /// </value>
        public bool HasError => !(Exception is null);
    }
}
