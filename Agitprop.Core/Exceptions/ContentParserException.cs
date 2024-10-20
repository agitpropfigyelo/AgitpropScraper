using System;

namespace Agitprop.Core.Exceptions;

public class ContentParserException : Exception
{
    public ContentParserException()
    {
    }

    public ContentParserException(string? message) : base(message)
    {
    }

    public ContentParserException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
