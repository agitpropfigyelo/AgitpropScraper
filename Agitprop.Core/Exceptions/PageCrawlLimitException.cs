namespace Agitprop.Core.Exceptions;

[Serializable]
internal class PageCrawlLimitException : Exception
{
    public PageCrawlLimitException()
    {
    }

    public PageCrawlLimitException(string? message) : base(message)
    {
    }

    public PageCrawlLimitException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public int PageCrawlLimit { get; set; }
}