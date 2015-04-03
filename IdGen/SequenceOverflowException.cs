using System;

namespace IdGen
{
    /// <summary>
    /// The exception that is thrown when a sequence overflows (e.g. too many Id's generated within the same timespan (ms)).
    /// </summary>
    public class SequenceOverflowException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceOverflowException"/> class with a message that describes the error.
        /// </summary>
        /// <param name="message">
        /// The message that describes the exception. The caller of this constructor is required to ensure that this 
        /// string has been localized for the current system culture.
        /// </param>
        public SequenceOverflowException(string message)
            : base(message) { }
    }
}
