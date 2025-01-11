namespace Agitprop.Core;

public class PageAlreadyVisitedException : Exception
{
    public PageAlreadyVisitedException()
    {
    }

    public PageAlreadyVisitedException(string? message) : base(message)
    {
    }

    public PageAlreadyVisitedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
