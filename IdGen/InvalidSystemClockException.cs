using System;

namespace IdGen
{
    /// <summary>
    /// The exception that is thrown when a clock going backwards is detected.
    /// </summary>
    public class InvalidSystemClockException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSystemClockException"/> class with a message that describes the error.
        /// </summary>
        /// <param name="message">
        /// The message that describes the exception. The caller of this constructor is required to ensure that this 
        /// string has been localized for the current system culture.
        /// </param>
        public InvalidSystemClockException(string message)
            : base(message) { }
    }
}
