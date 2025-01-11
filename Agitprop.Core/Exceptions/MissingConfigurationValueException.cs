using System;

namespace Agitprop.Core.Exceptions;

public class MissingConfigurationValueException : Exception
{
    public MissingConfigurationValueException()
    {
    }

    public MissingConfigurationValueException(string? message) : base(message)
    {
    }

    public MissingConfigurationValueException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
