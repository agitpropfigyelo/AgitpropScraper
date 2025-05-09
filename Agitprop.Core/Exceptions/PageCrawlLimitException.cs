namespace Agitprop.Core.Exceptions;

/// <summary>
/// Exception thrown when the page crawl limit is exceeded.
/// </summary>
[Serializable]
internal class PageCrawlLimitException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageCrawlLimitException"/> class.
    /// </summary>
    public PageCrawlLimitException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageCrawlLimitException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public PageCrawlLimitException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageCrawlLimitException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PageCrawlLimitException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets or sets the page crawl limit that was exceeded.
    /// </summary>
    public int PageCrawlLimit { get; set; }
}
