﻿using System;

namespace IdGen;

/// <summary>
/// The exception that is thrown when a clock going backwards is detected.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvalidSystemClockException"/> class with a message that describes
/// the error and underlying exception.
/// </remarks>
/// <param name="message">
/// The message that describes the exception. The caller of this constructor is required to ensure that this 
/// string has been localized for the current system culture.
/// </param>
/// <param name="innerException">
/// The exception that is the cause of the current <see cref="InvalidSystemClockException"/>. If the
/// innerException parameter is not null, the current exception is raised in a catch block that handles the
/// inner exception.
/// </param>
public class InvalidSystemClockException(string message, Exception? innerException) : Exception(message, innerException)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSystemClockException"/> class.
    /// </summary>
    public InvalidSystemClockException() : this(Translations.ERR_INVALID_SYSTEM_CLOCK) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSystemClockException"/> class with a message that describes the error.
    /// </summary>
    /// <param name="message">
    /// The message that describes the exception. The caller of this constructor is required to ensure that this 
    /// string has been localized for the current system culture.
    /// </param>
    public InvalidSystemClockException(string message)
        : this(message, null) { }
}
