namespace webscraper;
using System;

public class NoScraperFunctionException : Exception
{
    public NoScraperFunctionException()
    {
    }

    public NoScraperFunctionException(string message)
        : base(message)
    {
    }

    public NoScraperFunctionException(string message, Exception inner)
        : base(message, inner)
    {
    }
}