﻿using System;

namespace Agitprop.Core.Exceptions;

/// <summary>
/// Exception thrown when content parsing fails.
/// </summary>
public class ContentParserException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentParserException"/> class.
    /// </summary>
    public ContentParserException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentParserException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ContentParserException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentParserException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ContentParserException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
