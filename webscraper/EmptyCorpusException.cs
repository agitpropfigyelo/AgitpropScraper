namespace webscraper;
using System;

public class EmptyCorpusException : Exception
{
    public EmptyCorpusException()
    {
    }

    public EmptyCorpusException(string message)
        : base(message)
    {
    }

    public EmptyCorpusException(string message, Exception inner)
        : base(message, inner)
    {
    }
}